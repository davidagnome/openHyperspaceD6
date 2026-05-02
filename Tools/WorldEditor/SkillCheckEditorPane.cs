using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace TerminalHyperspace.WorldEditor;

public partial class MainWindow
{
    private static readonly string[] Difficulties = { "Easy", "Moderate", "Difficult", "Challenging" };

    private SkillCheckFileParser? _scParser;
    private List<SkillCheckModel> _scs = new();
    private SkillCheckModel? _selectedSc;
    private ListBox? _scList;
    private TextBox? _scFilter, _scMember, _scId, _scDesc, _scSuccess, _scFail, _scPenaltyText;
    private ComboBox? _scSkill, _scDifficulty, _scNpc;
    private NumericUpDown? _scTN, _scReward, _scUp, _scPenalty;
    private CheckBox? _scRepeatable;
    private bool _scBuilt, _scSync;

    private void BuildSkillCheckViewIfNeeded()
    {
        if (_scBuilt) return;
        _scBuilt = true;

        var (listPane, filter, add, del, list) = EditorHelpers.BuildListPane("Add new check", "Delete selected check");
        _scFilter = filter; _scList = list;
        filter.KeyUp += (_, _) => RefreshScList();
        list.SelectionChanged += (_, _) => { _selectedSc = list.SelectedItem as SkillCheckModel; RefreshScForm(); };
        add.Click += (_, _) => { var m = new SkillCheckModel { MemberName = $"NewCheck{_scs.Count + 1}", Skill = "Persuade", Difficulty = "Moderate", TargetNumber = 12 }; _scs.Add(m); RefreshScList(); list.SelectedItem = m; };
        del.Click += (_, _) => { if (_selectedSc != null) { _scs.Remove(_selectedSc); _selectedSc = null; RefreshScList(); RefreshScForm(); } };

        _scMember = EditorHelpers.NewTextBox();
        _scId = EditorHelpers.NewTextBox();
        _scDesc = EditorHelpers.NewTextBox(multiline: true);
        _scSuccess = EditorHelpers.NewTextBox(multiline: true);
        _scFail = EditorHelpers.NewTextBox(multiline: true);
        _scPenaltyText = EditorHelpers.NewTextBox(multiline: true);
        _scSkill = EditorHelpers.NewCombo(AllSkills);
        _scDifficulty = EditorHelpers.NewCombo(Difficulties);
        _scTN = EditorHelpers.NewNumeric(0, 99);
        _scReward = EditorHelpers.NewNumeric(0, 99999);
        _scUp = EditorHelpers.NewNumeric(0, 9);
        _scRepeatable = EditorHelpers.NewCheck("Repeatable");
        _scPenalty = EditorHelpers.NewNumeric(0, 99999);
        _scNpc = EditorHelpers.NewCombo(Array.Empty<string>());
        _scNpc.HorizontalAlignment = HorizontalAlignment.Stretch;

        var form = new StackPanel { Margin = new Avalonia.Thickness(12), Spacing = 4 };
        form.Children.Add(new TextBlock { Text = "Skill Check", FontSize = 18, FontWeight = FontWeight.Bold, Foreground = new SolidColorBrush(Color.Parse("#F7FAFB")) });
        form.Children.Add(EditorHelpers.FormRow("Member name", _scMember));
        form.Children.Add(EditorHelpers.FormRow("Id (snake_case)", _scId));
        form.Children.Add(EditorHelpers.FormRow("Description", _scDesc));
        form.Children.Add(EditorHelpers.FormRow("Success text", _scSuccess));
        form.Children.Add(EditorHelpers.FormRow("Fail text", _scFail));
        form.Children.Add(EditorHelpers.FormRow("Fail penalty text", _scPenaltyText));
        form.Children.Add(EditorHelpers.FormRow("Skill", _scSkill));
        form.Children.Add(EditorHelpers.FormRow("Difficulty", _scDifficulty));
        form.Children.Add(EditorHelpers.FormRow("Target number", _scTN));
        form.Children.Add(EditorHelpers.FormRow("Credit reward", _scReward));
        form.Children.Add(EditorHelpers.FormRow("Upgrade points", _scUp));
        form.Children.Add(EditorHelpers.FormRow("",                 _scRepeatable));
        form.Children.Add(EditorHelpers.FormRow("Credit penalty", _scPenalty));
        form.Children.Add(EditorHelpers.FormRow("Combat NPC on fail (NPCData ref)", _scNpc));

        _scMember.TextChanged += (_, _) => SyncSc(m => { m.MemberName = _scMember!.Text ?? ""; RefreshScList(); });
        _scId.TextChanged += (_, _) => SyncSc(m => m.Id = _scId!.Text ?? "");
        _scDesc.TextChanged += (_, _) => SyncSc(m => m.Description = _scDesc!.Text ?? "");
        _scSuccess.TextChanged += (_, _) => SyncSc(m => m.SuccessText = _scSuccess!.Text ?? "");
        _scFail.TextChanged += (_, _) => SyncSc(m => m.FailText = _scFail!.Text ?? "");
        _scPenaltyText.TextChanged += (_, _) => SyncSc(m => m.FailPenaltyText = _scPenaltyText!.Text ?? "");
        _scSkill.SelectionChanged += (_, _) => SyncSc(m => { m.Skill = _scSkill!.SelectedItem as string ?? ""; RefreshScList(); });
        _scDifficulty.SelectionChanged += (_, _) => SyncSc(m => { m.Difficulty = _scDifficulty!.SelectedItem as string ?? "Moderate"; RefreshScList(); });
        _scTN.ValueChanged += (_, _) => SyncSc(m => m.TargetNumber = (int)(_scTN!.Value ?? 0));
        _scReward.ValueChanged += (_, _) => SyncSc(m => m.CreditReward = (int)(_scReward!.Value ?? 0));
        _scUp.ValueChanged += (_, _) => SyncSc(m => m.UpgradePointReward = (int)(_scUp!.Value ?? 0));
        _scRepeatable.IsCheckedChanged += (_, _) => SyncSc(m => m.Repeatable = _scRepeatable!.IsChecked == true);
        _scPenalty.ValueChanged += (_, _) => SyncSc(m => m.CreditPenalty = (int)(_scPenalty!.Value ?? 0));
        _scNpc.SelectionChanged += (_, _) => SyncSc(m => m.CombatNpcOnFail = _scNpc!.SelectedItem as string ?? "");

        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("260,*") };
        Grid.SetColumn(listPane, 0); grid.Children.Add(listPane);
        var sv = new ScrollViewer { Content = form };
        Grid.SetColumn(sv, 1); grid.Children.Add(sv);
        SkillCheckView.Padding = default;
        SkillCheckView.Child = grid;
    }

    private void SyncSc(Action<SkillCheckModel> apply) { if (_scSync || _selectedSc == null) return; apply(_selectedSc); }

    private void RefreshScList()
    {
        if (_scList == null) return;
        var f = (_scFilter?.Text ?? "").Trim().ToLowerInvariant();
        var visible = string.IsNullOrEmpty(f) ? _scs.ToList()
            : _scs.Where(c => c.MemberName.ToLowerInvariant().Contains(f) || c.Id.ToLowerInvariant().Contains(f) || c.Skill.ToLowerInvariant().Contains(f)).ToList();
        _scList.ItemsSource = visible;
        if (_selectedSc != null && visible.Contains(_selectedSc))
            _scList.SelectedItem = _selectedSc;
        else
            _selectedSc = null;
    }

    private void RefreshScForm()
    {
        if (_selectedSc == null) return;
        _scSync = true;
        try
        {
            _scMember!.Text = _selectedSc.MemberName;
            _scId!.Text = _selectedSc.Id;
            _scDesc!.Text = _selectedSc.Description;
            _scSuccess!.Text = _selectedSc.SuccessText;
            _scFail!.Text = _selectedSc.FailText;
            _scPenaltyText!.Text = _selectedSc.FailPenaltyText;
            _scSkill!.SelectedItem = string.IsNullOrEmpty(_selectedSc.Skill) ? null : _selectedSc.Skill;
            _scDifficulty!.SelectedItem = string.IsNullOrEmpty(_selectedSc.Difficulty) ? "Moderate" : _selectedSc.Difficulty;
            _scTN!.Value = _selectedSc.TargetNumber;
            _scReward!.Value = _selectedSc.CreditReward;
            _scUp!.Value = _selectedSc.UpgradePointReward;
            _scRepeatable!.IsChecked = _selectedSc.Repeatable;
            _scPenalty!.Value = _selectedSc.CreditPenalty;
            _scNpc!.SelectedItem = string.IsNullOrEmpty(_selectedSc.CombatNpcOnFail) ? null : _selectedSc.CombatNpcOnFail;
        }
        finally { _scSync = false; }
    }

    private void LoadSkillChecks()
    {
        BuildSkillCheckViewIfNeeded();
        var path = PathBox.Text ?? "";
        if (!File.Exists(path)) { Status($"file not found: {path}", error: true); return; }
        _scParser = new SkillCheckFileParser(path);
        if (!_scParser.TryLoad()) { Status($"parse failed: {_scParser.Error}", error: true); return; }
        _scs = _scParser.Checks.ToList();
        _selectedSc = null;
        if (_scList != null) _scList.SelectedItem = null;

        // Populate the "Combat NPC on fail" dropdown by scanning the sibling
        // NPCData.cs for Character factory members. A blank first entry lets
        // the user clear the selection.
        var contentDir = System.IO.Path.GetDirectoryName(path)!;
        var npcs = SymbolScanner.ScanFactories(System.IO.Path.Combine(contentDir, "NPCData.cs"), "Character");
        var choices = new List<string> { "" };
        choices.AddRange(npcs);
        if (_scNpc != null) _scNpc.ItemsSource = choices;

        RefreshScList();
        Status($"loaded {_scs.Count} checks");
    }

    private void SaveSkillChecks()
    {
        if (_scParser == null) { Status("nothing loaded", error: true); return; }
        try { SkillCheckFileWriter.Save(_scParser, _scs); Status($"saved {_scs.Count} checks"); LoadSkillChecks(); }
        catch (Exception ex) { Status($"save failed: {ex.Message}", error: true); }
    }
}
