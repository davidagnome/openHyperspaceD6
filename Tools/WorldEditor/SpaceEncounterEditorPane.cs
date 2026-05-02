using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace TerminalHyperspace.WorldEditor;

public partial class MainWindow
{
    private SpaceEncounterFileParser? _seParser;
    private List<SpaceEncounterModel> _ses = new();
    private SpaceEncounterModel? _selectedSe;
    private ListBox? _seList;
    private TextBox? _seFilter, _seMember, _sePilotName, _seShipName, _seShipDesc;
    private CheckBox? _sePilotIsPlayer, _seShipIsSpace;
    private NumericUpDown? _seManeuverD, _seManeuverP, _seResolve;
    private ComboBox? _seArmor, _seShield;
    private StackPanel? _seAttrsRows, _seSkillsRows, _seWeaponRows, _seEquipmentRows;
    private bool _seBuilt, _seSync;

    private List<string> _shieldChoices = new();

    private void BuildSpaceEncounterViewIfNeeded()
    {
        if (_seBuilt) return;
        _seBuilt = true;

        var (listPane, filter, add, del, list) = EditorHelpers.BuildListPane("Add new encounter", "Delete selected encounter");
        _seFilter = filter; _seList = list;
        filter.KeyUp += (_, _) => RefreshSeList();
        list.SelectionChanged += (_, _) => { _selectedSe = list.SelectedItem as SpaceEncounterModel; RefreshSeForm(); };
        add.Click += (_, _) =>
        {
            var m = new SpaceEncounterModel
            {
                MemberName = $"NewEncounter{_ses.Count + 1}",
                PilotName = "New Pilot",
                ShipName = "New Ship",
                ShipIsSpace = true,
                ShipManeuverDice = 1,
                ShipResolve = 16,
            };
            _ses.Add(m); RefreshSeList(); list.SelectedItem = m;
        };
        del.Click += (_, _) => { if (_selectedSe != null) { _ses.Remove(_selectedSe); _selectedSe = null; RefreshSeList(); RefreshSeForm(); } };

        _seMember        = EditorHelpers.NewTextBox();
        _sePilotName     = EditorHelpers.NewTextBox();
        _sePilotIsPlayer = EditorHelpers.NewCheck("IsPlayer");
        _seAttrsRows     = new StackPanel { Spacing = 3 };
        _seSkillsRows    = new StackPanel { Spacing = 3 };
        _seArmor         = EditorHelpers.NewCombo(Array.Empty<string>());
        _seShipName      = EditorHelpers.NewTextBox();
        _seShipDesc      = EditorHelpers.NewTextBox(multiline: true);
        _seShipIsSpace   = EditorHelpers.NewCheck("IsSpace");
        _seManeuverD     = EditorHelpers.NewNumeric(0, 12);
        _seManeuverP     = EditorHelpers.NewNumeric(0, 2);
        _seResolve       = EditorHelpers.NewNumeric(0, 999);
        _seShield        = EditorHelpers.NewCombo(Array.Empty<string>());
        _seWeaponRows    = new StackPanel { Spacing = 3 };
        _seEquipmentRows = new StackPanel { Spacing = 3 };
        _seArmor.HorizontalAlignment  = HorizontalAlignment.Stretch;
        _seShield.HorizontalAlignment = HorizontalAlignment.Stretch;

        var maneuver = new StackPanel { Orientation = Orientation.Horizontal };
        maneuver.Children.Add(_seManeuverD);
        maneuver.Children.Add(new TextBlock { Text = "D+", VerticalAlignment = VerticalAlignment.Center, Margin = new Avalonia.Thickness(4, 0) });
        maneuver.Children.Add(_seManeuverP);

        var form = new StackPanel { Margin = new Avalonia.Thickness(12), Spacing = 4 };
        form.Children.Add(new TextBlock { Text = "Space Encounter", FontSize = 18, FontWeight = FontWeight.Bold, Foreground = new SolidColorBrush(Color.Parse("#F7FAFB")) });
        form.Children.Add(EditorHelpers.FormRow("Member name", _seMember));

        form.Children.Add(new TextBlock { Text = "Pilot", FontWeight = FontWeight.Bold, Foreground = new SolidColorBrush(Color.Parse("#AAB6BF")), Margin = new Avalonia.Thickness(0, 8, 0, 4) });
        form.Children.Add(EditorHelpers.FormRow("Pilot name", _sePilotName));
        form.Children.Add(EditorHelpers.FormRow("",           _sePilotIsPlayer));
        form.Children.Add(new TextBlock { Text = "Pilot attributes", FontStyle = FontStyle.Italic, Foreground = new SolidColorBrush(Color.Parse("#88909A")), Margin = new Avalonia.Thickness(0, 6, 0, 2) });
        form.Children.Add(_seAttrsRows);
        form.Children.Add(new TextBlock { Text = "Pilot skill bonuses", FontStyle = FontStyle.Italic, Foreground = new SolidColorBrush(Color.Parse("#88909A")), Margin = new Avalonia.Thickness(0, 6, 0, 2) });
        form.Children.Add(_seSkillsRows);
        form.Children.Add(EditorHelpers.FormRow("Equipped armor", _seArmor));

        form.Children.Add(new TextBlock { Text = "Ship", FontWeight = FontWeight.Bold, Foreground = new SolidColorBrush(Color.Parse("#AAB6BF")), Margin = new Avalonia.Thickness(0, 12, 0, 4) });
        form.Children.Add(EditorHelpers.FormRow("Ship name",    _seShipName));
        form.Children.Add(EditorHelpers.FormRow("Description",  _seShipDesc));
        form.Children.Add(EditorHelpers.FormRow("",             _seShipIsSpace));
        form.Children.Add(EditorHelpers.FormRow("Maneuverability", maneuver));
        form.Children.Add(EditorHelpers.FormRow("Resolve",      _seResolve));
        form.Children.Add(EditorHelpers.FormRow("Shield",       _seShield));
        form.Children.Add(new TextBlock { Text = "Weapons", FontStyle = FontStyle.Italic, Foreground = new SolidColorBrush(Color.Parse("#88909A")), Margin = new Avalonia.Thickness(0, 6, 0, 2) });
        form.Children.Add(_seWeaponRows);
        form.Children.Add(new TextBlock { Text = "Equipment", FontStyle = FontStyle.Italic, Foreground = new SolidColorBrush(Color.Parse("#88909A")), Margin = new Avalonia.Thickness(0, 6, 0, 2) });
        form.Children.Add(_seEquipmentRows);

        _seMember.TextChanged    += (_, _) => SyncSe(m => { m.MemberName = _seMember!.Text ?? ""; RefreshSeList(); });
        _sePilotName.TextChanged += (_, _) => SyncSe(m => m.PilotName = _sePilotName!.Text ?? "");
        _sePilotIsPlayer.IsCheckedChanged += (_, _) => SyncSe(m => m.PilotIsPlayer = _sePilotIsPlayer!.IsChecked == true);
        _seArmor.SelectionChanged += (_, _) => SyncSe(m => m.PilotEquippedArmorMember = _seArmor!.SelectedItem as string ?? "");
        _seShipName.TextChanged   += (_, _) => SyncSe(m => { m.ShipName = _seShipName!.Text ?? ""; RefreshSeList(); });
        _seShipDesc.TextChanged   += (_, _) => SyncSe(m => m.ShipDescription = _seShipDesc!.Text ?? "");
        _seShipIsSpace.IsCheckedChanged += (_, _) => SyncSe(m => m.ShipIsSpace = _seShipIsSpace!.IsChecked == true);
        _seManeuverD.ValueChanged += (_, _) => SyncSe(m => m.ShipManeuverDice = (int)(_seManeuverD!.Value ?? 0));
        _seManeuverP.ValueChanged += (_, _) => SyncSe(m => m.ShipManeuverPips = (int)(_seManeuverP!.Value ?? 0));
        _seResolve.ValueChanged   += (_, _) => SyncSe(m => m.ShipResolve = (int)(_seResolve!.Value ?? 0));
        _seShield.SelectionChanged += (_, _) => SyncSe(m => m.ShipShieldMember = _seShield!.SelectedItem as string ?? "");

        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("260,*") };
        Grid.SetColumn(listPane, 0); grid.Children.Add(listPane);
        var sv = new ScrollViewer { Content = form };
        Grid.SetColumn(sv, 1); grid.Children.Add(sv);
        SpaceEncounterView.Padding = default;
        SpaceEncounterView.Child = grid;
    }

    private void SyncSe(Action<SpaceEncounterModel> apply) { if (_seSync || _selectedSe == null) return; apply(_selectedSe); }

    private void RefreshSeList()
    {
        if (_seList == null) return;
        var f = (_seFilter?.Text ?? "").Trim().ToLowerInvariant();
        var visible = string.IsNullOrEmpty(f) ? _ses.ToList()
            : _ses.Where(e => e.MemberName.ToLowerInvariant().Contains(f) || e.ShipName.ToLowerInvariant().Contains(f) || e.PilotName.ToLowerInvariant().Contains(f)).ToList();
        _seList.ItemsSource = visible;
        if (_selectedSe != null && visible.Contains(_selectedSe))
            _seList.SelectedItem = _selectedSe;
        else
            _selectedSe = null;
    }

    private void RefreshSeForm()
    {
        if (_selectedSe == null)
        {
            _seAttrsRows?.Children.Clear();
            _seSkillsRows?.Children.Clear();
            _seWeaponRows?.Children.Clear();
            _seEquipmentRows?.Children.Clear();
            return;
        }
        _seSync = true;
        try
        {
            _seMember!.Text       = _selectedSe.MemberName;
            _sePilotName!.Text    = _selectedSe.PilotName;
            _sePilotIsPlayer!.IsChecked = _selectedSe.PilotIsPlayer;
            BuildDiceMapEditor(_seAttrsRows!,  _selectedSe.PilotAttributes,   AllAttributes);
            BuildDiceMapEditor(_seSkillsRows!, _selectedSe.PilotSkillBonuses, AllSkills);
            _seArmor!.SelectedItem = string.IsNullOrEmpty(_selectedSe.PilotEquippedArmorMember) ? null : _selectedSe.PilotEquippedArmorMember;
            _seShipName!.Text     = _selectedSe.ShipName;
            _seShipDesc!.Text     = _selectedSe.ShipDescription;
            _seShipIsSpace!.IsChecked = _selectedSe.ShipIsSpace;
            _seManeuverD!.Value   = _selectedSe.ShipManeuverDice;
            _seManeuverP!.Value   = _selectedSe.ShipManeuverPips;
            _seResolve!.Value     = _selectedSe.ShipResolve;
            _seShield!.SelectedItem = string.IsNullOrEmpty(_selectedSe.ShipShieldMember) ? null : _selectedSe.ShipShieldMember;
            BuildWeaponRows(_seWeaponRows!, _selectedSe.ShipWeapons);
            BuildEquipmentRows(_seEquipmentRows!, _selectedSe.ShipEquipment);
        }
        finally { _seSync = false; }
    }

    private void LoadSpaceEncounters()
    {
        BuildSpaceEncounterViewIfNeeded();
        var path = PathBox.Text ?? "";
        if (!File.Exists(path)) { Status($"file not found: {path}", error: true); return; }
        _seParser = new SpaceEncounterFileParser(path);
        if (!_seParser.TryLoad()) { Status($"parse failed: {_seParser.Error}", error: true); return; }
        _ses = _seParser.Encounters.ToList();
        _selectedSe = null;
        if (_seList != null) _seList.SelectedItem = null;

        // Populate Equipped Armor and Shield dropdowns from sibling Content files.
        var contentDir = System.IO.Path.GetDirectoryName(path)!;
        _armorChoices = SymbolScanner.ScanFactories(System.IO.Path.Combine(contentDir, "ArmorData.cs"), "Armor");
        _shieldChoices = SymbolScanner.ScanFactories(System.IO.Path.Combine(contentDir, "ArmorData.cs"), "VehicleShield");
        var armors = new List<string> { "" }; armors.AddRange(_armorChoices);
        var shields = new List<string> { "" }; shields.AddRange(_shieldChoices);
        if (_seArmor  != null) _seArmor.ItemsSource  = armors;
        if (_seShield != null) _seShield.ItemsSource = shields;

        RefreshSeList();
        RefreshSeForm();
        Status($"loaded {_ses.Count} encounters");
    }

    private void SaveSpaceEncounters()
    {
        if (_seParser == null) { Status("nothing loaded", error: true); return; }
        try { SpaceEncounterFileWriter.Save(_seParser, _ses); Status($"saved {_ses.Count} encounters"); LoadSpaceEncounters(); }
        catch (Exception ex) { Status($"save failed: {ex.Message}", error: true); }
    }

    // ---------- Weapons / Equipment row editors ----------
    // Each entry renders as its own row of inline controls with a delete button,
    // and a final "+ Add" row appends a fresh blank entry.

    private void BuildWeaponRows(StackPanel container, List<VehicleWeaponModel> list)
    {
        container.Children.Clear();
        for (int i = 0; i < list.Count; i++)
        {
            var idx = i;
            container.Children.Add(BuildWeaponRow(container, list, idx));
        }
        var addBtn = new Button { Content = "+ Add weapon", Padding = new Avalonia.Thickness(10, 2), Margin = new Avalonia.Thickness(0, 4, 0, 0) };
        addBtn.Click += (_, _) =>
        {
            list.Add(new VehicleWeaponModel { Name = "New Weapon", DamageDice = 1, AttackSkill = "Gunnery" });
            BuildWeaponRows(container, list);
        };
        container.Children.Add(addBtn);
    }

    private Control BuildWeaponRow(StackPanel container, List<VehicleWeaponModel> list, int idx)
    {
        var entry = list[idx];

        var name = EditorHelpers.NewTextBox(minWidth: 180);
        name.Text = entry.Name;
        name.TextChanged += (_, _) => { if (idx < list.Count) list[idx].Name = name.Text ?? ""; };

        var dice = EditorHelpers.NewNumeric(0, 12);
        dice.Value = entry.DamageDice;
        dice.Width = 80;
        dice.ValueChanged += (_, _) => { if (idx < list.Count) list[idx].DamageDice = (int)(dice.Value ?? 0); };

        var dPlus = new TextBlock { Text = "D+", VerticalAlignment = VerticalAlignment.Center, Margin = new Avalonia.Thickness(2, 0) };

        var pips = EditorHelpers.NewNumeric(0, 2);
        pips.Value = entry.DamagePips;
        pips.Width = 70;
        pips.ValueChanged += (_, _) => { if (idx < list.Count) list[idx].DamagePips = (int)(pips.Value ?? 0); };

        var skill = EditorHelpers.NewCombo(AllSkills);
        skill.SelectedItem = string.IsNullOrEmpty(entry.AttackSkill) ? "Gunnery" : entry.AttackSkill;
        skill.Width = 140;
        skill.Margin = new Avalonia.Thickness(4, 0, 0, 0);
        skill.SelectionChanged += (_, _) => { if (idx < list.Count) list[idx].AttackSkill = skill.SelectedItem as string ?? "Gunnery"; };

        var del = new Button { Content = "✕", Padding = new Avalonia.Thickness(8, 2), Margin = new Avalonia.Thickness(6, 0, 0, 0) };
        ToolTip.SetTip(del, "Delete this weapon");
        del.Click += (_, _) => { if (idx < list.Count) list.RemoveAt(idx); BuildWeaponRows(container, list); };

        var row = new StackPanel { Orientation = Orientation.Horizontal };
        row.Children.Add(name);
        row.Children.Add(dice);
        row.Children.Add(dPlus);
        row.Children.Add(pips);
        row.Children.Add(skill);
        row.Children.Add(del);
        return row;
    }

    private void BuildEquipmentRows(StackPanel container, List<VehicleEquipmentModel> list)
    {
        container.Children.Clear();
        for (int i = 0; i < list.Count; i++)
        {
            var idx = i;
            container.Children.Add(BuildEquipmentRow(container, list, idx));
        }
        var addBtn = new Button { Content = "+ Add equipment", Padding = new Avalonia.Thickness(10, 2), Margin = new Avalonia.Thickness(0, 4, 0, 0) };
        addBtn.Click += (_, _) =>
        {
            list.Add(new VehicleEquipmentModel { Name = "New Equipment", BonusSkill = "Gunnery", BonusDice = 0, BonusPips = 1 });
            BuildEquipmentRows(container, list);
        };
        container.Children.Add(addBtn);
    }

    private Control BuildEquipmentRow(StackPanel container, List<VehicleEquipmentModel> list, int idx)
    {
        var entry = list[idx];

        var name = EditorHelpers.NewTextBox(minWidth: 180);
        name.Text = entry.Name;
        name.TextChanged += (_, _) => { if (idx < list.Count) list[idx].Name = name.Text ?? ""; };

        var skill = EditorHelpers.NewCombo(AllSkills);
        skill.SelectedItem = string.IsNullOrEmpty(entry.BonusSkill) ? "Gunnery" : entry.BonusSkill;
        skill.Width = 140;
        skill.Margin = new Avalonia.Thickness(4, 0, 0, 0);
        skill.SelectionChanged += (_, _) => { if (idx < list.Count) list[idx].BonusSkill = skill.SelectedItem as string ?? "Gunnery"; };

        var dice = EditorHelpers.NewNumeric(0, 12);
        dice.Value = entry.BonusDice;
        dice.Width = 80;
        dice.ValueChanged += (_, _) => { if (idx < list.Count) list[idx].BonusDice = (int)(dice.Value ?? 0); };

        var dPlus = new TextBlock { Text = "D+", VerticalAlignment = VerticalAlignment.Center, Margin = new Avalonia.Thickness(2, 0) };

        var pips = EditorHelpers.NewNumeric(0, 2);
        pips.Value = entry.BonusPips;
        pips.Width = 70;
        pips.ValueChanged += (_, _) => { if (idx < list.Count) list[idx].BonusPips = (int)(pips.Value ?? 0); };

        var del = new Button { Content = "✕", Padding = new Avalonia.Thickness(8, 2), Margin = new Avalonia.Thickness(6, 0, 0, 0) };
        ToolTip.SetTip(del, "Delete this equipment");
        del.Click += (_, _) => { if (idx < list.Count) list.RemoveAt(idx); BuildEquipmentRows(container, list); };

        var row = new StackPanel { Orientation = Orientation.Horizontal };
        row.Children.Add(name);
        row.Children.Add(skill);
        row.Children.Add(dice);
        row.Children.Add(dPlus);
        row.Children.Add(pips);
        row.Children.Add(del);
        return row;
    }
}
