using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NodeSharp.Nodes.Common.Extension;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Maui.Core;

namespace NodeSharp.Nodes.Inject.ViewModel;

public partial class NodeInjectParameterViewModel : ObservableObject
{
    private readonly IPopupService popupService;

    [ObservableProperty] private ParameterItem? selectedItem;

    [RelayCommand]
    private async Task RowDoubleClick(ParameterItem item)
    {
        var index = Items.IndexOf(item);
        var isPopupOk = await DisplayParameterEditPopup(item);
        
    if (isPopupOk && index >= 0 && index < Items.Count)
        {
            
            Items.RemoveAt(index);
            Items.Insert(index, item);
        }
    }

    public ObservableCollection<ParameterItem> Items { get; set; } = [];
    public List<string> SourceOptions { get; set; }
    public List<string> TypeOptions { get; set; }

    public NodeInjectParameterViewModel(IPopupService popupService)
    {
        this.popupService = popupService;
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


        Items.CollectionChanged += OnItemsCollectionChanged;
    }

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RemoveCommand.NotifyCanExecuteChanged();
        AddCommand.NotifyCanExecuteChanged();
    }

    partial void OnSelectedItemChanged(ParameterItem? value)
    {
        RemoveCommand.NotifyCanExecuteChanged();
        AddCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private async Task Add()
    {
        // Items.Add(new ParameterItem { Name = string.Empty, Source = "Primitive", Type = "String", Value = string.Empty });

        var parameterItem = new ParameterItem
            { Name = "<Change this>", Source = "Primitive", Type = "String", Value = "<Change this>" };

        var isPopupOk = await DisplayParameterEditPopup(parameterItem);

        if (isPopupOk)
        {
            Items.Add(parameterItem);
        }
    }

    [RelayCommand(CanExecute = nameof(CanRemove))]
    private void Remove()
    {
        if (SelectedItem != null)
        {
            Items.Remove(SelectedItem);
            SelectedItem = null;
        }
    }

    private bool CanRemove() => SelectedItem != null;

    public async Task<bool> DisplayParameterEditPopup(ParameterItem item)
    {
        var queryAttributes = new Dictionary<string, object>
        {
            [nameof(ParameterItem)] = item
        };

        var popupOptions = new PopupOptions
        {
            CanBeDismissedByTappingOutsideOfPopup = false
        };

        var popupResult = await popupService.ShowPopupAsync<ParameterEditorPopupViewModel, bool>(
            Shell.Current,
            options: popupOptions,
            shellParameters: queryAttributes);

        return popupResult.Result;
    }

    public void InitializeFromNode(NodeInject nodeInject)
    {
        Items.Clear();

        AddParameters(nodeInject.Parameters);

        RemoveCommand.NotifyCanExecuteChanged();
        AddCommand.NotifyCanExecuteChanged();
    }

    private void AddParameters(List<Parameter> parameters)
    {

        foreach (var nodeInject in parameters)
        {
            Items.Add(new ParameterItem
            {
                Name = nodeInject.Name, Source = nodeInject.Source.ToTitleCase(), Type = nodeInject.Type.ToTitleCase(),
                Value = nodeInject.Value
            });
        }
    }
}

public partial class ParameterItem : ObservableObject
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

    public ParameterItem Copy()
    {
        return new ParameterItem()
        {
            Name = this.Name,
            Source = this.Source,
            Type = this.Type,
            Value = this.Value,

            IsTypeEditable = this.IsTypeEditable,
            IsValueVisible = this.IsValueVisible
        };
    }

    public void Past(ParameterItem item)
    {
        Name = item.Name;
        Source = item.Source;
        Type = item.Type;
        Value = item.Value;

        IsTypeEditable = item.IsTypeEditable;
        IsValueVisible = item.IsValueVisible;
        
        OnPropertyChanged(string.Empty); // Notify all properties changed
        
    }
}