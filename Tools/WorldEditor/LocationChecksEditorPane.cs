using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Path = System.IO.Path;

namespace TerminalHyperspace.WorldEditor;

/// LocationChecks editor: filter pane lists every Location id discovered from
/// LocationData.cs (whether or not it currently has a checks entry). Selecting
/// one shows its CheckMemberNames list with per-row delete buttons plus a
/// dropdown of every SkillCheckEvent member from SkillCheckData.cs to add new
/// checks. Empty entries are pruned on save.
public partial class MainWindow
{
    private LocationChecksFileParser? _lcParser;
    private List<LocationCheckEntryModel> _lcEntries = new();
    private LocationCheckEntryModel? _selectedLc;
    /// All Location ids discovered in LocationData.cs — used for the filter
    /// pane list. Falls back to dictionary keys if LocationData.cs can't be
    /// scanned.
    private List<string> _lcLocationIds = new();
    /// All SkillCheckEvent member names — populates the "Add" dropdown so
    /// only existing checks can be added.
    private List<string> _lcAvailableChecks = new();

    private ListBox? _lcList;
    private TextBox? _lcFilter;
    private TextBlock? _lcHeader;
    private StackPanel? _lcMemberRows;
    private ComboBox? _lcAddCombo;
    private Button? _lcAddBtn;
    private TextBlock? _lcEmptyHint;
    private bool _lcBuilt;

    private void BuildLocationChecksViewIfNeeded()
    {
        if (_lcBuilt) return;
        _lcBuilt = true;

        var (listPane, filter, add, del, list) =
            EditorHelpers.BuildListPane("(disabled — locations come from LocationData.cs)",
                                        "(disabled — locations come from LocationData.cs)");
        // Add/Delete operate on the per-location members, not on Locations
        // themselves — those are owned by LocationData.cs.
        add.IsEnabled = false;
        del.IsEnabled = false;
        _lcFilter = filter;
        _lcList = list;
        filter.KeyUp += (_, _) => RefreshLcList();
        list.SelectionChanged += (_, _) =>
        {
            if (list.SelectedItem is LocationCheckListItem li)
            {
                // Locations without an entry yet still get a selectable target —
                // a transient empty model that's only promoted into _lcEntries
                // when the user adds the first check (see OnAddLcMemberClick).
                _selectedLc = FindOrNullEntry(li.LocationId)
                              ?? new LocationCheckEntryModel { LocationId = li.LocationId };
            }
            else _selectedLc = null;
            RefreshLcEditor();
        };

        _lcHeader = new TextBlock
        {
            FontSize = 18, FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#F7FAFB")),
            Text = "Select a Location",
        };
        _lcEmptyHint = new TextBlock
        {
            Foreground = new SolidColorBrush(Color.Parse("#888888")),
            FontStyle = FontStyle.Italic,
            Text = "No SkillCheckEvents bound to this location yet — pick one below and click Add.",
            TextWrapping = TextWrapping.Wrap,
        };
        _lcMemberRows = new StackPanel { Spacing = 4 };
        _lcAddCombo = new ComboBox { MinWidth = 280, PlaceholderText = "pick a SkillCheckEvent…" };
        _lcAddBtn = new Button { Content = "Add", Margin = new Avalonia.Thickness(8, 0, 0, 0) };
        _lcAddBtn.Click += (_, _) => OnAddLcMemberClick();

        var addRow = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto") };
        Grid.SetColumn(_lcAddCombo, 0); addRow.Children.Add(_lcAddCombo);
        Grid.SetColumn(_lcAddBtn, 1);   addRow.Children.Add(_lcAddBtn);

        var form = new StackPanel { Margin = new Avalonia.Thickness(16), Spacing = 10 };
        form.Children.Add(_lcHeader);
        form.Children.Add(new TextBlock
        {
            Text = "Bound SkillCheckEvents",
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#AAB6BF")),
            Margin = new Avalonia.Thickness(0, 8, 0, 0),
        });
        form.Children.Add(_lcMemberRows);
        form.Children.Add(_lcEmptyHint);
        form.Children.Add(new TextBlock
        {
            Text = "Add",
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#AAB6BF")),
            Margin = new Avalonia.Thickness(0, 8, 0, 0),
        });
        form.Children.Add(addRow);

        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("260,*") };
        Grid.SetColumn(listPane, 0); grid.Children.Add(listPane);
        var sv = new ScrollViewer { Content = form };
        Grid.SetColumn(sv, 1); grid.Children.Add(sv);
        LocationChecksView.Padding = default;
        LocationChecksView.Child = grid;
    }

    // ---------- Filter list ----------

    private void RefreshLcList()
    {
        if (_lcList == null) return;
        var f = (_lcFilter?.Text ?? "").Trim().ToLowerInvariant();
        var visible = _lcLocationIds
            .Where(id => string.IsNullOrEmpty(f) || id.ToLowerInvariant().Contains(f))
            .Select(id => new LocationCheckListItem(id, FindOrNullEntry(id)?.CheckMemberNames.Count ?? 0))
            .ToList();
        _lcList.ItemsSource = visible;
        if (_selectedLc != null)
        {
            for (int i = 0; i < visible.Count; i++)
                if (visible[i].LocationId == _selectedLc.LocationId)
                {
                    _lcList.SelectedIndex = i;
                    break;
                }
        }
    }

    private LocationCheckEntryModel? FindOrNullEntry(string locationId) =>
        _lcEntries.FirstOrDefault(e => e.LocationId == locationId);

    /// Returns the entry for the given location id, creating a fresh empty one
    /// (and adding it to _lcEntries) if it doesn't exist yet.
    private LocationCheckEntryModel EnsureEntry(string locationId)
    {
        var existing = FindOrNullEntry(locationId);
        if (existing != null) return existing;
        var fresh = new LocationCheckEntryModel { LocationId = locationId };
        _lcEntries.Add(fresh);
        return fresh;
    }

    // ---------- Editor pane ----------

    private void RefreshLcEditor()
    {
        if (_lcMemberRows == null || _lcHeader == null || _lcAddCombo == null || _lcEmptyHint == null) return;
        _lcMemberRows.Children.Clear();
        if (_selectedLc == null)
        {
            _lcHeader.Text = "Select a Location";
            _lcEmptyHint.IsVisible = false;
            _lcAddCombo.IsEnabled = false;
            if (_lcAddBtn != null) _lcAddBtn.IsEnabled = false;
            return;
        }

        _lcHeader.Text = _selectedLc.LocationId;
        _lcAddCombo.IsEnabled = true;
        if (_lcAddBtn != null) _lcAddBtn.IsEnabled = true;
        _lcEmptyHint.IsVisible = _selectedLc.CheckMemberNames.Count == 0;

        for (int i = 0; i < _selectedLc.CheckMemberNames.Count; i++)
        {
            var idx = i;
            var name = _selectedLc.CheckMemberNames[idx];

            var row = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto") };
            var label = new TextBlock
            {
                Text = name,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(
                    _lcAvailableChecks.Contains(name)
                        ? Color.Parse("#D8E3EC")
                        : Color.Parse("#FF8B6B")),  // orange-red if the member no longer exists
                FontFamily = new FontFamily("Menlo, Consolas, monospace"),
            };
            ToolTip.SetTip(label,
                _lcAvailableChecks.Contains(name)
                    ? name
                    : $"{name} — not found in SkillCheckData.cs");
            Grid.SetColumn(label, 0);

            var del = new Button
            {
                Content = "✕",
                Padding = new Avalonia.Thickness(8, 2),
                Margin = new Avalonia.Thickness(8, 0, 0, 0),
            };
            ToolTip.SetTip(del, $"Remove {name}");
            del.Click += (_, _) => RemoveLcMember(idx);
            Grid.SetColumn(del, 1);

            row.Children.Add(label);
            row.Children.Add(del);
            _lcMemberRows.Children.Add(row);
        }
    }

    private void OnAddLcMemberClick()
    {
        if (_selectedLc == null || _lcAddCombo?.SelectedItem is not string memberName) return;
        if (string.IsNullOrWhiteSpace(memberName)) return;
        if (_selectedLc.CheckMemberNames.Contains(memberName))
        {
            Status($"{memberName} already bound to {_selectedLc.LocationId}");
            return;
        }
        // Promote a transient (selection-only) entry into the real list the
        // moment it gains its first check.
        if (!_lcEntries.Contains(_selectedLc))
            _lcEntries.Add(_selectedLc);
        _selectedLc.CheckMemberNames.Add(memberName);
        RefreshLcEditor();
        RefreshLcList();
        Status($"added {memberName} to {_selectedLc.LocationId} (Save to write to disk)");
    }

    private void RemoveLcMember(int idx)
    {
        if (_selectedLc == null) return;
        if (idx < 0 || idx >= _selectedLc.CheckMemberNames.Count) return;
        var removed = _selectedLc.CheckMemberNames[idx];
        _selectedLc.CheckMemberNames.RemoveAt(idx);
        RefreshLcEditor();
        RefreshLcList();
        Status($"removed {removed} from {_selectedLc.LocationId} (Save to write to disk)");
    }

    // ---------- Load / Save ----------

    private void LoadLocationChecks()
    {
        BuildLocationChecksViewIfNeeded();
        var path = PathBox.Text ?? "";
        if (!File.Exists(path)) { Status($"file not found: {path}", error: true); return; }
        _lcParser = new LocationChecksFileParser(path);
        if (!_lcParser.TryLoad()) { Status($"parse failed: {_lcParser.Error}", error: true); return; }
        _lcEntries = _lcParser.Entries.ToList();

        // Discover available SkillCheckEvent member names by parsing the same
        // file with the existing SkillCheck parser — those names populate the
        // "Add" dropdown so users can only bind checks that actually exist.
        var scParser = new SkillCheckFileParser(path);
        _lcAvailableChecks = scParser.TryLoad()
            ? scParser.Checks.Select(c => c.MemberName).OrderBy(s => s).ToList()
            : new List<string>();
        if (_lcAddCombo != null) _lcAddCombo.ItemsSource = _lcAvailableChecks;

        // Discover Location ids from the sibling LocationData.cs so the filter
        // pane always shows every Location, even ones with no checks bound.
        var contentDir = Path.GetDirectoryName(path)!;
        var locPath = Path.Combine(contentDir, "LocationData.cs");
        var fromLocations = new List<string>();
        if (File.Exists(locPath))
        {
            var locParser = new LocationFileParser(locPath);
            if (locParser.TryLoad())
                fromLocations = locParser.Rooms.Select(r => r.Id).ToList();
        }
        // Union with dictionary keys so legacy/unknown ids still appear.
        _lcLocationIds = fromLocations
            .Union(_lcEntries.Select(e => e.LocationId))
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .OrderBy(s => s)
            .ToList();

        _selectedLc = null;
        if (_lcList != null) _lcList.SelectedItem = null;
        RefreshLcList();
        RefreshLcEditor();
        Status($"loaded {_lcEntries.Count} location-check entries ({_lcAvailableChecks.Count} checks available, {_lcLocationIds.Count} locations)");
    }

    private void SaveLocationChecks()
    {
        if (_lcParser == null) { Status("nothing loaded", error: true); return; }
        try
        {
            LocationChecksFileWriter.Save(_lcParser, _lcEntries);
            var written = _lcEntries.Count(e => e.CheckMemberNames.Count > 0);
            Status($"saved {written} location-check entries → {Path.GetFileName(_lcParser.FilePath)}");
            LoadLocationChecks();
        }
        catch (Exception ex) { Status($"save failed: {ex.Message}", error: true); }
    }
}

/// Display wrapper for the filter pane. Shows `id (count)` so users can see
/// at a glance which Locations have any LocationChecks bound.
public class LocationCheckListItem
{
    public string LocationId { get; }
    public int Count { get; }
    public LocationCheckListItem(string id, int count) { LocationId = id; Count = count; }
    public override string ToString() => Count > 0 ? $"{LocationId}  ({Count})" : LocationId;
}
