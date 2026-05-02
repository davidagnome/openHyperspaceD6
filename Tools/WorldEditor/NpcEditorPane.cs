using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace TerminalHyperspace.WorldEditor;

public partial class MainWindow
{
    private NpcFileParser? _npcParser;
    private List<NpcModel> _npcs = new();
    private NpcModel? _selectedNpc;
    private ListBox? _npcList;
    private TextBox? _npcFilter, _npcMember, _npcName;
    private CheckBox? _npcIsPlayer;
    private StackPanel? _npcAttrsRows, _npcSkillsRows, _npcInventoryRows, _npcReferenceRows;
    private ComboBox? _npcWeapon, _npcArmor;
    private bool _npcBuilt, _npcSync;

    /// Skill → governing Attribute mapping. Mirrors Models/SkillType.cs SkillMap
    /// so the reference column can group skills under their attribute parent
    /// and compute cumulative dice (attribute + skill bonus).
    private static readonly Dictionary<string, string> SkillToAttribute = new()
    {
        ["Agility"]    = "Dexterity",  ["Blasters"]   = "Dexterity", ["Melee"]     = "Dexterity", ["Steal"]    = "Dexterity", ["Throw"]   = "Dexterity",
        ["Galaxy"]     = "Knowledge",  ["Streetwise"] = "Knowledge", ["Survival"]  = "Knowledge", ["Willpower"]= "Knowledge", ["Xenology"]= "Knowledge",
        ["Astrogation"]= "Mechanical", ["Drive"]      = "Mechanical",["Gunnery"]   = "Mechanical",["Pilot"]    = "Mechanical",["Sensors"] = "Mechanical",
        ["Deceive"]    = "Perception", ["Hide"]       = "Perception",["Persuade"]  = "Perception",["Search"]   = "Perception",["Tactics"] = "Perception",
        ["Athletics"]  = "Strength",   ["Brawl"]      = "Strength",  ["Intimidate"]= "Strength",  ["Stamina"]  = "Strength",  ["Swim"]    = "Strength",
        ["Armament"]   = "Technical",  ["Computers"]  = "Technical", ["Droids"]    = "Technical", ["Medicine"] = "Technical", ["Vehicles"]= "Technical",
        ["Alter"]      = "Force",      ["Control"]    = "Force",     ["Sense"]     = "Force",
    };

    // Available choices for the equipment / inventory dropdowns. Populated
    // from the sibling Content/*.cs files when the NPC pane is loaded.
    private List<string> _itemChoices = new();
    private List<string> _armorChoices = new();

    private void BuildNpcViewIfNeeded()
    {
        if (_npcBuilt) return;
        _npcBuilt = true;

        var (listPane, filter, add, del, list) = EditorHelpers.BuildListPane("Add new NPC", "Delete selected NPC");
        _npcFilter = filter; _npcList = list;
        filter.KeyUp += (_, _) => RefreshNpcList();
        list.SelectionChanged += (_, _) => { _selectedNpc = list.SelectedItem as NpcModel; RefreshNpcForm(); };
        add.Click += (_, _) => { var m = new NpcModel { MemberName = $"NewNpc{_npcs.Count + 1}", DisplayName = "New NPC" }; _npcs.Add(m); RefreshNpcList(); list.SelectedItem = m; };
        del.Click += (_, _) => { if (_selectedNpc != null) { _npcs.Remove(_selectedNpc); _selectedNpc = null; RefreshNpcList(); RefreshNpcForm(); } };

        _npcMember        = EditorHelpers.NewTextBox();
        _npcName          = EditorHelpers.NewTextBox();
        _npcIsPlayer      = EditorHelpers.NewCheck("IsPlayer");
        _npcAttrsRows     = new StackPanel { Spacing = 3 };
        _npcSkillsRows    = new StackPanel { Spacing = 3 };
        _npcInventoryRows = new StackPanel { Spacing = 3 };
        _npcReferenceRows = new StackPanel { Spacing = 2, Margin = new Avalonia.Thickness(12) };
        _npcWeapon        = EditorHelpers.NewCombo(Array.Empty<string>());
        _npcArmor         = EditorHelpers.NewCombo(Array.Empty<string>());
        _npcWeapon.HorizontalAlignment = HorizontalAlignment.Stretch;
        _npcArmor.HorizontalAlignment  = HorizontalAlignment.Stretch;

        var form = new StackPanel { Margin = new Avalonia.Thickness(12), Spacing = 4 };
        form.Children.Add(new TextBlock { Text = "NPC", FontSize = 18, FontWeight = FontWeight.Bold, Foreground = new SolidColorBrush(Color.Parse("#F7FAFB")) });
        form.Children.Add(EditorHelpers.FormRow("Member name", _npcMember));
        form.Children.Add(EditorHelpers.FormRow("Display name", _npcName));
        form.Children.Add(EditorHelpers.FormRow("",             _npcIsPlayer));

        var attrsLabel = new TextBlock { Text = "Attributes", FontWeight = FontWeight.Bold, Foreground = new SolidColorBrush(Color.Parse("#AAB6BF")), Margin = new Avalonia.Thickness(0, 8, 0, 4) };
        form.Children.Add(attrsLabel);
        form.Children.Add(_npcAttrsRows);

        var skillsLabel = new TextBlock { Text = "Skill bonuses", FontWeight = FontWeight.Bold, Foreground = new SolidColorBrush(Color.Parse("#AAB6BF")), Margin = new Avalonia.Thickness(0, 8, 0, 4) };
        form.Children.Add(skillsLabel);
        form.Children.Add(_npcSkillsRows);

        var invLabel = new TextBlock { Text = "Inventory", FontWeight = FontWeight.Bold, Foreground = new SolidColorBrush(Color.Parse("#AAB6BF")), Margin = new Avalonia.Thickness(0, 8, 0, 4) };
        form.Children.Add(invLabel);
        form.Children.Add(_npcInventoryRows);

        form.Children.Add(EditorHelpers.FormRow("Equipped weapon", _npcWeapon));
        form.Children.Add(EditorHelpers.FormRow("Equipped armor",  _npcArmor));

        _npcMember.TextChanged += (_, _) => SyncNpc(m => m.MemberName = _npcMember!.Text ?? "");
        _npcName.TextChanged   += (_, _) => SyncNpc(m => { m.DisplayName = _npcName!.Text ?? ""; RefreshNpcList(); });
        _npcIsPlayer.IsCheckedChanged += (_, _) => SyncNpc(m => m.IsPlayer = _npcIsPlayer!.IsChecked == true);
        _npcWeapon.SelectionChanged += (_, _) => SyncNpc(m => m.EquippedWeaponMember = _npcWeapon!.SelectedItem as string ?? "");
        _npcArmor.SelectionChanged  += (_, _) => SyncNpc(m => m.EquippedArmorMember  = _npcArmor!.SelectedItem  as string ?? "");

        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("260,*,260") };
        Grid.SetColumn(listPane, 0); grid.Children.Add(listPane);
        var sv = new ScrollViewer { Content = form };
        Grid.SetColumn(sv, 1); grid.Children.Add(sv);

        // Reference column: read-only summary of cumulative skill dice grouped
        // under each governing attribute. Wrapped in its own ScrollViewer because
        // 7 attributes × ~5 skills can exceed the pane height on shorter windows.
        var refHeader = new TextBlock
        {
            Text = "Skill Totals (Attribute + Bonus)",
            FontSize = 13, FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#AAB6BF")),
            Margin = new Avalonia.Thickness(12, 12, 12, 0),
        };
        var refStack = new StackPanel();
        refStack.Children.Add(refHeader);
        refStack.Children.Add(_npcReferenceRows);
        var refScroll = new ScrollViewer
        {
            Content = refStack,
            Background = new SolidColorBrush(Color.Parse("#0B1116")),
        };
        Grid.SetColumn(refScroll, 2); grid.Children.Add(refScroll);

        NPCView.Padding = default;
        NPCView.Child = grid;
    }

    private void SyncNpc(Action<NpcModel> apply) { if (_npcSync || _selectedNpc == null) return; apply(_selectedNpc); }

    private void RefreshNpcList()
    {
        if (_npcList == null) return;
        var f = (_npcFilter?.Text ?? "").Trim().ToLowerInvariant();
        var visible = string.IsNullOrEmpty(f) ? _npcs.ToList()
            : _npcs.Where(n => n.MemberName.ToLowerInvariant().Contains(f) || n.DisplayName.ToLowerInvariant().Contains(f)).ToList();
        _npcList.ItemsSource = visible;
        if (_selectedNpc != null && visible.Contains(_selectedNpc))
            _npcList.SelectedItem = _selectedNpc;
        else
            _selectedNpc = null;
    }

    private void RefreshNpcForm()
    {
        if (_selectedNpc == null)
        {
            _npcAttrsRows?.Children.Clear();
            _npcSkillsRows?.Children.Clear();
            _npcInventoryRows?.Children.Clear();
            _npcReferenceRows?.Children.Clear();
            return;
        }
        _npcSync = true;
        try
        {
            _npcMember!.Text   = _selectedNpc.MemberName;
            _npcName!.Text     = _selectedNpc.DisplayName;
            _npcIsPlayer!.IsChecked = _selectedNpc.IsPlayer;
            BuildDiceMapEditor(_npcAttrsRows!,  _selectedNpc.Attributes,   AllAttributes, RefreshNpcReference);
            BuildDiceMapEditor(_npcSkillsRows!, _selectedNpc.SkillBonuses, AllSkills,     RefreshNpcReference);
            BuildItemListEditor(_npcInventoryRows!, _selectedNpc.Inventory, _itemChoices);
            _npcWeapon!.SelectedItem = string.IsNullOrEmpty(_selectedNpc.EquippedWeaponMember) ? null : _selectedNpc.EquippedWeaponMember;
            _npcArmor!.SelectedItem  = string.IsNullOrEmpty(_selectedNpc.EquippedArmorMember)  ? null : _selectedNpc.EquippedArmorMember;
            RefreshNpcReference();
        }
        finally { _npcSync = false; }
    }

    /// Renders the reference column: each Attribute, then its Skills indented
    /// underneath with the cumulative dice (attribute + skill bonus, with
    /// 3-pip-to-1-die carry to match Models/DiceCode.cs).
    private void RefreshNpcReference()
    {
        if (_npcReferenceRows == null) return;
        _npcReferenceRows.Children.Clear();
        if (_selectedNpc == null) return;

        var attrBrush  = new SolidColorBrush(Color.Parse("#F7FAFB"));
        var skillBrush = new SolidColorBrush(Color.Parse("#C8D2DA"));
        var muteBrush  = new SolidColorBrush(Color.Parse("#5A6975"));

        foreach (var attr in AllAttributes)
        {
            var (aDice, aPips) = _selectedNpc.Attributes.TryGetValue(attr, out var av) ? av : (0, 0);
            var attrLine = new TextBlock
            {
                Text = $"{attr}: {FormatDice(aDice, aPips)}",
                FontWeight = FontWeight.Bold,
                Foreground = attrBrush,
                Margin = new Avalonia.Thickness(0, 6, 0, 2),
            };
            _npcReferenceRows.Children.Add(attrLine);

            foreach (var skill in AllSkills.Where(s => SkillToAttribute.TryGetValue(s, out var a) && a == attr))
            {
                var (sDice, sPips) = _selectedNpc.SkillBonuses.TryGetValue(skill, out var sv) ? sv : (0, 0);
                var (totalDice, totalPips) = AddDice(aDice, aPips, sDice, sPips);
                var hasBonus = sDice != 0 || sPips != 0;
                _npcReferenceRows.Children.Add(new TextBlock
                {
                    Text = $"    {skill}: {FormatDice(totalDice, totalPips)}",
                    Foreground = hasBonus ? skillBrush : muteBrush,
                });
            }
        }
    }

    /// Combines an attribute dice code with a skill bonus dice code, carrying
    /// every 3 pips into a die — same rule as the runtime DiceCode struct.
    private static (int dice, int pips) AddDice(int aD, int aP, int bD, int bP)
    {
        var totalPips = aP + bP;
        return (aD + bD + totalPips / 3, totalPips % 3);
    }

    private static string FormatDice(int dice, int pips)
        => pips == 0 ? $"{dice}D" : $"{dice}D+{pips}";

    private void LoadNpcs()
    {
        BuildNpcViewIfNeeded();
        var path = PathBox.Text ?? "";
        if (!File.Exists(path)) { Status($"file not found: {path}", error: true); return; }
        _npcParser = new NpcFileParser(path);
        if (!_npcParser.TryLoad()) { Status($"parse failed: {_npcParser.Error}", error: true); return; }
        _npcs = _npcParser.Npcs.ToList();
        _selectedNpc = null;
        if (_npcList != null) _npcList.SelectedItem = null;

        // Populate equipment / inventory choices from sibling Content files.
        var contentDir = System.IO.Path.GetDirectoryName(path)!;
        _itemChoices  = SymbolScanner.ScanFactories(System.IO.Path.Combine(contentDir, "ItemData.cs"),  "Item");
        _armorChoices = SymbolScanner.ScanFactories(System.IO.Path.Combine(contentDir, "ArmorData.cs"), "Armor");
        var weaponItems = new List<string> { "" };
        weaponItems.AddRange(_itemChoices);
        var armorItems = new List<string> { "" };
        armorItems.AddRange(_armorChoices);
        if (_npcWeapon != null) _npcWeapon.ItemsSource = weaponItems;
        if (_npcArmor  != null) _npcArmor.ItemsSource  = armorItems;

        RefreshNpcList();
        RefreshNpcForm();
        Status($"loaded {_npcs.Count} NPCs");
    }

    private void SaveNpcs()
    {
        if (_npcParser == null) { Status("nothing loaded", error: true); return; }
        try { NpcFileWriter.Save(_npcParser, _npcs); Status($"saved {_npcs.Count} NPCs"); LoadNpcs(); }
        catch (Exception ex) { Status($"save failed: {ex.Message}", error: true); }
    }

    /// Renders an editable list of factory-name picks (for Inventory). Each
    /// row is [combo][✕]; a final add-row appends a new entry. Mutating the
    /// rows updates `list` in place.
    private static void BuildItemListEditor(StackPanel container, List<string> list, List<string> choices)
    {
        container.Children.Clear();

        for (int i = 0; i < list.Count; i++)
        {
            var idx = i;
            var combo = EditorHelpers.NewCombo(choices);
            combo.SelectedItem = list[idx];
            combo.Width = 220;
            combo.SelectionChanged += (_, _) =>
            {
                if (idx >= list.Count) return;
                if (combo.SelectedItem is string s) list[idx] = s;
            };

            var del = new Button
            {
                Content = "✕",
                Padding = new Avalonia.Thickness(8, 2),
                Margin = new Avalonia.Thickness(6, 0, 0, 0),
            };
            ToolTip.SetTip(del, "Remove from inventory");
            del.Click += (_, _) =>
            {
                if (idx < list.Count) list.RemoveAt(idx);
                BuildItemListEditor(container, list, choices);
            };

            var row = new StackPanel { Orientation = Orientation.Horizontal };
            row.Children.Add(combo);
            row.Children.Add(del);
            container.Children.Add(row);
        }

        // Add row.
        var addCombo = EditorHelpers.NewCombo(choices);
        addCombo.PlaceholderText = "(add…)";
        addCombo.Width = 220;
        var add = new Button
        {
            Content = "Add",
            Padding = new Avalonia.Thickness(10, 2),
            Margin = new Avalonia.Thickness(6, 0, 0, 0),
            IsEnabled = choices.Count > 0,
        };
        add.Click += (_, _) =>
        {
            if (addCombo.SelectedItem is not string s) return;
            list.Add(s);
            BuildItemListEditor(container, list, choices);
        };
        var addRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Avalonia.Thickness(0, 6, 0, 0) };
        addRow.Children.Add(addCombo);
        addRow.Children.Add(add);
        container.Children.Add(addRow);
    }

    // Shared parsing helpers used by NPC + Role + Species panes ---------------

    /// "Dexterity:3D+1, Strength:2D" → dictionary keyed by enum-name.
    public static Dictionary<string, (int Dice, int Pips)> ParseDiceMap(string? text)
    {
        var map = new Dictionary<string, (int, int)>();
        if (string.IsNullOrWhiteSpace(text)) return map;
        foreach (var entry in text.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var t = entry.Trim();
            var colon = t.IndexOf(':');
            if (colon <= 0) continue;
            var key = t.Substring(0, colon).Trim();
            var dice = t.Substring(colon + 1).Trim().ToUpperInvariant();
            int dIdx = dice.IndexOf('D');
            if (dIdx <= 0) continue;
            int.TryParse(dice.Substring(0, dIdx), out int d);
            int p = 0;
            if (dIdx + 1 < dice.Length && dice[dIdx + 1] == '+')
                int.TryParse(dice.Substring(dIdx + 2), out p);
            map[key] = (d, p);
        }
        return map;
    }

    public static string FormatDiceMap(Dictionary<string, (int Dice, int Pips)> map)
        => string.Join(", ", map.Select(kv =>
            kv.Value.Pips == 0 ? $"{kv.Key}:{kv.Value.Dice}D" : $"{kv.Key}:{kv.Value.Dice}D+{kv.Value.Pips}"));

    public static List<string> ParseCsv(string? text)
        => string.IsNullOrWhiteSpace(text) ? new List<string>()
            : text.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).Where(s => s.Length > 0).ToList();
}
