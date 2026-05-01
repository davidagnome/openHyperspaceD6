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
    private TextBox? _npcFilter, _npcMember, _npcName, _npcWeapon, _npcArmor;
    private CheckBox? _npcIsPlayer;
    private TextBox? _npcInventory;        // CSV editor for member-name list
    private TextBox? _npcAttributes;       // "Dexterity:3D, Strength:2D+1, …"
    private TextBox? _npcSkillBonuses;     // same format keyed by SkillType
    private bool _npcBuilt, _npcSync;

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
        _npcAttributes    = EditorHelpers.NewTextBox(multiline: true);
        _npcSkillBonuses  = EditorHelpers.NewTextBox(multiline: true);
        _npcInventory     = EditorHelpers.NewTextBox();
        _npcWeapon        = EditorHelpers.NewTextBox(180);
        _npcArmor         = EditorHelpers.NewTextBox(180);

        var form = new StackPanel { Margin = new Avalonia.Thickness(12), Spacing = 4 };
        form.Children.Add(new TextBlock { Text = "NPC", FontSize = 18, FontWeight = FontWeight.Bold, Foreground = new SolidColorBrush(Color.Parse("#F7FAFB")) });
        form.Children.Add(EditorHelpers.FormRow("Member name", _npcMember));
        form.Children.Add(EditorHelpers.FormRow("Display name", _npcName));
        form.Children.Add(EditorHelpers.FormRow("",             _npcIsPlayer));
        form.Children.Add(EditorHelpers.FormRow("Attributes (Name:ND[+P], …)", _npcAttributes));
        form.Children.Add(EditorHelpers.FormRow("Skill bonuses (Skill:ND[+P], …)", _npcSkillBonuses));
        form.Children.Add(EditorHelpers.FormRow("Inventory (CSV of ItemData refs)", _npcInventory));
        form.Children.Add(EditorHelpers.FormRow("Equipped weapon (ItemData ref)", _npcWeapon));
        form.Children.Add(EditorHelpers.FormRow("Equipped armor (ArmorData ref)", _npcArmor));

        _npcMember.TextChanged += (_, _) => SyncNpc(m => m.MemberName = _npcMember!.Text ?? "");
        _npcName.TextChanged   += (_, _) => SyncNpc(m => { m.DisplayName = _npcName!.Text ?? ""; RefreshNpcList(); });
        _npcIsPlayer.IsCheckedChanged += (_, _) => SyncNpc(m => m.IsPlayer = _npcIsPlayer!.IsChecked == true);
        _npcAttributes.TextChanged += (_, _) => SyncNpc(m => m.Attributes = ParseDiceMap(_npcAttributes!.Text));
        _npcSkillBonuses.TextChanged += (_, _) => SyncNpc(m => m.SkillBonuses = ParseDiceMap(_npcSkillBonuses!.Text));
        _npcInventory.TextChanged += (_, _) => SyncNpc(m => m.Inventory = ParseCsv(_npcInventory!.Text));
        _npcWeapon.TextChanged += (_, _) => SyncNpc(m => m.EquippedWeaponMember = _npcWeapon!.Text ?? "");
        _npcArmor.TextChanged += (_, _) => SyncNpc(m => m.EquippedArmorMember = _npcArmor!.Text ?? "");

        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("260,*") };
        Grid.SetColumn(listPane, 0); grid.Children.Add(listPane);
        var sv = new ScrollViewer { Content = form };
        Grid.SetColumn(sv, 1); grid.Children.Add(sv);
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
        if (_selectedNpc == null) return;
        _npcSync = true;
        try
        {
            _npcMember!.Text   = _selectedNpc.MemberName;
            _npcName!.Text     = _selectedNpc.DisplayName;
            _npcIsPlayer!.IsChecked = _selectedNpc.IsPlayer;
            _npcAttributes!.Text   = FormatDiceMap(_selectedNpc.Attributes);
            _npcSkillBonuses!.Text = FormatDiceMap(_selectedNpc.SkillBonuses);
            _npcInventory!.Text    = string.Join(", ", _selectedNpc.Inventory);
            _npcWeapon!.Text       = _selectedNpc.EquippedWeaponMember;
            _npcArmor!.Text        = _selectedNpc.EquippedArmorMember;
        }
        finally { _npcSync = false; }
    }

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
        RefreshNpcList();
        Status($"loaded {_npcs.Count} NPCs");
    }

    private void SaveNpcs()
    {
        if (_npcParser == null) { Status("nothing loaded", error: true); return; }
        try { NpcFileWriter.Save(_npcParser, _npcs); Status($"saved {_npcs.Count} NPCs"); LoadNpcs(); }
        catch (Exception ex) { Status($"save failed: {ex.Message}", error: true); }
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
