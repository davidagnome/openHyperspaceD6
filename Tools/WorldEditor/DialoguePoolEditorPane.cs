using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace TerminalHyperspace.WorldEditor;

public partial class MainWindow
{
    private DialogueFileParser? _dlgPoolParser;
    private List<DialoguePoolModel> _dlgPools = new();
    private DialoguePoolModel? _selectedDlgPool;
    private ListBox? _dlgPoolList;
    private TextBox? _dlgPoolFilter, _dlgPoolName;
    private StackPanel? _dlgPoolMemberRows;
    private bool _dlgPoolBuilt, _dlgPoolSync;

    /// Available Dialogue factory names (from the parser's Dialogues list),
    /// used to populate the pool member dropdowns.
    private List<string> _dlgFactoryChoices = new();

    private void BuildDialoguePoolViewIfNeeded()
    {
        if (_dlgPoolBuilt) return;
        _dlgPoolBuilt = true;

        var (listPane, filter, add, del, list) = EditorHelpers.BuildListPane("Add new pool", "Delete selected pool");
        _dlgPoolFilter = filter; _dlgPoolList = list;
        filter.KeyUp += (_, _) => RefreshDlgPoolList();
        list.SelectionChanged += (_, _) => { _selectedDlgPool = list.SelectedItem as DialoguePoolModel; RefreshDlgPoolForm(); };
        add.Click += (_, _) =>
        {
            var p = new DialoguePoolModel { PoolName = $"NewPool{_dlgPools.Count + 1}" };
            _dlgPools.Add(p); RefreshDlgPoolList(); list.SelectedItem = p;
        };
        del.Click += (_, _) => { if (_selectedDlgPool != null) { _dlgPools.Remove(_selectedDlgPool); _selectedDlgPool = null; RefreshDlgPoolList(); RefreshDlgPoolForm(); } };

        _dlgPoolName       = EditorHelpers.NewTextBox();
        _dlgPoolMemberRows = new StackPanel { Spacing = 3 };

        var form = new StackPanel { Margin = new Avalonia.Thickness(12), Spacing = 4 };
        form.Children.Add(new TextBlock { Text = "Dialogue Pool", FontSize = 18, FontWeight = FontWeight.Bold, Foreground = new SolidColorBrush(Color.Parse("#F7FAFB")) });
        form.Children.Add(EditorHelpers.FormRow("Pool name", _dlgPoolName));
        form.Children.Add(new TextBlock { Text = "Dialogue members",
            FontStyle = FontStyle.Italic, Foreground = new SolidColorBrush(Color.Parse("#88909A")), Margin = new Avalonia.Thickness(0, 6, 0, 2) });
        form.Children.Add(_dlgPoolMemberRows);

        _dlgPoolName.TextChanged += (_, _) => SyncDlgPool(p => { p.PoolName = _dlgPoolName!.Text ?? ""; RefreshDlgPoolList(); });

        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("260,*") };
        Grid.SetColumn(listPane, 0); grid.Children.Add(listPane);
        var sv = new ScrollViewer { Content = form };
        Grid.SetColumn(sv, 1); grid.Children.Add(sv);
        DialoguePoolView.Padding = default;
        DialoguePoolView.Child = grid;
    }

    private void SyncDlgPool(Action<DialoguePoolModel> apply) { if (_dlgPoolSync || _selectedDlgPool == null) return; apply(_selectedDlgPool); }

    private void RefreshDlgPoolList()
    {
        if (_dlgPoolList == null) return;
        var f = (_dlgPoolFilter?.Text ?? "").Trim().ToLowerInvariant();
        var visible = string.IsNullOrEmpty(f) ? _dlgPools.ToList()
            : _dlgPools.Where(p => p.PoolName.ToLowerInvariant().Contains(f)).ToList();
        _dlgPoolList.ItemsSource = visible;
        if (_selectedDlgPool != null && visible.Contains(_selectedDlgPool))
            _dlgPoolList.SelectedItem = _selectedDlgPool;
        else
            _selectedDlgPool = null;
    }

    private void RefreshDlgPoolForm()
    {
        if (_selectedDlgPool == null)
        {
            _dlgPoolMemberRows?.Children.Clear();
            return;
        }
        _dlgPoolSync = true;
        try
        {
            _dlgPoolName!.Text = _selectedDlgPool.PoolName;
            BuildDlgPoolMemberRows(_dlgPoolMemberRows!, _selectedDlgPool.FactoryNames);
        }
        finally { _dlgPoolSync = false; }
    }

    /// Each member row: [Dialogue ComboBox][✕]; an Add row at the bottom.
    /// Reuses the choice list scanned from DialogueData on load.
    private void BuildDlgPoolMemberRows(StackPanel container, List<string> list)
    {
        container.Children.Clear();
        for (int i = 0; i < list.Count; i++)
        {
            var idx = i;
            var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto") };
            var cb = EditorHelpers.NewCombo(_dlgFactoryChoices);
            cb.SelectedItem = list[idx];
            cb.HorizontalAlignment = HorizontalAlignment.Stretch;
            cb.SelectionChanged += (_, _) =>
            {
                if (idx >= list.Count) return;
                if (cb.SelectedItem is string s) list[idx] = s;
            };
            Grid.SetColumn(cb, 0);

            var del = new Button { Content = "✕", Padding = new Avalonia.Thickness(8, 2), Margin = new Avalonia.Thickness(4, 0, 0, 0) };
            ToolTip.SetTip(del, "Remove from pool");
            del.Click += (_, _) => { if (idx < list.Count) list.RemoveAt(idx); BuildDlgPoolMemberRows(container, list); };
            Grid.SetColumn(del, 1);

            grid.Children.Add(cb);
            grid.Children.Add(del);
            container.Children.Add(grid);
        }

        var addCombo = EditorHelpers.NewCombo(_dlgFactoryChoices);
        addCombo.PlaceholderText = "(add…)";
        addCombo.HorizontalAlignment = HorizontalAlignment.Stretch;
        var add = new Button { Content = "Add", Padding = new Avalonia.Thickness(10, 2), Margin = new Avalonia.Thickness(6, 0, 0, 0), IsEnabled = _dlgFactoryChoices.Count > 0 };
        add.Click += (_, _) =>
        {
            if (addCombo.SelectedItem is not string s) return;
            list.Add(s);
            BuildDlgPoolMemberRows(container, list);
        };
        var row = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto"), Margin = new Avalonia.Thickness(0, 6, 0, 0) };
        Grid.SetColumn(addCombo, 0); row.Children.Add(addCombo);
        Grid.SetColumn(add, 1);      row.Children.Add(add);
        container.Children.Add(row);
    }

    private void LoadDialoguePools()
    {
        BuildDialoguePoolViewIfNeeded();
        var path = PathBox.Text ?? "";
        if (!File.Exists(path)) { Status($"file not found: {path}", error: true); return; }
        _dlgPoolParser = new DialogueFileParser(path);
        if (!_dlgPoolParser.TryLoad()) { Status($"parse failed: {_dlgPoolParser.Error}", error: true); return; }
        _dlgPools = _dlgPoolParser.Pools.ToList();
        _dlgFactoryChoices = _dlgPoolParser.Dialogues.Select(d => d.MemberName).OrderBy(s => s).ToList();
        _selectedDlgPool = null;
        if (_dlgPoolList != null) _dlgPoolList.SelectedItem = null;
        RefreshDlgPoolList();
        RefreshDlgPoolForm();
        Status($"loaded {_dlgPools.Count} pools, {_dlgFactoryChoices.Count} dialogue factories");
    }

    private void SaveDialoguePools()
    {
        if (_dlgPoolParser == null) { Status("nothing loaded", error: true); return; }
        try { DialoguePoolFileWriter.Save(_dlgPoolParser, _dlgPools); Status($"saved {_dlgPools.Count} pools"); LoadDialoguePools(); }
        catch (Exception ex) { Status($"save failed: {ex.Message}", error: true); }
    }
}
