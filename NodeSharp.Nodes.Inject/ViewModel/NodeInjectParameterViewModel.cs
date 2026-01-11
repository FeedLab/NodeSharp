using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NodeSharp.Nodes.Inject.ViewModel;

public class NodeInjectParameterViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<DataGridItem> Items { get; set; }
    public List<string> SourceOptions { get; set; }
    public List<string> TypeOptions { get; set; }

    public NodeInjectParameterViewModel()
    {
        // Initialize source options (3 items)
        SourceOptions =
        [
            "Primitive",
            //"Environment",
            "Timestamp"
        ];

        // Initialize type options (5 items)
        TypeOptions =
        [
            "String",
            "Number",
            "Boolean"
        ];

        // Initialize with some sample data
        Items =
        [
            new DataGridItem { Name = "TimestampISO", Source = "Primitive", Type = "String", Value = "2026-01-11T16:24:00+07:00" },
            new DataGridItem { Name = "Age", Source = "Primitive", Type = "Number", Value = "30" },
            new DataGridItem { Name = "Timestamp", Source = "Timestamp",  Type = "Int64", Value = "639037204904322024" }
        ];
    }
}

public partial class DataGridItem : ObservableObject
{
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private string source = string.Empty;
    [ObservableProperty] private string type = string.Empty;
    [ObservableProperty] private string value = string.Empty;
    
    [ObservableProperty] private bool isTypeEditable = true;
    [ObservableProperty] private bool isValueVisible = true;

    partial void OnSourceChanged(string newValue)
    {
        IsValueVisible = false;
        IsTypeEditable = false;

        switch (newValue)
        {
            case "Primitive":
                IsValueVisible = true;
                IsTypeEditable = true;
                break;
            case "Timestamp":
                IsValueVisible = false;
                IsTypeEditable = false;
                Type = "Int64";
                break;
            case "Environment":
                IsValueVisible = true;
                IsTypeEditable = false;
                Type = "String";
                break;
            default:
                throw new Exception($"Invalid source: {newValue}");
        }

    }
}
