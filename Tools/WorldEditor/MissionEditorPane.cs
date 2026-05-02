using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace TerminalHyperspace.WorldEditor;

public partial class MainWindow
{
    private static readonly string[] MissionTypes = { "Escort", "Delivery", "Sabotage", "Recon" };

    private MissionFileParser? _msnParser;
    private List<MissionModel> _msns = new();
    private MissionModel? _selectedMsn;
    private ListBox? _msnList;
    private TextBox? _msnFilter, _msnMember, _msnId, _msnTitle, _msnBriefing, _msnEscort,
                     _msnSuccess, _msnFail;
    private ComboBox? _msnType, _msnCheckSkill, _msnDest, _msnItemDestId;
    private NumericUpDown? _msnTN, _msnReward, _msnUp;
    private CheckBox? _msnHasItem;
    private TextBox? _msnItemName, _msnItemDesc, _msnItemDestName;
    private bool _msnBuilt, _msnSync;

    /// (Id, DisplayName) pairs scanned from sibling LocationData.cs so the
    /// destination dropdowns offer real ids and we can auto-fill the
    /// destination name from a chosen id.
    private List<(string Id, string Name)> _msnLocations = new();

    private void BuildMissionViewIfNeeded()
    {
        if (_msnBuilt) return;
        _msnBuilt = true;

        var (listPane, filter, add, del, list) = EditorHelpers.BuildListPane("Add new mission", "Delete selected mission");
        _msnFilter = filter; _msnList = list;
        filter.KeyUp += (_, _) => RefreshMsnList();
        list.SelectionChanged += (_, _) => { _selectedMsn = list.SelectedItem as MissionModel; RefreshMsnForm(); };
        add.Click += (_, _) => { var m = new MissionModel { MemberName = $"NewMission{_msns.Count + 1}", Title = "New Mission", Type = "Escort", CreditReward = 100, UpgradePointReward = 1 }; _msns.Add(m); RefreshMsnList(); list.SelectedItem = m; };
        del.Click += (_, _) => { if (_selectedMsn != null) { _msns.Remove(_selectedMsn); _selectedMsn = null; RefreshMsnList(); RefreshMsnForm(); } };

        _msnMember = EditorHelpers.NewTextBox();
        _msnId = EditorHelpers.NewTextBox();
        _msnTitle = EditorHelpers.NewTextBox();
        _msnBriefing = EditorHelpers.NewTextBox(multiline: true);
        _msnType = EditorHelpers.NewCombo(MissionTypes);
        _msnDest = EditorHelpers.NewCombo(Array.Empty<string>());
        _msnDest.HorizontalAlignment = HorizontalAlignment.Stretch;
        _msnEscort = EditorHelpers.NewTextBox();
        _msnHasItem = EditorHelpers.NewCheck("Has MissionItem (Delivery)");
        _msnItemName = EditorHelpers.NewTextBox();
        _msnItemDesc = EditorHelpers.NewTextBox(multiline: true);
        _msnItemDestId = EditorHelpers.NewCombo(Array.Empty<string>());
        _msnItemDestId.HorizontalAlignment = HorizontalAlignment.Stretch;
        _msnItemDestName = EditorHelpers.NewTextBox();
        _msnCheckSkill = EditorHelpers.NewCombo(AllSkills);
        _msnTN = EditorHelpers.NewNumeric(0, 99);
        _msnSuccess = EditorHelpers.NewTextBox(multiline: true);
        _msnFail = EditorHelpers.NewTextBox(multiline: true);
        _msnReward = EditorHelpers.NewNumeric(0, 99999);
        _msnUp = EditorHelpers.NewNumeric(0, 9);

        var form = new StackPanel { Margin = new Avalonia.Thickness(12), Spacing = 4 };
        form.Children.Add(new TextBlock { Text = "Mission", FontSize = 18, FontWeight = FontWeight.Bold, Foreground = new SolidColorBrush(Color.Parse("#F7FAFB")) });
        form.Children.Add(EditorHelpers.FormRow("Member name", _msnMember));
        form.Children.Add(EditorHelpers.FormRow("Id (snake_case)", _msnId));
        form.Children.Add(EditorHelpers.FormRow("Title", _msnTitle));
        form.Children.Add(EditorHelpers.FormRow("Briefing", _msnBriefing));
        form.Children.Add(EditorHelpers.FormRow("Type", _msnType));
        form.Children.Add(EditorHelpers.FormRow("Destination location id", _msnDest));
        form.Children.Add(EditorHelpers.FormRow("Escort NPC display name", _msnEscort));
        form.Children.Add(EditorHelpers.FormRow("",                 _msnHasItem));
        form.Children.Add(EditorHelpers.FormRow("  Item name", _msnItemName));
        form.Children.Add(EditorHelpers.FormRow("  Item description", _msnItemDesc));
        form.Children.Add(EditorHelpers.FormRow("  Item dest. location id", _msnItemDestId));
        form.Children.Add(EditorHelpers.FormRow("  Item dest. name", _msnItemDestName));
        form.Children.Add(EditorHelpers.FormRow("Sabotage/Recon skill", _msnCheckSkill));
        form.Children.Add(EditorHelpers.FormRow("Target number", _msnTN));
        form.Children.Add(EditorHelpers.FormRow("Check success text", _msnSuccess));
        form.Children.Add(EditorHelpers.FormRow("Check fail text", _msnFail));
        form.Children.Add(EditorHelpers.FormRow("Credit reward", _msnReward));
        form.Children.Add(EditorHelpers.FormRow("Upgrade points", _msnUp));

        _msnMember.TextChanged += (_, _) => SyncMsn(m => { m.MemberName = _msnMember!.Text ?? ""; RefreshMsnList(); });
        _msnId.TextChanged += (_, _) => SyncMsn(m => m.Id = _msnId!.Text ?? "");
        _msnTitle.TextChanged += (_, _) => SyncMsn(m => { m.Title = _msnTitle!.Text ?? ""; RefreshMsnList(); });
        _msnBriefing.TextChanged += (_, _) => SyncMsn(m => m.BriefingText = _msnBriefing!.Text ?? "");
        _msnType.SelectionChanged += (_, _) => SyncMsn(m => { m.Type = _msnType!.SelectedItem as string ?? "Escort"; RefreshMsnList(); });
        _msnDest.SelectionChanged += (_, _) => SyncMsn(m => m.DestinationLocationId = _msnDest!.SelectedItem as string ?? "");
        _msnEscort.TextChanged += (_, _) => SyncMsn(m => m.EscortNpcName = _msnEscort!.Text ?? "");
        _msnHasItem.IsCheckedChanged += (_, _) => SyncMsn(m => m.HasMissionItem = _msnHasItem!.IsChecked == true);
        _msnItemName.TextChanged += (_, _) => SyncMsn(m => m.MissionItemName = _msnItemName!.Text ?? "");
        _msnItemDesc.TextChanged += (_, _) => SyncMsn(m => m.MissionItemDescription = _msnItemDesc!.Text ?? "");
        // When the user picks an item destination id, auto-fill the destination
        // name from the matching room's display Name. Skip during programmatic
        // refreshes (handled by _msnSync) so we don't clobber a saved name.
        _msnItemDestId.SelectionChanged += (_, _) => SyncMsn(m =>
        {
            var id = _msnItemDestId!.SelectedItem as string ?? "";
            m.MissionItemDestinationLocationId = id;
            if (!string.IsNullOrEmpty(id))
            {
                var match = _msnLocations.FirstOrDefault(p => p.Id == id);
                if (!string.IsNullOrEmpty(match.Name))
                {
                    m.MissionItemDestinationName = match.Name;
                    _msnItemDestName!.Text = match.Name;
                }
            }
        });
        _msnItemDestName.TextChanged += (_, _) => SyncMsn(m => m.MissionItemDestinationName = _msnItemDestName!.Text ?? "");
        _msnCheckSkill.SelectionChanged += (_, _) => SyncMsn(m => m.CheckSkill = _msnCheckSkill!.SelectedItem as string ?? "");
        _msnTN.ValueChanged += (_, _) => SyncMsn(m => m.CheckTargetNumber = (int)(_msnTN!.Value ?? 0));
        _msnSuccess.TextChanged += (_, _) => SyncMsn(m => m.CheckSuccessText = _msnSuccess!.Text ?? "");
        _msnFail.TextChanged += (_, _) => SyncMsn(m => m.CheckFailText = _msnFail!.Text ?? "");
        _msnReward.ValueChanged += (_, _) => SyncMsn(m => m.CreditReward = (int)(_msnReward!.Value ?? 0));
        _msnUp.ValueChanged += (_, _) => SyncMsn(m => m.UpgradePointReward = (int)(_msnUp!.Value ?? 0));

        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("260,*") };
        Grid.SetColumn(listPane, 0); grid.Children.Add(listPane);
        var sv = new ScrollViewer { Content = form };
        Grid.SetColumn(sv, 1); grid.Children.Add(sv);
        MissionsView.Padding = default;
        MissionsView.Child = grid;
    }

    private void SyncMsn(Action<MissionModel> apply) { if (_msnSync || _selectedMsn == null) return; apply(_selectedMsn); }

    private void RefreshMsnList()
    {
        if (_msnList == null) return;
        var f = (_msnFilter?.Text ?? "").Trim().ToLowerInvariant();
        var visible = string.IsNullOrEmpty(f) ? _msns.ToList()
            : _msns.Where(m => m.MemberName.ToLowerInvariant().Contains(f) || m.Title.ToLowerInvariant().Contains(f) || m.Type.ToLowerInvariant().Contains(f)).ToList();
        _msnList.ItemsSource = visible;
        if (_selectedMsn != null && visible.Contains(_selectedMsn))
            _msnList.SelectedItem = _selectedMsn;
        else
            _selectedMsn = null;
    }

    private void RefreshMsnForm()
    {
        if (_selectedMsn == null) return;
        _msnSync = true;
        try
        {
            _msnMember!.Text = _selectedMsn.MemberName;
            _msnId!.Text = _selectedMsn.Id;
            _msnTitle!.Text = _selectedMsn.Title;
            _msnBriefing!.Text = _selectedMsn.BriefingText;
            _msnType!.SelectedItem = _selectedMsn.Type;
            _msnDest!.SelectedItem = string.IsNullOrEmpty(_selectedMsn.DestinationLocationId) ? null : _selectedMsn.DestinationLocationId;
            _msnEscort!.Text = _selectedMsn.EscortNpcName;
            _msnHasItem!.IsChecked = _selectedMsn.HasMissionItem;
            _msnItemName!.Text = _selectedMsn.MissionItemName;
            _msnItemDesc!.Text = _selectedMsn.MissionItemDescription;
            _msnItemDestId!.SelectedItem = string.IsNullOrEmpty(_selectedMsn.MissionItemDestinationLocationId) ? null : _selectedMsn.MissionItemDestinationLocationId;
            _msnItemDestName!.Text = _selectedMsn.MissionItemDestinationName;
            _msnCheckSkill!.SelectedItem = string.IsNullOrEmpty(_selectedMsn.CheckSkill) ? null : _selectedMsn.CheckSkill;
            _msnTN!.Value = _selectedMsn.CheckTargetNumber;
            _msnSuccess!.Text = _selectedMsn.CheckSuccessText;
            _msnFail!.Text = _selectedMsn.CheckFailText;
            _msnReward!.Value = _selectedMsn.CreditReward;
            _msnUp!.Value = _selectedMsn.UpgradePointReward;
        }
        finally { _msnSync = false; }
    }

    private void LoadMissions()
    {
        BuildMissionViewIfNeeded();
        var path = PathBox.Text ?? "";
        if (!File.Exists(path)) { Status($"file not found: {path}", error: true); return; }
        _msnParser = new MissionFileParser(path);
        if (!_msnParser.TryLoad()) { Status($"parse failed: {_msnParser.Error}", error: true); return; }
        _msns = _msnParser.Missions.ToList();
        _selectedMsn = null;
        if (_msnList != null) _msnList.SelectedItem = null;

        // Populate the destination dropdowns from the sibling LocationData.cs.
        // A blank first entry lets the user clear the selection.
        var contentDir = System.IO.Path.GetDirectoryName(path)!;
        var locPath = System.IO.Path.Combine(contentDir, "LocationData.cs");
        _msnLocations = new List<(string, string)>();
        if (File.Exists(locPath))
        {
            var lp = new LocationFileParser(locPath);
            if (lp.TryLoad())
                _msnLocations = lp.Rooms.Select(r => (r.Id, r.Name)).Where(p => !string.IsNullOrEmpty(p.Id)).OrderBy(p => p.Id).ToList();
        }
        var ids = new List<string> { "" };
        ids.AddRange(_msnLocations.Select(p => p.Id));
        if (_msnDest       != null) _msnDest.ItemsSource       = ids;
        if (_msnItemDestId != null) _msnItemDestId.ItemsSource = ids;

        RefreshMsnList();
        Status($"loaded {_msns.Count} missions");
    }

    private void SaveMissions()
    {
        if (_msnParser == null) { Status("nothing loaded", error: true); return; }
        try { MissionFileWriter.Save(_msnParser, _msns); Status($"saved {_msns.Count} missions"); LoadMissions(); }
        catch (Exception ex) { Status($"save failed: {ex.Message}", error: true); }
    }
}
