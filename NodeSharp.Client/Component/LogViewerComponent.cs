using System.Collections.ObjectModel;
using Syncfusion.Maui.ListView;

namespace NodeSharp.Client.Component;

public class LogViewer : ContentView
{
    public ObservableCollection<string> LogMessages { get; } = [];
    private readonly SfListView listView;

    public LogViewer()
    {
        listView = new SfListView
        {
            ItemsSource = LogMessages, ItemTemplate = new DataTemplate(() =>
            {
                var label = new Label { FontFamily = "Consolas", FontSize = 14, TextColor = Colors.White };
                label.SetBinding(Label.TextProperty, ".");
                return label;
            }),
            BackgroundColor = Colors.Black
        };
        Content = listView;
    }

    public void AddLog(string message)
    {
        LogMessages.Add(message);
        listView.ScrollTo(LogMessages.Count - 1, ScrollToPosition.End, true);
    }
}