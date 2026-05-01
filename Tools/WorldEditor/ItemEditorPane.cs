using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace TerminalHyperspace.WorldEditor;

public partial class MainWindow
{
    private static readonly string[] AllSkills =
    {
        "Agility","Blasters","Melee","Steal","Throw",
        "Galaxy","Streetwise","Survival","Willpower","Xenology",
        "Astrogation","Drive","Gunnery","Pilot","Sensors",
        "Deceive","Hide","Persuade","Search","Tactics",
        "Athletics","Brawl","Intimidate","Stamina","Swim",
        "Armament","Computers","Droids","Medicine","Vehicles",
        "Alter","Control","Sense"
    };

    private ItemFileParser? _itemParser;
    private List<ItemModel> _items = new();
    private ItemModel? _selectedItem;
    private ListBox? _itemList;
    private TextBox? _itemFilterBox;
    private TextBox? _itemMember = null!, _itemName = null!, _itemDesc = null!;
    private NumericUpDown? _itemDmgD = null!, _itemDmgP = null!, _itemRange = null!, _itemPrice = null!;
    private ComboBox? _itemSkill = null!;
    private CheckBox? _itemIsWeapon = null!, _itemIsCons = null!;
    private bool _itemBuilt;
    private bool _itemSync;

    private void BuildItemViewIfNeeded()
    {
        if (_itemBuilt) return;
        _itemBuilt = true;

        var (listPane, filter, add, del, list) = EditorHelpers.BuildListPane("Add new item", "Delete selected item");
        _itemFilterBox = filter; _itemList = list;
        filter.KeyUp += (_, _) => RefreshItemList();
        list.SelectionChanged += (_, _) =>
        {
            _selectedItem = list.SelectedItem is ItemModel m ? m : null;
            RefreshItemForm();
        };
        add.Click += (_, _) =>
        {
            var m = new ItemModel { MemberName = $"NewItem{_items.Count + 1}", Name = "New Item", Price = 10 };
            _items.Add(m); RefreshItemList(); list.SelectedItem = m;
        };
        del.Click += (_, _) =>
        {
            if (_selectedItem == null) return;
            _items.Remove(_selectedItem); _selectedItem = null;
            RefreshItemList(); RefreshItemForm();
        };

        // Form
        _itemMember = EditorHelpers.NewTextBox();
        _itemName   = EditorHelpers.NewTextBox();
        _itemDesc   = EditorHelpers.NewTextBox(multiline: true);
        _itemIsWeapon = EditorHelpers.NewCheck("IsWeapon");
        _itemIsCons   = EditorHelpers.NewCheck("IsConsumable");
        _itemDmgD = EditorHelpers.NewNumeric(0, 12);
        _itemDmgP = EditorHelpers.NewNumeric(0, 2);
        _itemSkill = EditorHelpers.NewCombo(AllSkills);
        _itemRange = EditorHelpers.NewNumeric(0, 200);
        _itemPrice = EditorHelpers.NewNumeric(0, 99999);

        var dmgRow = new StackPanel { Orientation = Orientation.Horizontal };
        dmgRow.Children.Add(_itemDmgD);
        dmgRow.Children.Add(new TextBlock { Text = "D+", VerticalAlignment = VerticalAlignment.Center, Margin = new Avalonia.Thickness(4, 0) });
        dmgRow.Children.Add(_itemDmgP);

        var form = new StackPanel { Margin = new Avalonia.Thickness(12), Spacing = 4 };
        form.Children.Add(new TextBlock { Text = "Item", FontSize = 18, FontWeight = FontWeight.Bold, Foreground = new SolidColorBrush(Color.Parse("#F7FAFB")) });
        form.Children.Add(EditorHelpers.FormRow("Member name (Id)", _itemMember));
        form.Children.Add(EditorHelpers.FormRow("Display name",     _itemName));
        form.Children.Add(EditorHelpers.FormRow("Description",      _itemDesc));
        form.Children.Add(EditorHelpers.FormRow("Flags",            _itemIsWeapon));
        form.Children.Add(EditorHelpers.FormRow("",                 _itemIsCons));
        form.Children.Add(EditorHelpers.FormRow("Damage (dice/pips)", dmgRow));
        form.Children.Add(EditorHelpers.FormRow("Attack skill",     _itemSkill));
        form.Children.Add(EditorHelpers.FormRow("Range (m)",        _itemRange));
        form.Children.Add(EditorHelpers.FormRow("Price (cr)",       _itemPrice));

        // Two-way binding back to the model.
        _itemMember.TextChanged += (_, _) => SyncItem(m => m.MemberName = _itemMember!.Text ?? "");
        _itemName.TextChanged   += (_, _) => SyncItem(m => { m.Name = _itemName!.Text ?? ""; RefreshItemList(); });
        _itemDesc.TextChanged   += (_, _) => SyncItem(m => m.Description = _itemDesc!.Text ?? "");
        _itemIsWeapon.IsCheckedChanged += (_, _) => SyncItem(m => m.IsWeapon = _itemIsWeapon!.IsChecked == true);
        _itemIsCons.IsCheckedChanged   += (_, _) => SyncItem(m => m.IsConsumable = _itemIsCons!.IsChecked == true);
        _itemDmgD.ValueChanged += (_, _) => SyncItem(m => m.DamageDice = (int)(_itemDmgD!.Value ?? 0));
        _itemDmgP.ValueChanged += (_, _) => SyncItem(m => m.DamagePips = (int)(_itemDmgP!.Value ?? 0));
        _itemSkill.SelectionChanged += (_, _) => SyncItem(m => m.AttackSkill = _itemSkill!.SelectedItem as string ?? "");
        _itemRange.ValueChanged += (_, _) => SyncItem(m => m.Range = (int)(_itemRange!.Value ?? 0));
        _itemPrice.ValueChanged += (_, _) => SyncItem(m => m.Price = (int)(_itemPrice!.Value ?? 0));

        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("260,*") };
        Grid.SetColumn(listPane, 0); grid.Children.Add(listPane);
        var formScroll = new ScrollViewer { Content = form };
        Grid.SetColumn(formScroll, 1); grid.Children.Add(formScroll);
        ItemView.Padding = default;
        ItemView.Child = grid;
    }

    private void SyncItem(Action<ItemModel> apply)
    {
        if (_itemSync || _selectedItem == null) return;
        apply(_selectedItem);
    }

    private void RefreshItemList()
    {
        if (_itemList == null) return;
        var f = (_itemFilterBox?.Text ?? "").Trim().ToLowerInvariant();
        var visible = string.IsNullOrEmpty(f)
            ? _items.ToList()
            : _items.Where(i => i.MemberName.ToLowerInvariant().Contains(f) || i.Name.ToLowerInvariant().Contains(f)).ToList();
        _itemList.ItemsSource = visible;
        // Only restore the selection if the model is actually in the rebuilt
        // ItemsSource. Setting SelectedItem to a stale reference can fault the
        // ListBox's internal index lookup.
        if (_selectedItem != null && visible.Contains(_selectedItem))
            _itemList.SelectedItem = _selectedItem;
        else
            _selectedItem = null;
    }

    private void RefreshItemForm()
    {
        if (_selectedItem == null) return;
        _itemSync = true;
        try
        {
            _itemMember!.Text = _selectedItem.MemberName;
            _itemName!.Text   = _selectedItem.Name;
            _itemDesc!.Text   = _selectedItem.Description;
            _itemIsWeapon!.IsChecked = _selectedItem.IsWeapon;
            _itemIsCons!.IsChecked   = _selectedItem.IsConsumable;
            _itemDmgD!.Value  = _selectedItem.DamageDice;
            _itemDmgP!.Value  = _selectedItem.DamagePips;
            _itemSkill!.SelectedItem = string.IsNullOrEmpty(_selectedItem.AttackSkill) ? null : _selectedItem.AttackSkill;
            _itemRange!.Value = _selectedItem.Range;
            _itemPrice!.Value = _selectedItem.Price;
        }
        finally { _itemSync = false; }
    }

    private void LoadItems()
    {
        BuildItemViewIfNeeded();
        var path = PathBox.Text ?? "";
        if (!File.Exists(path)) { Status($"file not found: {path}", error: true); return; }
        _itemParser = new ItemFileParser(path);
        if (!_itemParser.TryLoad()) { Status($"parse failed: {_itemParser.Error}", error: true); return; }
        _items = _itemParser.Items.ToList();
        _selectedItem = null;
        if (_itemList != null) _itemList.SelectedItem = null;
        RefreshItemList();
        Status($"loaded {_items.Count} items");
    }

    private void SaveItems()
    {
        if (_itemParser == null) { Status("nothing loaded", error: true); return; }
        try
        {
            ItemFileWriter.Save(_itemParser, _items);
            Status($"saved {_items.Count} items");
            LoadItems();
        }
        catch (Exception ex) { Status($"save failed: {ex.Message}", error: true); }
    }
}

// ListBox displays ItemModel by ToString().
public static class ItemModelDisplay
{
    public static string Describe(this ItemModel m) => $"{m.MemberName} — {m.Name}";
}
