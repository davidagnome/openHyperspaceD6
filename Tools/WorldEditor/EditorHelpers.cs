using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace TerminalHyperspace.WorldEditor;

/// Builders for the recurring "list pane" (filter + add/delete + listbox) and
/// helper widgets used across all seven entity editors.
public static class EditorHelpers
{
    public static (Border container, TextBox filter, Button add, Button delete, ListBox list)
        BuildListPane(string addTooltip, string deleteTooltip)
    {
        var filter = new TextBox { Watermark = "filter…" };
        var add    = new Button { Content = "+", Padding = new Avalonia.Thickness(8, 2), Margin = new Avalonia.Thickness(2, 0) };
        var delete = new Button { Content = "−", Padding = new Avalonia.Thickness(8, 2), Margin = new Avalonia.Thickness(2, 0) };
        ToolTip.SetTip(add, addTooltip);
        ToolTip.SetTip(delete, deleteTooltip);

        var topGrid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto,Auto"), Margin = new Avalonia.Thickness(4) };
        Grid.SetColumn(filter, 0); topGrid.Children.Add(filter);
        Grid.SetColumn(add, 1);    topGrid.Children.Add(add);
        Grid.SetColumn(delete, 2); topGrid.Children.Add(delete);

        var list = new ListBox { Background = Brushes.Transparent, BorderThickness = default };

        var dock = new DockPanel();
        DockPanel.SetDock(topGrid, Dock.Top);
        dock.Children.Add(topGrid);
        dock.Children.Add(list);

        var container = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#101820")),
            BorderBrush = new SolidColorBrush(Color.Parse("#1F2A36")),
            BorderThickness = new Avalonia.Thickness(0, 0, 1, 0),
            Child = dock,
        };
        return (container, filter, add, delete, list);
    }

    public static StackPanel FormRow(string label, Control field, double labelWidth = 160)
    {
        var sp = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Avalonia.Thickness(0, 2, 0, 2) };
        sp.Children.Add(new TextBlock
        {
            Text = label, Width = labelWidth, VerticalAlignment = VerticalAlignment.Center,
            Foreground = new SolidColorBrush(Color.Parse("#AAB6BF")),
        });
        sp.Children.Add(field);
        if (field is TextBox tb)
            tb.MinWidth = 320;
        return sp;
    }

    public static TextBox NewTextBox(double minWidth = 320, bool multiline = false)
        => new TextBox
        {
            MinWidth = minWidth,
            AcceptsReturn = multiline,
            TextWrapping = multiline ? TextWrapping.Wrap : TextWrapping.NoWrap,
            MinHeight = multiline ? 60 : 0,
        };

    public static NumericUpDown NewNumeric(decimal min = 0, decimal max = 9999, decimal increment = 1, string fmt = "0")
        => new NumericUpDown { Minimum = min, Maximum = max, Increment = increment, FormatString = fmt, MinWidth = 120 };

    public static ComboBox NewCombo(IEnumerable<string> items)
    {
        var cb = new ComboBox { MinWidth = 200 };
        cb.ItemsSource = items.ToList();
        return cb;
    }

    public static CheckBox NewCheck(string text)
        => new CheckBox { Content = text };
}
