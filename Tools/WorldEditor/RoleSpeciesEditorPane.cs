using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace TerminalHyperspace.WorldEditor;

/// One pane shape for both Role and Species — they share an identical model.
/// The active entity type comes from whichever Load{Roles|Species}() was called.
public partial class MainWindow
{
    /// Canonical attribute names in the order used by the runtime AttributeType enum.
    private static readonly string[] AllAttributes =
        { "Dexterity", "Knowledge", "Mechanical", "Perception", "Strength", "Technical", "Force" };

    // Role pane state
    private RoleSpeciesFileParser? _roleParser;
    private List<RoleSpeciesModel> _roles = new();
    private RoleSpeciesModel? _selectedRole;
    private ListBox? _roleList;
    private TextBox? _roleFilter, _roleName, _roleDesc;
    private StackPanel? _roleAttrsRows, _roleSkillsRows;
    private bool _roleBuilt, _roleSync;

    // Species pane state (mirror of Role)
    private RoleSpeciesFileParser? _speciesParser;
    private List<RoleSpeciesModel> _species = new();
    private RoleSpeciesModel? _selectedSpecies;
    private ListBox? _speciesList;
    private TextBox? _speciesFilter, _speciesName, _speciesDesc;
    private StackPanel? _speciesAttrsRows, _speciesSkillsRows;
    private bool _speciesBuilt, _speciesSync;

    // ---------- Role ----------

    private void BuildRoleViewIfNeeded()
    {
        if (_roleBuilt) return;
        _roleBuilt = true;
        BuildRsView(RoleView, "Role",
            out _roleList!, out _roleFilter!, out _roleName!, out _roleDesc!,
            out _roleAttrsRows!, out _roleSkillsRows!,
            sel => { _selectedRole = sel; RefreshRoleForm(); },
            () => { var m = new RoleSpeciesModel { Name = $"NewRole{_roles.Count + 1}" }; _roles.Add(m); RefreshRoleList(); _roleList!.SelectedItem = m; },
            () => { if (_selectedRole != null) { _roles.Remove(_selectedRole); _selectedRole = null; RefreshRoleList(); RefreshRoleForm(); } },
            () => RefreshRoleList(),
            t => SyncRole(m => { m.Name = t ?? ""; RefreshRoleList(); }),
            t => SyncRole(m => m.Description = t ?? ""));
    }

    private void SyncRole(Action<RoleSpeciesModel> apply) { if (_roleSync || _selectedRole == null) return; apply(_selectedRole); }

    private void RefreshRoleList()
    {
        if (_roleList == null) return;
        var f = (_roleFilter?.Text ?? "").Trim().ToLowerInvariant();
        var visible = string.IsNullOrEmpty(f) ? _roles.ToList()
            : _roles.Where(r => r.Name.ToLowerInvariant().Contains(f)).ToList();
        _roleList.ItemsSource = visible;
        if (_selectedRole != null && visible.Contains(_selectedRole))
            _roleList.SelectedItem = _selectedRole;
        else
            _selectedRole = null;
    }

    private void RefreshRoleForm()
    {
        if (_selectedRole == null)
        {
            // Clear the row stacks if nothing is selected.
            _roleAttrsRows?.Children.Clear();
            _roleSkillsRows?.Children.Clear();
            return;
        }
        _roleSync = true;
        try
        {
            _roleName!.Text = _selectedRole.Name;
            _roleDesc!.Text = _selectedRole.Description;
            BuildDiceMapEditor(_roleAttrsRows!, _selectedRole.AttributeBonuses, AllAttributes);
            BuildDiceMapEditor(_roleSkillsRows!, _selectedRole.SkillBonuses, AllSkills);
        }
        finally { _roleSync = false; }
    }

    private void LoadRoles()
    {
        BuildRoleViewIfNeeded();
        var path = PathBox.Text ?? "";
        if (!File.Exists(path)) { Status($"file not found: {path}", error: true); return; }
        _roleParser = new RoleSpeciesFileParser(path, "Role");
        if (!_roleParser.TryLoad()) { Status($"parse failed: {_roleParser.Error}", error: true); return; }
        _roles = _roleParser.Entries.ToList();
        _selectedRole = null;
        if (_roleList != null) _roleList.SelectedItem = null;
        RefreshRoleList();
        RefreshRoleForm();
        Status($"loaded {_roles.Count} roles");
    }

    private void SaveRoles()
    {
        if (_roleParser == null) { Status("nothing loaded", error: true); return; }
        try { RoleSpeciesFileWriter.Save(_roleParser, _roles); Status($"saved {_roles.Count} roles"); LoadRoles(); }
        catch (Exception ex) { Status($"save failed: {ex.Message}", error: true); }
    }

    // ---------- Species ----------

    private void BuildSpeciesViewIfNeeded()
    {
        if (_speciesBuilt) return;
        _speciesBuilt = true;
        BuildRsView(SpeciesView, "Species",
            out _speciesList!, out _speciesFilter!, out _speciesName!, out _speciesDesc!,
            out _speciesAttrsRows!, out _speciesSkillsRows!,
            sel => { _selectedSpecies = sel; RefreshSpeciesForm(); },
            () => { var m = new RoleSpeciesModel { Name = $"NewSpecies{_species.Count + 1}" }; _species.Add(m); RefreshSpeciesList(); _speciesList!.SelectedItem = m; },
            () => { if (_selectedSpecies != null) { _species.Remove(_selectedSpecies); _selectedSpecies = null; RefreshSpeciesList(); RefreshSpeciesForm(); } },
            () => RefreshSpeciesList(),
            t => SyncSpecies(m => { m.Name = t ?? ""; RefreshSpeciesList(); }),
            t => SyncSpecies(m => m.Description = t ?? ""));
    }

    private void SyncSpecies(Action<RoleSpeciesModel> apply) { if (_speciesSync || _selectedSpecies == null) return; apply(_selectedSpecies); }

    private void RefreshSpeciesList()
    {
        if (_speciesList == null) return;
        var f = (_speciesFilter?.Text ?? "").Trim().ToLowerInvariant();
        var visible = string.IsNullOrEmpty(f) ? _species.ToList()
            : _species.Where(s => s.Name.ToLowerInvariant().Contains(f)).ToList();
        _speciesList.ItemsSource = visible;
        if (_selectedSpecies != null && visible.Contains(_selectedSpecies))
            _speciesList.SelectedItem = _selectedSpecies;
        else
            _selectedSpecies = null;
    }

    private void RefreshSpeciesForm()
    {
        if (_selectedSpecies == null)
        {
            _speciesAttrsRows?.Children.Clear();
            _speciesSkillsRows?.Children.Clear();
            return;
        }
        _speciesSync = true;
        try
        {
            _speciesName!.Text = _selectedSpecies.Name;
            _speciesDesc!.Text = _selectedSpecies.Description;
            BuildDiceMapEditor(_speciesAttrsRows!, _selectedSpecies.AttributeBonuses, AllAttributes);
            BuildDiceMapEditor(_speciesSkillsRows!, _selectedSpecies.SkillBonuses, AllSkills);
        }
        finally { _speciesSync = false; }
    }

    private void LoadSpecies()
    {
        BuildSpeciesViewIfNeeded();
        var path = PathBox.Text ?? "";
        if (!File.Exists(path)) { Status($"file not found: {path}", error: true); return; }
        _speciesParser = new RoleSpeciesFileParser(path, "Species");
        if (!_speciesParser.TryLoad()) { Status($"parse failed: {_speciesParser.Error}", error: true); return; }
        _species = _speciesParser.Entries.ToList();
        _selectedSpecies = null;
        if (_speciesList != null) _speciesList.SelectedItem = null;
        RefreshSpeciesList();
        RefreshSpeciesForm();
        Status($"loaded {_species.Count} species");
    }

    private void SaveSpecies()
    {
        if (_speciesParser == null) { Status("nothing loaded", error: true); return; }
        try { RoleSpeciesFileWriter.Save(_speciesParser, _species); Status($"saved {_species.Count} species"); LoadSpecies(); }
        catch (Exception ex) { Status($"save failed: {ex.Message}", error: true); }
    }

    // ---------- Shared form builder ----------

    private static void BuildRsView(
        Border host, string entityName,
        out ListBox list, out TextBox filter,
        out TextBox name, out TextBox desc,
        out StackPanel attrsRows, out StackPanel skillsRows,
        Action<RoleSpeciesModel?> onSelect,
        Action onAdd, Action onDelete, Action onFilter,
        Action<string> onName, Action<string> onDesc)
    {
        var (listPane, fl, add, del, lb) = EditorHelpers.BuildListPane($"Add new {entityName.ToLowerInvariant()}", $"Delete selected {entityName.ToLowerInvariant()}");
        filter = fl; list = lb;
        fl.KeyUp += (_, _) => onFilter();
        lb.SelectionChanged += (_, _) => onSelect(lb.SelectedItem as RoleSpeciesModel);
        add.Click += (_, _) => onAdd();
        del.Click += (_, _) => onDelete();

        name = EditorHelpers.NewTextBox();
        desc = EditorHelpers.NewTextBox(multiline: true);
        attrsRows = new StackPanel { Spacing = 3 };
        skillsRows = new StackPanel { Spacing = 3 };

        var form = new StackPanel { Margin = new Avalonia.Thickness(12), Spacing = 6 };
        form.Children.Add(new TextBlock { Text = entityName, FontSize = 18, FontWeight = FontWeight.Bold, Foreground = new SolidColorBrush(Color.Parse("#F7FAFB")) });
        form.Children.Add(EditorHelpers.FormRow("Name", name));
        form.Children.Add(EditorHelpers.FormRow("Description", desc));

        var attrsLabel = entityName == "Species" ? "Base attributes" : "Attribute bonuses";
        form.Children.Add(new TextBlock { Text = attrsLabel, FontWeight = FontWeight.Bold, Foreground = new SolidColorBrush(Color.Parse("#AAB6BF")), Margin = new Avalonia.Thickness(0, 8, 0, 4) });
        form.Children.Add(attrsRows);

        form.Children.Add(new TextBlock { Text = "Skill bonuses", FontWeight = FontWeight.Bold, Foreground = new SolidColorBrush(Color.Parse("#AAB6BF")), Margin = new Avalonia.Thickness(0, 8, 0, 4) });
        form.Children.Add(skillsRows);

        var nb = name; var db = desc;
        nb.TextChanged += (_, _) => onName(nb.Text ?? "");
        db.TextChanged += (_, _) => onDesc(db.Text ?? "");

        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("260,*") };
        Grid.SetColumn(listPane, 0); grid.Children.Add(listPane);
        var sv = new ScrollViewer { Content = form };
        Grid.SetColumn(sv, 1); grid.Children.Add(sv);
        host.Padding = default;
        host.Child = grid;
    }

    /// Renders structured rows for a Dictionary&lt;string, (Dice, Pips)&gt;:
    /// each row is [key dropdown][dice ↑↓][pips ↑↓][✕ delete], and a final
    /// "add" row at the bottom with a dropdown of unused keys.
    /// Mutating the rows updates the dict in place; the caller is responsible
    /// for re-rendering when the underlying selection changes.
    private static void BuildDiceMapEditor(
        StackPanel container,
        Dictionary<string, (int Dice, int Pips)> map,
        string[] availableKeys,
        Action? onChange = null)
    {
        container.Children.Clear();
        // Render existing entries in a stable order using the canonical key list.
        var ordered = availableKeys.Where(map.ContainsKey).Concat(map.Keys.Except(availableKeys)).Distinct().ToList();

        foreach (var key in ordered)
        {
            container.Children.Add(BuildDiceRow(container, map, availableKeys, key, onChange));
        }

        container.Children.Add(BuildAddDiceRow(container, map, availableKeys, onChange));
    }

    private static Control BuildDiceRow(
        StackPanel container,
        Dictionary<string, (int Dice, int Pips)> map,
        string[] availableKeys,
        string key,
        Action? onChange)
    {
        var (dice, pips) = map[key];

        var keyCombo = EditorHelpers.NewCombo(availableKeys);
        keyCombo.SelectedItem = key;
        keyCombo.Width = 160;

        var diceBox = EditorHelpers.NewNumeric(0, 12);
        diceBox.Value = dice;
        diceBox.Width = 90;

        var dPlus = new TextBlock { Text = "D+", VerticalAlignment = VerticalAlignment.Center, Margin = new Avalonia.Thickness(2, 0) };

        var pipsBox = EditorHelpers.NewNumeric(0, 2);
        pipsBox.Value = pips;
        pipsBox.Width = 70;

        var del = new Button
        {
            Content = "✕",
            Padding = new Avalonia.Thickness(8, 2),
            Margin = new Avalonia.Thickness(6, 0, 0, 0),
        };
        ToolTip.SetTip(del, "Delete this entry");

        // Track current key so rename detection works after combo change.
        var currentKey = key;

        keyCombo.SelectionChanged += (_, _) =>
        {
            if (keyCombo.SelectedItem is not string newKey) return;
            if (newKey == currentKey) return;
            // If new key is already used, ignore the change and revert.
            if (map.ContainsKey(newKey))
            {
                keyCombo.SelectedItem = currentKey;
                return;
            }
            var v = map[currentKey];
            map.Remove(currentKey);
            map[newKey] = v;
            currentKey = newKey;
            onChange?.Invoke();
        };

        diceBox.ValueChanged += (_, _) =>
        {
            if (!map.ContainsKey(currentKey)) return;
            var v = map[currentKey];
            map[currentKey] = ((int)(diceBox.Value ?? 0), v.Pips);
            onChange?.Invoke();
        };

        pipsBox.ValueChanged += (_, _) =>
        {
            if (!map.ContainsKey(currentKey)) return;
            var v = map[currentKey];
            map[currentKey] = (v.Dice, (int)(pipsBox.Value ?? 0));
            onChange?.Invoke();
        };

        del.Click += (_, _) =>
        {
            map.Remove(currentKey);
            BuildDiceMapEditor(container, map, availableKeys, onChange);
            onChange?.Invoke();
        };

        var row = new StackPanel { Orientation = Orientation.Horizontal };
        row.Children.Add(keyCombo);
        row.Children.Add(diceBox);
        row.Children.Add(dPlus);
        row.Children.Add(pipsBox);
        row.Children.Add(del);
        return row;
    }

    private static Control BuildAddDiceRow(
        StackPanel container,
        Dictionary<string, (int Dice, int Pips)> map,
        string[] availableKeys,
        Action? onChange)
    {
        var unused = availableKeys.Where(k => !map.ContainsKey(k)).ToList();
        var combo = EditorHelpers.NewCombo(unused);
        combo.PlaceholderText = "(add…)";
        combo.Width = 160;

        var add = new Button
        {
            Content = "Add",
            Padding = new Avalonia.Thickness(10, 2),
            Margin = new Avalonia.Thickness(6, 0, 0, 0),
            IsEnabled = unused.Count > 0,
        };
        add.Click += (_, _) =>
        {
            if (combo.SelectedItem is not string newKey) return;
            if (map.ContainsKey(newKey)) return;
            map[newKey] = (1, 0);
            BuildDiceMapEditor(container, map, availableKeys, onChange);
            onChange?.Invoke();
        };

        var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Avalonia.Thickness(0, 6, 0, 0) };
        row.Children.Add(combo);
        row.Children.Add(add);
        return row;
    }
}
