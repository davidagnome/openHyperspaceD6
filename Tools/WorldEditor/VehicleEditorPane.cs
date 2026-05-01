using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace TerminalHyperspace.WorldEditor;

public partial class MainWindow
{
    private VehicleFileParser? _vhParser;
    private List<VehicleModel> _vhs = new();
    private VehicleModel? _selectedVh;
    private ListBox? _vhList;
    private TextBox? _vhFilter, _vhMember, _vhName, _vhDesc, _vhShield, _vhWeapons, _vhEquipment;
    private CheckBox? _vhIsSpace;
    private NumericUpDown? _vhManeuverD, _vhManeuverP, _vhResolve, _vhPrice;
    private bool _vhBuilt, _vhSync;

    private void BuildVehicleViewIfNeeded()
    {
        if (_vhBuilt) return;
        _vhBuilt = true;

        var (listPane, filter, add, del, list) = EditorHelpers.BuildListPane("Add new vehicle", "Delete selected vehicle");
        _vhFilter = filter; _vhList = list;
        filter.KeyUp += (_, _) => RefreshVhList();
        list.SelectionChanged += (_, _) => { _selectedVh = list.SelectedItem as VehicleModel; RefreshVhForm(); };
        add.Click += (_, _) => { var m = new VehicleModel { MemberName = $"NewVehicle{_vhs.Count + 1}", Name = "New Vehicle", Resolve = 16, ManeuverDice = 1 }; _vhs.Add(m); RefreshVhList(); list.SelectedItem = m; };
        del.Click += (_, _) => { if (_selectedVh != null) { _vhs.Remove(_selectedVh); _selectedVh = null; RefreshVhList(); RefreshVhForm(); } };

        _vhMember = EditorHelpers.NewTextBox();
        _vhName = EditorHelpers.NewTextBox();
        _vhDesc = EditorHelpers.NewTextBox(multiline: true);
        _vhIsSpace = EditorHelpers.NewCheck("IsSpace");
        _vhManeuverD = EditorHelpers.NewNumeric(0, 12);
        _vhManeuverP = EditorHelpers.NewNumeric(0, 2);
        _vhResolve = EditorHelpers.NewNumeric(0, 999);
        _vhShield = EditorHelpers.NewTextBox(180);
        _vhWeapons = EditorHelpers.NewTextBox(multiline: true);
        _vhEquipment = EditorHelpers.NewTextBox(multiline: true);
        _vhPrice = EditorHelpers.NewNumeric(0, 99999);

        var maneuver = new StackPanel { Orientation = Orientation.Horizontal };
        maneuver.Children.Add(_vhManeuverD);
        maneuver.Children.Add(new TextBlock { Text = "D+", VerticalAlignment = VerticalAlignment.Center, Margin = new Avalonia.Thickness(4, 0) });
        maneuver.Children.Add(_vhManeuverP);

        var form = new StackPanel { Margin = new Avalonia.Thickness(12), Spacing = 4 };
        form.Children.Add(new TextBlock { Text = "Vehicle", FontSize = 18, FontWeight = FontWeight.Bold, Foreground = new SolidColorBrush(Color.Parse("#F7FAFB")) });
        form.Children.Add(EditorHelpers.FormRow("Member name", _vhMember));
        form.Children.Add(EditorHelpers.FormRow("Display name", _vhName));
        form.Children.Add(EditorHelpers.FormRow("Description", _vhDesc));
        form.Children.Add(EditorHelpers.FormRow("",            _vhIsSpace));
        form.Children.Add(EditorHelpers.FormRow("Maneuverability", maneuver));
        form.Children.Add(EditorHelpers.FormRow("Resolve",     _vhResolve));
        form.Children.Add(EditorHelpers.FormRow("Shield (ShieldData ref)", _vhShield));
        form.Children.Add(EditorHelpers.FormRow("Weapons", _vhWeapons));
        form.Children.Add(new TextBlock { Text = "Format: Name|N D[+P]|Skill, …  e.g. \"Laser Cannons|3D+1|Gunnery\"", Foreground = new SolidColorBrush(Color.Parse("#666")), FontSize = 11, Margin = new Avalonia.Thickness(170, 0, 0, 4) });
        form.Children.Add(EditorHelpers.FormRow("Equipment", _vhEquipment));
        form.Children.Add(new TextBlock { Text = "Format: Name|BonusSkill|N D[+P], …", Foreground = new SolidColorBrush(Color.Parse("#666")), FontSize = 11, Margin = new Avalonia.Thickness(170, 0, 0, 4) });
        form.Children.Add(EditorHelpers.FormRow("Price (cr)", _vhPrice));

        _vhMember.TextChanged += (_, _) => SyncVh(m => { m.MemberName = _vhMember!.Text ?? ""; RefreshVhList(); });
        _vhName.TextChanged   += (_, _) => SyncVh(m => { m.Name = _vhName!.Text ?? ""; RefreshVhList(); });
        _vhDesc.TextChanged   += (_, _) => SyncVh(m => m.Description = _vhDesc!.Text ?? "");
        _vhIsSpace.IsCheckedChanged += (_, _) => SyncVh(m => m.IsSpace = _vhIsSpace!.IsChecked == true);
        _vhManeuverD.ValueChanged += (_, _) => SyncVh(m => m.ManeuverDice = (int)(_vhManeuverD!.Value ?? 0));
        _vhManeuverP.ValueChanged += (_, _) => SyncVh(m => m.ManeuverPips = (int)(_vhManeuverP!.Value ?? 0));
        _vhResolve.ValueChanged += (_, _) => SyncVh(m => m.Resolve = (int)(_vhResolve!.Value ?? 0));
        _vhShield.TextChanged += (_, _) => SyncVh(m => m.ShieldMember = _vhShield!.Text ?? "");
        _vhWeapons.TextChanged += (_, _) => SyncVh(m => m.Weapons = ParseVehicleWeapons(_vhWeapons!.Text));
        _vhEquipment.TextChanged += (_, _) => SyncVh(m => m.Equipment = ParseVehicleEquipment(_vhEquipment!.Text));
        _vhPrice.ValueChanged += (_, _) => SyncVh(m => m.Price = (int)(_vhPrice!.Value ?? 0));

        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("260,*") };
        Grid.SetColumn(listPane, 0); grid.Children.Add(listPane);
        var sv = new ScrollViewer { Content = form };
        Grid.SetColumn(sv, 1); grid.Children.Add(sv);
        VehicleView.Padding = default;
        VehicleView.Child = grid;
    }

    private void SyncVh(Action<VehicleModel> apply) { if (_vhSync || _selectedVh == null) return; apply(_selectedVh); }

    private void RefreshVhList()
    {
        if (_vhList == null) return;
        var f = (_vhFilter?.Text ?? "").Trim().ToLowerInvariant();
        var visible = string.IsNullOrEmpty(f) ? _vhs.ToList()
            : _vhs.Where(v => v.MemberName.ToLowerInvariant().Contains(f) || v.Name.ToLowerInvariant().Contains(f)).ToList();
        _vhList.ItemsSource = visible;
        if (_selectedVh != null && visible.Contains(_selectedVh))
            _vhList.SelectedItem = _selectedVh;
        else
            _selectedVh = null;
    }

    private void RefreshVhForm()
    {
        if (_selectedVh == null) return;
        _vhSync = true;
        try
        {
            _vhMember!.Text = _selectedVh.MemberName;
            _vhName!.Text = _selectedVh.Name;
            _vhDesc!.Text = _selectedVh.Description;
            _vhIsSpace!.IsChecked = _selectedVh.IsSpace;
            _vhManeuverD!.Value = _selectedVh.ManeuverDice;
            _vhManeuverP!.Value = _selectedVh.ManeuverPips;
            _vhResolve!.Value = _selectedVh.Resolve;
            _vhShield!.Text = _selectedVh.ShieldMember;
            _vhWeapons!.Text = string.Join(", ", _selectedVh.Weapons.Select(w => $"{w.Name}|{w.DamageDice}D{(w.DamagePips > 0 ? "+" + w.DamagePips : "")}|{w.AttackSkill}"));
            _vhEquipment!.Text = string.Join(", ", _selectedVh.Equipment.Select(eq => $"{eq.Name}|{eq.BonusSkill}|{eq.BonusDice}D{(eq.BonusPips > 0 ? "+" + eq.BonusPips : "")}"));
            _vhPrice!.Value = _selectedVh.Price;
        }
        finally { _vhSync = false; }
    }

    private static (int d, int p) ParseDicePart(string s)
    {
        s = s.Trim().ToUpperInvariant();
        var di = s.IndexOf('D');
        if (di <= 0) return (0, 0);
        int.TryParse(s.Substring(0, di), out int d);
        int p = 0;
        if (di + 1 < s.Length && s[di + 1] == '+') int.TryParse(s.Substring(di + 2), out p);
        return (d, p);
    }

    private static List<VehicleWeaponModel> ParseVehicleWeapons(string? text)
    {
        var result = new List<VehicleWeaponModel>();
        if (string.IsNullOrWhiteSpace(text)) return result;
        foreach (var entry in text.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = entry.Split('|', StringSplitOptions.TrimEntries);
            if (parts.Length < 3) continue;
            var (d, p) = ParseDicePart(parts[1]);
            result.Add(new VehicleWeaponModel { Name = parts[0], DamageDice = d, DamagePips = p, AttackSkill = parts[2] });
        }
        return result;
    }

    private static List<VehicleEquipmentModel> ParseVehicleEquipment(string? text)
    {
        var result = new List<VehicleEquipmentModel>();
        if (string.IsNullOrWhiteSpace(text)) return result;
        foreach (var entry in text.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = entry.Split('|', StringSplitOptions.TrimEntries);
            if (parts.Length < 3) continue;
            var (d, p) = ParseDicePart(parts[2]);
            result.Add(new VehicleEquipmentModel { Name = parts[0], BonusSkill = parts[1], BonusDice = d, BonusPips = p });
        }
        return result;
    }

    private void LoadVehicles()
    {
        BuildVehicleViewIfNeeded();
        var path = PathBox.Text ?? "";
        if (!File.Exists(path)) { Status($"file not found: {path}", error: true); return; }
        _vhParser = new VehicleFileParser(path);
        if (!_vhParser.TryLoad()) { Status($"parse failed: {_vhParser.Error}", error: true); return; }
        _vhs = _vhParser.Vehicles.ToList();
        _selectedVh = null;
        if (_vhList != null) _vhList.SelectedItem = null;
        RefreshVhList();
        Status($"loaded {_vhs.Count} vehicles");
    }

    private void SaveVehicles()
    {
        if (_vhParser == null) { Status("nothing loaded", error: true); return; }
        try { VehicleFileWriter.Save(_vhParser, _vhs); Status($"saved {_vhs.Count} vehicles"); LoadVehicles(); }
        catch (Exception ex) { Status($"save failed: {ex.Message}", error: true); }
    }
}
