using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace TerminalHyperspace.WorldEditor;

public partial class MainWindow
{
    private static readonly string[] DialogueSpeakers = { "npcName", "playerName" };

    private DialogueFileParser? _dlgParser;
    private List<DialogueModel> _dlgs = new();
    private DialogueModel? _selectedDlg;
    private ListBox? _dlgList;
    private TextBox? _dlgFilter, _dlgMember;
    private StackPanel? _dlgLineRows;
    private bool _dlgBuilt, _dlgSync;

    private void BuildDialogueViewIfNeeded()
    {
        if (_dlgBuilt) return;
        _dlgBuilt = true;

        var (listPane, filter, add, del, list) = EditorHelpers.BuildListPane("Add new dialogue", "Delete selected dialogue");
        _dlgFilter = filter; _dlgList = list;
        filter.KeyUp += (_, _) => RefreshDlgList();
        list.SelectionChanged += (_, _) => { _selectedDlg = list.SelectedItem as DialogueModel; RefreshDlgForm(); };
        add.Click += (_, _) =>
        {
            var d = new DialogueModel
            {
                MemberName = $"NewDialogue{_dlgs.Count + 1}",
                Lines = new() { new DialogueLineModel { Speaker = "npcName", Line = "..." } },
            };
            _dlgs.Add(d); RefreshDlgList(); list.SelectedItem = d;
        };
        del.Click += (_, _) => { if (_selectedDlg != null) { _dlgs.Remove(_selectedDlg); _selectedDlg = null; RefreshDlgList(); RefreshDlgForm(); } };

        _dlgMember = EditorHelpers.NewTextBox();
        _dlgLineRows = new StackPanel { Spacing = 3 };

        var form = new StackPanel { Margin = new Avalonia.Thickness(12), Spacing = 4 };
        form.Children.Add(new TextBlock { Text = "Dialogue", FontSize = 18, FontWeight = FontWeight.Bold, Foreground = new SolidColorBrush(Color.Parse("#F7FAFB")) });
        form.Children.Add(EditorHelpers.FormRow("Member name", _dlgMember));
        form.Children.Add(new TextBlock { Text = "Lines (Speaker uses the runtime parameter name — npcName or playerName)",
            FontStyle = FontStyle.Italic, Foreground = new SolidColorBrush(Color.Parse("#88909A")), Margin = new Avalonia.Thickness(0, 6, 0, 2) });
        form.Children.Add(_dlgLineRows);

        _dlgMember.TextChanged += (_, _) => SyncDlg(d => { d.MemberName = _dlgMember!.Text ?? ""; RefreshDlgList(); });

        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("260,*") };
        Grid.SetColumn(listPane, 0); grid.Children.Add(listPane);
        var sv = new ScrollViewer { Content = form };
        Grid.SetColumn(sv, 1); grid.Children.Add(sv);
        DialogueView.Padding = default;
        DialogueView.Child = grid;
    }

    private void SyncDlg(Action<DialogueModel> apply) { if (_dlgSync || _selectedDlg == null) return; apply(_selectedDlg); }

    private void RefreshDlgList()
    {
        if (_dlgList == null) return;
        var f = (_dlgFilter?.Text ?? "").Trim().ToLowerInvariant();
        var visible = string.IsNullOrEmpty(f) ? _dlgs.ToList()
            : _dlgs.Where(d => d.MemberName.ToLowerInvariant().Contains(f)).ToList();
        _dlgList.ItemsSource = visible;
        if (_selectedDlg != null && visible.Contains(_selectedDlg))
            _dlgList.SelectedItem = _selectedDlg;
        else
            _selectedDlg = null;
    }

    private void RefreshDlgForm()
    {
        if (_selectedDlg == null)
        {
            _dlgLineRows?.Children.Clear();
            return;
        }
        _dlgSync = true;
        try
        {
            _dlgMember!.Text = _selectedDlg.MemberName;
            BuildDialogueLineRows(_dlgLineRows!, _selectedDlg.Lines);
        }
        finally { _dlgSync = false; }
    }

    /// One row per line: [Speaker combo][Line textbox][✕]. An "+ Add line"
    /// button at the bottom appends a fresh line.
    private void BuildDialogueLineRows(StackPanel container, List<DialogueLineModel> list)
    {
        container.Children.Clear();
        for (int i = 0; i < list.Count; i++)
        {
            var idx = i;
            var entry = list[idx];

            var speaker = EditorHelpers.NewCombo(DialogueSpeakers);
            speaker.SelectedItem = DialogueSpeakers.Contains(entry.Speaker) ? entry.Speaker : "npcName";
            speaker.Width = 130;
            speaker.SelectionChanged += (_, _) =>
            {
                if (idx >= list.Count) return;
                if (speaker.SelectedItem is string s) list[idx].Speaker = s;
            };

            var line = EditorHelpers.NewTextBox(minWidth: 320, multiline: true);
            line.Text = entry.Line;
            line.MinHeight = 40;
            line.Margin = new Avalonia.Thickness(4, 0, 0, 0);
            line.TextChanged += (_, _) => { if (idx < list.Count) list[idx].Line = line.Text ?? ""; };

            var del = new Button { Content = "✕", Padding = new Avalonia.Thickness(8, 2), Margin = new Avalonia.Thickness(6, 0, 0, 0) };
            ToolTip.SetTip(del, "Delete this line");
            del.Click += (_, _) => { if (idx < list.Count) list.RemoveAt(idx); BuildDialogueLineRows(container, list); };

            var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto") };
            Grid.SetColumn(speaker, 0); grid.Children.Add(speaker);
            Grid.SetColumn(line, 1);    grid.Children.Add(line);
            Grid.SetColumn(del, 2);     grid.Children.Add(del);
            container.Children.Add(grid);
        }
        var addBtn = new Button { Content = "+ Add line", Padding = new Avalonia.Thickness(10, 2), Margin = new Avalonia.Thickness(0, 4, 0, 0) };
        addBtn.Click += (_, _) =>
        {
            list.Add(new DialogueLineModel { Speaker = "npcName", Line = "" });
            BuildDialogueLineRows(container, list);
        };
        container.Children.Add(addBtn);
    }

    private void LoadDialogues()
    {
        BuildDialogueViewIfNeeded();
        var path = PathBox.Text ?? "";
        if (!File.Exists(path)) { Status($"file not found: {path}", error: true); return; }
        _dlgParser = new DialogueFileParser(path);
        if (!_dlgParser.TryLoad()) { Status($"parse failed: {_dlgParser.Error}", error: true); return; }
        _dlgs = _dlgParser.Dialogues.ToList();
        _selectedDlg = null;
        if (_dlgList != null) _dlgList.SelectedItem = null;
        RefreshDlgList();
        Status($"loaded {_dlgs.Count} dialogues");
    }

    private void SaveDialogues()
    {
        if (_dlgParser == null) { Status("nothing loaded", error: true); return; }
        try { DialogueFileWriter.Save(_dlgParser, _dlgs); Status($"saved {_dlgs.Count} dialogues"); LoadDialogues(); }
        catch (Exception ex) { Status($"save failed: {ex.Message}", error: true); }
    }
}
