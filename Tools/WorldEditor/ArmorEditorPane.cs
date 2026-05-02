using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace TerminalHyperspace.WorldEditor;

public partial class MainWindow
{
    private static readonly string[] ArmorClimates = { "Normal", "Hot", "Cold", "Aquatic" };

    private ArmorFileParser? _armorParser;
    private List<ArmorModel> _armors = new();
    private ArmorModel? _selectedArmor;
    private ListBox? _armorList;
    private TextBox? _armorFilter, _armorMember, _armorName;
    private NumericUpDown? _armorDice, _armorPrice;
    private ComboBox? _armorClimate;
    private CheckBox? _armorPurchasable;
    private bool _armorBuilt, _armorSync;

    private void BuildArmorViewIfNeeded()
    {
        if (_armorBuilt) return;
        _armorBuilt = true;

        var (listPane, filter, add, del, list) = EditorHelpers.BuildListPane("Add new armor", "Delete selected armor");
        _armorFilter = filter; _armorList = list;
        filter.KeyUp += (_, _) => RefreshArmorList();
        list.SelectionChanged += (_, _) => { _selectedArmor = list.SelectedItem as ArmorModel; RefreshArmorForm(); };
        add.Click += (_, _) =>
        {
            var m = new ArmorModel { MemberName = $"NewArmor{_armors.Count + 1}", Name = "New Armor", Dice = 1, Price = 100, Purchasable = true };
            _armors.Add(m); RefreshArmorList(); list.SelectedItem = m;
        };
        del.Click += (_, _) => { if (_selectedArmor != null) { _armors.Remove(_selectedArmor); _selectedArmor = null; RefreshArmorList(); RefreshArmorForm(); } };

        _armorMember      = EditorHelpers.NewTextBox();
        _armorName        = EditorHelpers.NewTextBox();
        _armorDice        = EditorHelpers.NewNumeric(0, 12);
        _armorPrice       = EditorHelpers.NewNumeric(0, 999999);
        _armorClimate     = EditorHelpers.NewCombo(ArmorClimates);
        _armorPurchasable = EditorHelpers.NewCheck("Purchasable (include in shops)");

        var form = new StackPanel { Margin = new Avalonia.Thickness(12), Spacing = 4 };
        form.Children.Add(new TextBlock { Text = "Armor", FontSize = 18, FontWeight = FontWeight.Bold, Foreground = new SolidColorBrush(Color.Parse("#F7FAFB")) });
        form.Children.Add(EditorHelpers.FormRow("Member name (Id)", _armorMember));
        form.Children.Add(EditorHelpers.FormRow("Display name",     _armorName));
        form.Children.Add(EditorHelpers.FormRow("Dice (resistance)", _armorDice));
        form.Children.Add(EditorHelpers.FormRow("Price (cr)",       _armorPrice));
        form.Children.Add(EditorHelpers.FormRow("Climate",          _armorClimate));
        form.Children.Add(EditorHelpers.FormRow("",                 _armorPurchasable));

        _armorMember.TextChanged    += (_, _) => SyncArmor(m => { m.MemberName = _armorMember!.Text ?? ""; RefreshArmorList(); });
        _armorName.TextChanged      += (_, _) => SyncArmor(m => { m.Name = _armorName!.Text ?? ""; RefreshArmorList(); });
        _armorDice.ValueChanged     += (_, _) => SyncArmor(m => m.Dice = (int)(_armorDice!.Value ?? 0));
        _armorPrice.ValueChanged    += (_, _) => SyncArmor(m => m.Price = (int)(_armorPrice!.Value ?? 0));
        _armorClimate.SelectionChanged += (_, _) => SyncArmor(m => m.Climate = _armorClimate!.SelectedItem as string ?? "Normal");
        _armorPurchasable.IsCheckedChanged += (_, _) => SyncArmor(m => m.Purchasable = _armorPurchasable!.IsChecked == true);

        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("260,*") };
        Grid.SetColumn(listPane, 0); grid.Children.Add(listPane);
        var sv = new ScrollViewer { Content = form };
        Grid.SetColumn(sv, 1); grid.Children.Add(sv);
        ArmorView.Padding = default;
        ArmorView.Child = grid;
    }

    private void SyncArmor(Action<ArmorModel> apply) { if (_armorSync || _selectedArmor == null) return; apply(_selectedArmor); }

    private void RefreshArmorList()
    {
        if (_armorList == null) return;
        var f = (_armorFilter?.Text ?? "").Trim().ToLowerInvariant();
        var visible = string.IsNullOrEmpty(f) ? _armors.ToList()
            : _armors.Where(a => a.MemberName.ToLowerInvariant().Contains(f) || a.Name.ToLowerInvariant().Contains(f)).ToList();
        _armorList.ItemsSource = visible;
        if (_selectedArmor != null && visible.Contains(_selectedArmor))
            _armorList.SelectedItem = _selectedArmor;
        else
            _selectedArmor = null;
    }

    private void RefreshArmorForm()
    {
        if (_selectedArmor == null) return;
        _armorSync = true;
        try
        {
            _armorMember!.Text  = _selectedArmor.MemberName;
            _armorName!.Text    = _selectedArmor.Name;
            _armorDice!.Value   = _selectedArmor.Dice;
            _armorPrice!.Value  = _selectedArmor.Price;
            _armorClimate!.SelectedItem = string.IsNullOrEmpty(_selectedArmor.Climate) ? "Normal" : _selectedArmor.Climate;
            _armorPurchasable!.IsChecked = _selectedArmor.Purchasable;
        }
        finally { _armorSync = false; }
    }

    private void LoadArmors()
    {
        BuildArmorViewIfNeeded();
        var path = PathBox.Text ?? "";
        if (!File.Exists(path)) { Status($"file not found: {path}", error: true); return; }
        _armorParser = new ArmorFileParser(path);
        if (!_armorParser.TryLoad()) { Status($"parse failed: {_armorParser.Error}", error: true); return; }
        _armors = _armorParser.Armors.ToList();
        _selectedArmor = null;
        if (_armorList != null) _armorList.SelectedItem = null;
        RefreshArmorList();
        Status($"loaded {_armors.Count} armors");
    }

    private void SaveArmors()
    {
        if (_armorParser == null) { Status("nothing loaded", error: true); return; }
        try { ArmorFileWriter.Save(_armorParser, _armors); Status($"saved {_armors.Count} armors"); LoadArmors(); }
        catch (Exception ex) { Status($"save failed: {ex.Message}", error: true); }
    }
}
