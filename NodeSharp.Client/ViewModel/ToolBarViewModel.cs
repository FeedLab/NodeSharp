using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using NodeSharp.Client.Component;
using NodeSharp.Client.Extension;
using NodeSharp.NodeEngine;
using NodeSharp.NodeEngine.Exception;

namespace NodeSharp.Client.ViewModel;

[SuppressMessage("Usage", "CsWinRT1030:Project does not enable unsafe blocks")]
public partial class ToolBarViewModel : ObservableObject
{
    [ObservableProperty] private bool isSaveEnabled = false;
    [ObservableProperty] private bool isSaveAsEnabled = false;
    [ObservableProperty] private bool isLoadEnabled = true;
    [ObservableProperty] private bool isNewEnabled = true;
    [ObservableProperty] private bool isQuitEnabled = true;
    private readonly DiagramViewModel diagramViewModel;
    private readonly LineConnectionManager lineConnectionManager;
    private readonly NodeIo nodeIo;
    private readonly IPopupService popupService;
    private readonly ErrorPopupViewModel errorPopupViewModel;
    private readonly DebugViewModel debugViewModel;

    /// <inheritdoc/>
    public ToolBarViewModel(DiagramViewModel diagramViewModel, LineConnectionManager lineConnectionManager,
        NodeIo nodeIo, IPopupService popupService, ErrorPopupViewModel errorPopupViewModel, DebugViewModel debugViewModel)
    {
        this.diagramViewModel = diagramViewModel;
        this.lineConnectionManager = lineConnectionManager;
        this.nodeIo = nodeIo;
        this.popupService = popupService;
        this.errorPopupViewModel = errorPopupViewModel;
        this.debugViewModel = debugViewModel;

        // Subscribe to collection changes to refresh command states
        nodeIo.Nodes.CollectionChanged += (s, e) => { UpdateToolbarCommandStates(); };
    }

    private void UpdateToolbarCommandStates()
    {
        LoadCommand.NotifyCanExecuteChanged();
        SaveCommand.NotifyCanExecuteChanged();
        SaveAsCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanDoSave))]
    private async Task Save()
    {
        Console.WriteLine("Save executed!");

        try
        {
            var nodes = diagramViewModel.BoxNodes;

            foreach (var nodeBox in nodes)
            {
                nodeBox.Node.X = (int)nodeBox.X;
                nodeBox.Node.Y = (int)nodeBox.Y;
            }

            if (!string.IsNullOrEmpty(nodeIo.FileNameSaved))
            {
                await nodeIo.SaveToFileAsync();
            }
        }
        catch (Exception exception)
        {
            throw; // TODO handle exception
        }
    }

    [RelayCommand(CanExecute = nameof(CanDoSaveAs))]
    private async Task SaveAs()
    {
        Console.WriteLine("SaveAs executed!");

        var ms = new MemoryStream();
        await nodeIo.SaveToFileAsync(ms);

        var fileSaverResult = await FileSaver.Default.SaveAsync(
            "Nodes.json",
            ms,
            CancellationToken.None);

        if (fileSaverResult.IsSuccessful)
        {
            // User picked a location, file saved successfully
            nodeIo.FileNameSaved = fileSaverResult.FilePath;
            Console.WriteLine($"File saved at: {nodeIo.FileNameSaved}");

            UpdateToolbarCommandStates();
        }
        else
        {
            // Handle error or cancellation
            Console.WriteLine($"Error: {fileSaverResult.Exception?.Message}");
        }
    }

    [RelayCommand(CanExecute = nameof(CanDoLoad))]
    private async Task Load()
    {
        Console.WriteLine("Load executed!");

        try
        {
            diagramViewModel.Clear();
            WeakReferenceMessenger.Default.Send(new NodeActionEvent { ActionEventType = NodeActionEventType.Reset});
            
            WeakReferenceMessenger.Default.Send(new ConnectionPointStatus { IsCanvasInvalid = false });

            await PickFileAsync();

            lineConnectionManager.RecalculateLines(diagramViewModel.BoxNodes);

            WeakReferenceMessenger.Default.Send(new ConnectionPointStatus { IsCanvasInvalid = false });

            debugViewModel.UpdateToolbarCommandStates();
        }
        // catch (NodeParseException nodeParseException)
        // {       
        //     Debug.WriteLine(nodeParseException.Message);
        // }
        catch (Exception e)
        {
            await e.ShowPopupAsync("Error!!!");

            Debug.WriteLine(e.Message);
        }
    }

    [RelayCommand(CanExecute = nameof(CanDoNew))]
    private Task New()
    {
        Console.WriteLine("New executed!");

        nodeIo.Clear();
            WeakReferenceMessenger.Default.Send(new NodeActionEvent { ActionEventType = NodeActionEventType.Reset});
        WeakReferenceMessenger.Default.Send(new ConnectionPointStatus { IsCanvasInvalid = true });

        UpdateToolbarCommandStates();
        debugViewModel.UpdateToolbarCommandStates();

        return Task.CompletedTask;
    }

    [RelayCommand(CanExecute = nameof(CanDoQuit))]
    private Task Quit()
    {
        Console.WriteLine("Quit executed!");

        Application.Current?.Quit();

        return Task.CompletedTask;
    }

    private bool CanDoSave()
    {
        IsSaveEnabled = diagramViewModel.BoxNodes.Count != 0 && nodeIo.FileNameSaved != null;
        return IsSaveEnabled;
    }

    private bool CanDoSaveAs()
    {
        IsSaveAsEnabled = diagramViewModel.BoxNodes.Count != 0;

        return IsSaveAsEnabled;
    }

    private bool CanDoLoad()
    {
        IsLoadEnabled = diagramViewModel.BoxNodes.Count == 0;

        return IsLoadEnabled;
    }

    private bool CanDoNew()
    {
        return IsNewEnabled;
    }

    private bool CanDoQuit()
    {
        return IsQuitEnabled;
    }

    private async Task PickFileAsync()
    {
        var result = await FilePicker.Default.PickAsync();

        if (result != null)
        {
            // Full path (Windows/macOS only; on mobile you get a stream)
            var filePath = result.FullPath;
            Console.WriteLine($"Picked file: {filePath}");

            // Open as stream
            using var stream = await result.OpenReadAsync();
            using var reader = new StreamReader(stream);
            // var content = await reader.ReadToEndAsync();

            await diagramViewModel.Init(reader, filePath);

            Console.WriteLine($"File content: {filePath}");
        }
        else
        {
            Console.WriteLine("User canceled file picking.");
        }
    }
}

public class ConnectionPointStatus
{
    public bool IsCanvasInvalid { get; set; }
}