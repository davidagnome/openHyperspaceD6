using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Path = System.IO.Path;
using Line = Avalonia.Controls.Shapes.Line;

namespace TerminalHyperspace.WorldEditor;

public partial class MainWindow : Window
{
    /// Tracks which menu item is active so OnLoadClick / OnSaveClick can route
    /// to the right parser/writer pair.
    private string _activeView = "Location";

    private LocationFileParser? _parser;
    private List<RoomModel> _rooms = new();
    private RoomModel? _selected;
    private List<string> _npcChoices = new();
    private List<string> _spaceChoices = new();
    private bool _suppressEditorSync;

    private static readonly string[] Climates = { "Normal", "Hot", "Cold", "Aquatic" };

    /// Canonical exit directions — mirrors the "─ Move ─" section of the
    /// in-game Command Panel so the editor stays consistent with what the
    /// CommandParser actually accepts.
    private static readonly string[] ExitDirections =
    {
        "north", "south", "east", "west",
        "northeast", "northwest", "southeast", "southwest",
        "up", "down",
        "dock", "land", "jump", "explore", "board", "leave", "airlock"
    };

    public MainWindow()
    {
        InitializeComponent();

        // Default the LocationData.cs path to the project's Content folder, walking
        // up from the running tool's bin directory.
        PathBox.Text = ResolveDefaultLocationPath();

        EdClimate.ItemsSource = Climates;
        EdClimate.SelectedIndex = 0;
        EdExitDir.ItemsSource = ExitDirections;

        // Wire change events on the inline editors so typing into a field syncs
        // straight back into the selected RoomModel.
        EdId.TextChanged              += (_, _) => Sync(r => r.Id              = EdId.Text ?? "");
        EdName.TextChanged            += (_, _) => Sync(r => r.Name            = EdName.Text ?? "");
        EdDescription.TextChanged     += (_, _) => Sync(r => r.Description     = EdDescription.Text ?? "");
        EdPlanet.TextChanged          += (_, _) => Sync(r => r.PlanetName      = EdPlanet.Text ?? "");
        EdStarSystem.TextChanged      += (_, _) => Sync(r => r.StarSystemName  = EdStarSystem.Text ?? "");
        EdSector.TextChanged          += (_, _) => Sync(r => r.SectorName      = EdSector.Text ?? "");
        EdTerritory.TextChanged       += (_, _) => Sync(r => r.TerritoryName   = EdTerritory.Text ?? "");
        EdClimate.SelectionChanged    += (_, _) => Sync(r => r.Climate         = EdClimate.SelectedItem as string ?? "Normal");
        EdHyperX.ValueChanged         += (_, _) => Sync(r => r.HyperspaceX     = (int)(EdHyperX.Value ?? 0));
        EdHyperY.ValueChanged         += (_, _) => Sync(r => r.HyperspaceY     = (int)(EdHyperY.Value ?? 0));
        EdEncounterChance.ValueChanged      += (_, _) => Sync(r => r.EncounterChance = (double)(EdEncounterChance.Value ?? 0));
        EdSpaceEncounterChance.ValueChanged += (_, _) => Sync(r => r.SpaceEncounterChance = (double)(EdSpaceEncounterChance.Value ?? 0));
        EdIsSpace.IsCheckedChanged         += (_, _) => Sync(r => r.IsSpace         = EdIsSpace.IsChecked == true);
        EdIsSystemSpace.IsCheckedChanged   += (_, _) => Sync(r => r.IsSystemSpace   = EdIsSystemSpace.IsChecked == true);
        EdRequiresVehicle.IsCheckedChanged += (_, _) => Sync(r => r.RequiresVehicle = EdRequiresVehicle.IsChecked == true);
        EdHasShop.IsCheckedChanged         += (_, _) => Sync(r => r.HasShop         = EdHasShop.IsChecked == true);
        EdHasVehicleShop.IsCheckedChanged  += (_, _) => Sync(r => r.HasVehicleShop  = EdHasVehicleShop.IsChecked == true);

        Opened += (_, _) =>
        {
            if (!string.IsNullOrEmpty(PathBox.Text) && File.Exists(PathBox.Text))
                Load();
        };
    }

    // ---------- MainPanel sidebar nav ----------

    private void OnMainPanelSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (MainPanel.SelectedItem is not ListBoxItem item) return;
        var key = item.Tag as string ?? "Location";
        _activeView = key;

        // Lazily build each non-Location editor on first show.
        switch (key)
        {
            case "Item":       BuildItemViewIfNeeded();       break;
            case "NPC":        BuildNpcViewIfNeeded();        break;
            case "SkillCheck": BuildSkillCheckViewIfNeeded(); break;
            case "Missions":   BuildMissionViewIfNeeded();    break;
            case "Vehicle":    BuildVehicleViewIfNeeded();    break;
            case "Role":       BuildRoleViewIfNeeded();       break;
            case "Species":    BuildSpeciesViewIfNeeded();    break;
        }

        // Hide every view, then show the selected one.
        LocationView.IsVisible   = key == "Location";
        NPCView.IsVisible        = key == "NPC";
        SkillCheckView.IsVisible = key == "SkillCheck";
        MissionsView.IsVisible   = key == "Missions";
        ItemView.IsVisible       = key == "Item";
        VehicleView.IsVisible    = key == "Vehicle";
        RoleView.IsVisible       = key == "Role";
        SpeciesView.IsVisible    = key == "Species";

        // Top bar adapts to whichever Content/*.cs is implied by the selection so
        // the Load/Save buttons make sense even before the other parsers exist.
        var (label, defaultName) = key switch
        {
            "NPC"        => ("NPCData.cs:",         "NPCData.cs"),
            "SkillCheck" => ("SkillCheckData.cs:",  "SkillCheckData.cs"),
            "Missions"   => ("MissionData.cs:",     "MissionData.cs"),
            "Item"       => ("ItemData.cs:",        "ItemData.cs"),
            "Vehicle"    => ("VehicleData.cs:",     "VehicleData.cs"),
            "Role"       => ("RoleData.cs:",        "RoleData.cs"),
            "Species"    => ("SpeciesData.cs:",     "SpeciesData.cs"),
            _            => ("LocationData.cs:",   "LocationData.cs"),
        };
        TopBarLabel.Text = label;
        if (!string.IsNullOrEmpty(PathBox.Text))
        {
            var dir = Path.GetDirectoryName(PathBox.Text);
            if (!string.IsNullOrEmpty(dir))
                PathBox.Text = Path.Combine(dir, defaultName);
        }

        // Drop the previous editor's parser reference so it's clearly "unloaded".
        UnloadAllParsers(except: key);

        // Defer the auto-load to the next UI tick. This is critical: we're
        // currently running inside MainPanel.SelectionChanged, and LoadXxx()
        // mutates inner ListBox.ItemsSource which fires nested SelectionChanged
        // events. Re-entering the event dispatch synchronously has caused
        // SIGABRT crashes after several menu switches (Avalonia/Skia trips on
        // the in-flight layout pass). Posting back to the dispatcher lets the
        // current handler unwind first.
        if (!string.IsNullOrEmpty(PathBox.Text) && File.Exists(PathBox.Text))
        {
            var capturedKey = key;
            Dispatcher.UIThread.Post(() =>
            {
                try
                {
                    switch (capturedKey)
                    {
                        case "Item":       LoadItems();       break;
                        case "NPC":        LoadNpcs();        break;
                        case "SkillCheck": LoadSkillChecks(); break;
                        case "Missions":   LoadMissions();    break;
                        case "Vehicle":    LoadVehicles();    break;
                        case "Role":       LoadRoles();       break;
                        case "Species":    LoadSpecies();     break;
                        default:           Load();            break; // Location
                    }
                }
                catch (Exception ex)
                {
                    // Surface failures in the status bar instead of letting
                    // them abort the process.
                    Status($"auto-load failed: {ex.Message}", error: true);
                }
            });
        }
    }

    /// Drop parser/state references for every editor except the one we're
    /// switching to. Keeps memory usage low and prevents stale spans from
    /// being saved against a different file.
    private void UnloadAllParsers(string except)
    {
        if (except != "Location")   { _parser = null; _rooms.Clear(); _selected = null; }
        if (except != "Item")       { _itemParser = null; _items.Clear(); _selectedItem = null; }
        if (except != "NPC")        { _npcParser = null; _npcs.Clear(); _selectedNpc = null; }
        if (except != "SkillCheck") { _scParser = null; _scs.Clear(); _selectedSc = null; }
        if (except != "Missions")   { _msnParser = null; _msns.Clear(); _selectedMsn = null; }
        if (except != "Vehicle")    { _vhParser = null; _vhs.Clear(); _selectedVh = null; }
        if (except != "Role")       { _roleParser = null; _roles.Clear(); _selectedRole = null; }
        if (except != "Species")    { _speciesParser = null; _species.Clear(); _selectedSpecies = null; }
    }

    // ---------- Loading / saving ----------

    private static string ResolveDefaultLocationPath()
    {
        var dir = Directory.GetCurrentDirectory();
        for (int i = 0; i < 8 && dir != null; i++)
        {
            var probe = Path.Combine(dir, "Content", "LocationData.cs");
            if (File.Exists(probe)) return probe;
            dir = Path.GetDirectoryName(dir);
        }
        return Path.Combine(Directory.GetCurrentDirectory(), "Content", "LocationData.cs");
    }

    private void OnLoadClick(object? sender, RoutedEventArgs e)
    {
        switch (_activeView)
        {
            case "Item":       LoadItems();       break;
            case "NPC":        LoadNpcs();        break;
            case "SkillCheck": LoadSkillChecks(); break;
            case "Missions":   LoadMissions();    break;
            case "Vehicle":    LoadVehicles();    break;
            case "Role":       LoadRoles();       break;
            case "Species":    LoadSpecies();     break;
            default:           Load();            break; // Location
        }
    }

    private void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        switch (_activeView)
        {
            case "Item":       SaveItems();       break;
            case "NPC":        SaveNpcs();        break;
            case "SkillCheck": SaveSkillChecks(); break;
            case "Missions":   SaveMissions();    break;
            case "Vehicle":    SaveVehicles();    break;
            case "Role":       SaveRoles();       break;
            case "Species":    SaveSpecies();     break;
            default:           Save();            break; // Location
        }
    }

    private void Load()
    {
        var path = PathBox.Text ?? "";
        if (!File.Exists(path)) { Status($"file not found: {path}", error: true); return; }

        _parser = new LocationFileParser(path);
        if (!_parser.TryLoad())
        {
            Status($"parse failed: {_parser.Error}", error: true);
            return;
        }

        _rooms = _parser.Rooms.ToList();

        // Discover NPC + SpaceEncounter factory names by scanning sibling files.
        var contentDir = Path.GetDirectoryName(path)!;
        _npcChoices   = SymbolScanner.ScanFactories(Path.Combine(contentDir, "NPCData.cs"), "Character");
        _spaceChoices = SymbolScanner.ScanFactories(Path.Combine(contentDir, "SpaceEncounterData.cs"), "SpaceEncounter");
        EdNpcPicker.ItemsSource      = _npcChoices;
        EdFriendlyPicker.ItemsSource = _npcChoices;
        EdSpacePicker.ItemsSource    = _spaceChoices;

        RefreshRoomList();
        DrawWorldMap();
        Status($"loaded {_rooms.Count} rooms");
    }

    private void Save()
    {
        if (_parser == null) { Status("nothing loaded", error: true); return; }
        try
        {
            LocationFileWriter.Save(_parser, _rooms);
            Status($"saved {_rooms.Count} rooms → {Path.GetFileName(_parser.FilePath)}");
            // Reload so OriginalSpan values are accurate after the rewrite.
            Load();
        }
        catch (Exception ex)
        {
            Status($"save failed: {ex.Message}", error: true);
        }
    }

    private void Status(string text, bool error = false)
    {
        StatusText.Text = text;
        StatusText.Foreground = new SolidColorBrush(error ? Color.Parse("#FF6B6B") : Color.Parse("#9CCFA0"));
    }

    // ---------- Room list ----------

    private void RefreshRoomList()
    {
        var filter = (FilterBox.Text ?? "").Trim().ToLowerInvariant();
        var visible = string.IsNullOrEmpty(filter)
            ? _rooms
            : _rooms.Where(r => r.Id.ToLowerInvariant().Contains(filter)
                             || r.Name.ToLowerInvariant().Contains(filter)
                             || r.PlanetName.ToLowerInvariant().Contains(filter)).ToList();

        var prevSelected = _selected;
        RoomList.ItemsSource = visible.Select(r => new RoomListItem(r)).ToList();
        if (prevSelected != null)
        {
            for (int i = 0; i < RoomList.Items.Count; i++)
                if (((RoomListItem)RoomList.Items[i]!).Room == prevSelected)
                {
                    RoomList.SelectedIndex = i;
                    break;
                }
        }
    }

    private void OnFilterChanged(object? sender, KeyEventArgs e) => RefreshRoomList();

    private void OnRoomSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (RoomList.SelectedItem is RoomListItem rli)
            ShowRoom(rli.Room);
        else
            ShowRoom(null);
    }

    private void OnAddRoomClick(object? sender, RoutedEventArgs e)
    {
        var room = new RoomModel
        {
            Id = $"new_room_{_rooms.Count + 1}",
            Name = "New Room",
            Climate = "Normal",
            EncounterChance = 0.2,
        };
        _rooms.Add(room);
        RefreshRoomList();
        for (int i = 0; i < RoomList.Items.Count; i++)
            if (((RoomListItem)RoomList.Items[i]!).Room == room) { RoomList.SelectedIndex = i; break; }
        DrawWorldMap();
        Status($"added {room.Id} (Save to write to disk)");
    }

    private void OnDeleteRoomClick(object? sender, RoutedEventArgs e)
    {
        if (_selected == null) return;
        _rooms.Remove(_selected);
        _selected = null;
        RefreshRoomList();
        ShowRoom(null);
        DrawWorldMap();
        Status("removed (Save to write to disk)");
    }

    // ---------- Editor sync ----------

    private void Sync(Action<RoomModel> apply)
    {
        if (_suppressEditorSync || _selected == null) return;
        apply(_selected);
        // Refresh the list display so renamed rooms show their new label/id.
        RefreshRoomList();
    }

    private void ShowRoom(RoomModel? room)
    {
        _selected = room;
        if (room == null)
        {
            EditorGrid.IsVisible = false;
            NoSelectionText.IsVisible = true;
            return;
        }
        EditorGrid.IsVisible = true;
        NoSelectionText.IsVisible = false;

        _suppressEditorSync = true;
        try
        {
            EdId.Text              = room.Id;
            EdName.Text            = room.Name;
            EdDescription.Text     = room.Description;
            EdPlanet.Text          = room.PlanetName;
            EdStarSystem.Text      = room.StarSystemName;
            EdSector.Text          = room.SectorName;
            EdTerritory.Text       = room.TerritoryName;
            EdClimate.SelectedItem = string.IsNullOrEmpty(room.Climate) ? "Normal" : room.Climate;
            EdHyperX.Value         = room.HyperspaceX;
            EdHyperY.Value         = room.HyperspaceY;
            EdEncounterChance.Value      = (decimal)room.EncounterChance;
            EdSpaceEncounterChance.Value = (decimal)room.SpaceEncounterChance;
            EdIsSpace.IsChecked         = room.IsSpace;
            EdIsSystemSpace.IsChecked   = room.IsSystemSpace;
            EdRequiresVehicle.IsChecked = room.RequiresVehicle;
            EdHasShop.IsChecked         = room.HasShop;
            EdHasVehicleShop.IsChecked  = room.HasVehicleShop;
            EdFriendlyNPCsPresent.IsChecked = room.FriendlyNPCsPresent;
        }
        finally { _suppressEditorSync = false; }

        RefreshExitsList();
        RefreshAmbientList();
        RefreshEncounterList(EdPossibleEncounters, room.PossibleEncounters);
        RefreshEncounterList(EdFriendlyNPCs,       room.FriendlyNPCs);
        RefreshEncounterList(EdSpaceEncounters,    room.SpaceEncounters);

        EdExitDest.ItemsSource = _rooms.Select(r => r.Id).OrderBy(s => s).ToList();
    }

    // ---------- Sub-list refreshes ----------

    private void RefreshExitsList()
    {
        if (_selected == null) return;
        EdExits.ItemsSource = _selected.Exits.Select((e, i) => new ExitItem(e, i, this)).ToList();
    }

    private void RefreshAmbientList()
    {
        if (_selected == null) return;
        EdAmbient.Children.Clear();
        for (int i = 0; i < _selected.AmbientMessages.Count; i++)
        {
            var idx = i;
            var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto") };
            var tb = new TextBox
            {
                Text = _selected.AmbientMessages[idx],
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
            };
            // Sync as the user types so saves capture mid-edit changes too.
            tb.TextChanged += (_, _) =>
            {
                if (_selected == null) return;
                if (idx < _selected.AmbientMessages.Count)
                    _selected.AmbientMessages[idx] = tb.Text ?? "";
            };
            Grid.SetColumn(tb, 0);

            var del = new Button
            {
                Content = "✕",
                Margin = new Avalonia.Thickness(4, 0, 0, 0),
                Padding = new Avalonia.Thickness(8, 2),
            };
            ToolTip.SetTip(del, "Delete this ambient message");
            del.Click += (_, _) => RemoveAmbient(idx);
            Grid.SetColumn(del, 1);

            grid.Children.Add(tb);
            grid.Children.Add(del);
            EdAmbient.Children.Add(grid);
        }
    }

    private void RefreshEncounterList(ItemsControl ctrl, List<string> source)
    {
        if (_selected == null) return;
        ctrl.ItemsSource = source.Select((s, i) => new EncounterItem(s, i, source, this)).ToList();
    }

    // ---------- Add-buttons ----------

    private void OnAddPossibleEncounterClick(object? sender, RoutedEventArgs e)
    {
        if (_selected == null || EdNpcPicker.SelectedItem is not string s) return;
        _selected.PossibleEncounters.Add(s);
        RefreshEncounterList(EdPossibleEncounters, _selected.PossibleEncounters);
    }

    private void OnAddFriendlyNpcClick(object? sender, RoutedEventArgs e)
    {
        if (_selected == null || EdFriendlyPicker.SelectedItem is not string s) return;
        _selected.FriendlyNPCs.Add(s);
        _selected.FriendlyNPCsPresent = true;
        EdFriendlyNPCsPresent.IsChecked = true;
        RefreshEncounterList(EdFriendlyNPCs, _selected.FriendlyNPCs);
    }

    private void OnAddSpaceEncounterClick(object? sender, RoutedEventArgs e)
    {
        if (_selected == null || EdSpacePicker.SelectedItem is not string s) return;
        _selected.SpaceEncounters.Add(s);
        RefreshEncounterList(EdSpaceEncounters, _selected.SpaceEncounters);
    }

    private void OnAddExitClick(object? sender, RoutedEventArgs e)
    {
        if (_selected == null) return;
        if (EdExitDir.SelectedItem is not string dir || string.IsNullOrEmpty(dir)) return;
        if (EdExitDest.SelectedItem is not string dest) return;
        _selected.Exits.Add(new KeyValueEntry(dir, dest));
        EdExitDir.SelectedItem = null;
        RefreshExitsList();
        DrawWorldMap();
    }

    private void OnAddAmbientClick(object? sender, RoutedEventArgs e)
    {
        if (_selected == null) return;
        var t = (EdAmbientText.Text ?? "").Trim();
        if (string.IsNullOrEmpty(t)) return;
        _selected.AmbientMessages.Add(t);
        EdAmbientText.Text = "";
        RefreshAmbientList();
    }

    private void OnFriendlyToggle(object? sender, RoutedEventArgs e)
    {
        if (_selected == null) return;
        _selected.FriendlyNPCsPresent = EdFriendlyNPCsPresent.IsChecked == true;
        if (!_selected.FriendlyNPCsPresent) _selected.FriendlyNPCs.Clear();
        RefreshEncounterList(EdFriendlyNPCs, _selected.FriendlyNPCs);
    }

    public void RemoveExit(int idx)
    {
        if (_selected == null) return;
        _selected.Exits.RemoveAt(idx);
        RefreshExitsList();
        DrawWorldMap();
    }

    public void RemoveAmbient(int idx)
    {
        if (_selected == null) return;
        _selected.AmbientMessages.RemoveAt(idx);
        RefreshAmbientList();
    }

    public void RemoveEncounter(List<string> source, int idx, ItemsControl ctrl)
    {
        if (_selected == null) return;
        source.RemoveAt(idx);
        RefreshEncounterList(ctrl, source);
    }

    // ---------- World map canvas ----------

    private void OnRefreshMapClick(object? sender, RoutedEventArgs e) => DrawWorldMap();

    private static readonly Dictionary<string, (int dx, int dy)> Compass = new()
    {
        ["north"]     = (0, -1), ["south"]     = (0,  1),
        ["east"]      = (1,  0), ["west"]      = (-1, 0),
        ["northeast"] = (1, -1), ["northwest"] = (-1, -1),
        ["southeast"] = (1,  1), ["southwest"] = (-1, 1),
        ["ne"] = (1, -1), ["nw"] = (-1, -1), ["se"] = (1, 1), ["sw"] = (-1, 1),
    };

    private void DrawWorldMap()
    {
        WorldCanvas.Children.Clear();
        if (_rooms.Count == 0) return;

        // Layout each disconnected component independently via BFS, stacking them
        // vertically so the entire world is visible at once.
        var byId = _rooms.GroupBy(r => r.Id).ToDictionary(g => g.Key, g => g.First());
        var assigned = new Dictionary<string, (int x, int y)>();
        var components = new List<List<(string Id, int x, int y)>>();

        foreach (var seed in _rooms)
        {
            if (assigned.ContainsKey(seed.Id)) continue;
            var component = new List<(string, int, int)>();
            var queue = new Queue<string>();
            queue.Enqueue(seed.Id);
            assigned[seed.Id] = (0, 0);
            component.Add((seed.Id, 0, 0));
            while (queue.Count > 0)
            {
                var cur = queue.Dequeue();
                if (!byId.TryGetValue(cur, out var room)) continue;
                var (cx, cy) = assigned[cur];
                foreach (var ex in room.Exits)
                {
                    if (!Compass.TryGetValue(ex.Key.ToLowerInvariant(), out var d)) continue;
                    if (!byId.ContainsKey(ex.Value)) continue;
                    if (assigned.ContainsKey(ex.Value)) continue;
                    var pos = (cx + d.dx, cy + d.dy);
                    assigned[ex.Value] = pos;
                    component.Add((ex.Value, pos.Item1, pos.Item2));
                    queue.Enqueue(ex.Value);
                }
            }
            components.Add(component);
        }

        const int CellW = 130;
        const int CellH = 70;
        const int RoomW = 110;
        const int RoomH = 40;
        const int Gap   = 80;

        double yOffset = 30;
        var roomCenters = new Dictionary<string, (double x, double y)>();

        var visitedBrush = new SolidColorBrush(Color.Parse("#1A2B3F"));
        var spaceBrush   = new SolidColorBrush(Color.Parse("#2A1A3F"));
        var selectBrush  = new SolidColorBrush(Color.Parse("#3F2A1A"));
        var strokeNormal = new SolidColorBrush(Color.Parse("#5599FF"));
        var strokeSpace  = new SolidColorBrush(Color.Parse("#BB99FF"));
        var strokeSelect = new SolidColorBrush(Color.Parse("#FFCC55"));
        var edgeBrush    = new SolidColorBrush(Color.Parse("#3A6A8A"));
        var edgeUnknown  = new SolidColorBrush(Color.Parse("#553333"));
        var labelBrush   = new SolidColorBrush(Color.Parse("#EEEEEE"));

        foreach (var comp in components)
        {
            int minX = comp.Min(c => c.x), maxX = comp.Max(c => c.x);
            int minY = comp.Min(c => c.y), maxY = comp.Max(c => c.y);
            double xBase = 30 - minX * CellW;
            foreach (var (id, gx, gy) in comp)
            {
                var cx = xBase + gx * CellW + CellW / 2.0;
                var cy = yOffset + (gy - minY) * CellH + CellH / 2.0;
                roomCenters[id] = (cx, cy);
            }
            yOffset += (maxY - minY + 1) * CellH + Gap;
        }

        // Draw edges first so they're under the room boxes.
        foreach (var room in _rooms)
        {
            if (!roomCenters.TryGetValue(room.Id, out var src)) continue;
            foreach (var ex in room.Exits)
            {
                if (!Compass.ContainsKey(ex.Key.ToLowerInvariant())) continue;
                if (!roomCenters.TryGetValue(ex.Value, out var dst)) continue;
                WorldCanvas.Children.Add(new Line
                {
                    StartPoint = new Point(src.x, src.y),
                    EndPoint = new Point(dst.x, dst.y),
                    Stroke = byId.ContainsKey(ex.Value) ? edgeBrush : edgeUnknown,
                    StrokeThickness = 1.5,
                });
            }
        }

        foreach (var (id, (cx, cy)) in roomCenters)
        {
            var room = byId[id];
            var fill = room.IsSpace ? spaceBrush : visitedBrush;
            var stroke = room.IsSpace ? strokeSpace : strokeNormal;
            if (room == _selected) { fill = selectBrush; stroke = strokeSelect; }

            var rect = new Rectangle
            {
                Width = RoomW, Height = RoomH,
                Fill = fill, Stroke = stroke,
                StrokeThickness = room == _selected ? 2.5 : 1.2,
                RadiusX = 4, RadiusY = 4,
                Tag = id,
            };
            rect.PointerPressed += OnRoomBoxClicked;
            Canvas.SetLeft(rect, cx - RoomW / 2.0);
            Canvas.SetTop(rect, cy - RoomH / 2.0);
            WorldCanvas.Children.Add(rect);

            var label = new TextBlock
            {
                Text = string.IsNullOrEmpty(room.Name) ? room.Id : room.Name,
                Foreground = labelBrush,
                FontSize = 10,
                Width = RoomW - 6, Height = RoomH - 4,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                IsHitTestVisible = false,
            };
            Canvas.SetLeft(label, cx - (RoomW - 6) / 2.0);
            Canvas.SetTop(label, cy - (RoomH - 4) / 2.0);
            WorldCanvas.Children.Add(label);
        }

        WorldCanvas.Width  = Math.Max(1600, roomCenters.Values.DefaultIfEmpty((0, 0)).Max(p => p.x) + 100);
        WorldCanvas.Height = Math.Max(1200, yOffset + 40);
    }

    private void OnRoomBoxClicked(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Rectangle r && r.Tag is string id)
        {
            for (int i = 0; i < RoomList.Items.Count; i++)
                if (((RoomListItem)RoomList.Items[i]!).Room.Id == id) { RoomList.SelectedIndex = i; break; }
        }
    }
}

// ---------- ListBox / ItemsControl item wrappers ----------

public class RoomListItem
{
    public RoomModel Room { get; }
    public RoomListItem(RoomModel r) { Room = r; }
    public override string ToString()
    {
        var prefix = Room.IsNew ? "+ " : "  ";
        var label = string.IsNullOrEmpty(Room.Name) ? Room.Id : Room.Name;
        return $"{prefix}{label}";
    }
}

public class ExitItem
{
    public string Display { get; }
    public ExitItem(KeyValueEntry e, int idx, MainWindow owner)
    {
        Display = $"{e.Key} → {e.Value}";
        Index = idx;
        Owner = owner;
    }
    public int Index { get; }
    public MainWindow Owner { get; }
    public override string ToString() => Display;
}

public class AmbientItem
{
    public string Display { get; }
    public AmbientItem(string s, int idx, MainWindow owner)
    {
        Display = s.Length > 80 ? s.Substring(0, 80) + "…" : s;
        Index = idx;
        Owner = owner;
    }
    public int Index { get; }
    public MainWindow Owner { get; }
    public override string ToString() => Display;
}

public class EncounterItem
{
    public string Name { get; }
    public EncounterItem(string s, int idx, List<string> source, MainWindow owner)
    {
        Name = s; Index = idx; Source = source; Owner = owner;
    }
    public int Index { get; }
    public List<string> Source { get; }
    public MainWindow Owner { get; }
    public override string ToString() => Name;
}
