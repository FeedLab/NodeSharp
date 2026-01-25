using NodeSharp.Client.ViewModel;
using NodeSharp.NodeEngine;
using NodeSharp.Nodes.Common.Model;
using NodeSharp.Nodes.Common.Services;
using Syncfusion.Maui.ListView;
using MauiApplication = Microsoft.Maui.Controls.Application;
using MauiDragStartingEventArgs = Microsoft.Maui.Controls.DragStartingEventArgs;
using MauiPointerEventArgs = Microsoft.Maui.Controls.PointerEventArgs;
#if WINDOWS
using Microsoft.UI.Input;
#endif

namespace NodeSharp.Client.Component;

public partial class NodeToolListComponent : Microsoft.Maui.Controls.ContentView
{
    private readonly NodeToolListModel viewModel;
    private readonly DiagramViewModel diagramViewModel;
    private readonly NodeIo nodeIo;

    public NodeToolListComponent()
    {
        viewModel = AppService.GetRequiredService<NodeToolListModel>();
        diagramViewModel = AppService.GetRequiredService<DiagramViewModel>();
        nodeIo = AppService.GetRequiredService<NodeIo>();

        InitializeComponent();

        this.BindingContext = viewModel;
    }

    public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string),
        typeof(NodeToolListComponent), string.Empty, propertyChanged: OnTitleChanged);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    private static void OnTitleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (NodeToolListComponent)bindable;
        // control.TitleLabel.Text = (string)newValue;
    }

    private void OnSearchClicked(object sender, EventArgs e)
    {
        if (MauiApplication.Current is App app)
        {
            app.ToggleTheme();
        }
    }


    // private async void OnSaveTapped(object sender, EventArgs e)
    // {
    //     try
    //     {
    //         var nodes = diagramViewModel.BoxNodes;
    //
    //         foreach (var nodeBox in nodes)
    //         {
    //             nodeBox.Node.X = (int)nodeBox.X;
    //             nodeBox.Node.Y = (int)nodeBox.Y;
    //         }
    //         
    //         if (!string.IsNullOrEmpty(nodeIo.FileNameSaved))
    //         {
    //             await nodeIo.SaveToFileAsync();
    //         }
    //     }
    //     catch (Exception exception)
    //     {
    //         throw new Exception("OnSaveTapped: error during save process.", exception);
    //     }
    // }

    // private async void OnSaveAsTapped(object sender, EventArgs e)
    // {
    //     try
    //     {
    //         var nodes = diagramViewModel.BoxNodes;
    //
    //         foreach (var nodeBox in nodes)
    //         {
    //             nodeBox.Node.X = (int)nodeBox.X;
    //             nodeBox.Node.Y = (int)nodeBox.Y;
    //         }
    //         
    //         var ms = new MemoryStream();
    //         await nodeIo.SaveToFileAsync(ms);
    //         
    //         var fileSaverResult = await FileSaver.Default.SaveAsync(
    //             "Nodes.json", 
    //             ms,
    //             CancellationToken.None);
    //
    //         if (fileSaverResult.IsSuccessful)
    //         {
    //             // User picked a location, file saved successfully
    //             nodeIo.FileNameSaved = fileSaverResult.FilePath;
    //             Console.WriteLine($"File saved at: {nodeIo.FileNameSaved}");
    //         }
    //         else
    //         {
    //             // Handle error or cancellation
    //             Console.WriteLine($"Error: {fileSaverResult.Exception?.Message}");
    //         }
    //
    //         // await main.SaveToFileAsync("Test.json");
    //     }
    //     catch (Exception exception)
    //     {
    //         throw; // TODO handle exception
    //     }
    // }

    // private async void OnLoadTapped(object sender, EventArgs e)
    // {
    //     try
    //     {
    //         var result = await FilePicker.Default.PickAsync();
    //
    //         if (result != null)
    //         {
    //             // Full path (Windows/macOS only; on mobile you get a stream)
    //             var filePath = result.FullPath;
    //             Console.WriteLine($"Picked file: {filePath}");
    //
    //             // Open as stream
    //             using var stream = await result.OpenReadAsync();
    //             using var reader = new StreamReader(stream);
    //             string content = await reader.ReadToEndAsync();
    //
    //             Console.WriteLine($"File content: {content}");
    //             
    //             // await diagramViewModel.Init(reader);
    //         }
    //         else
    //         {
    //             Console.WriteLine("User canceled file picking.");
    //         }
    //         
    //     }
    //     catch (Exception exception)
    //     {
    //         throw; // TODO handle exception
    //     }
    // }
    



    private void OnNewTapped(object sender, EventArgs e)
    {
        diagramViewModel.Clear();
    }

    private void OnQuitTapped(object sender, EventArgs e)
    {
        MauiApplication.Current?.Quit();
    }

    private void OnItemDragging(object sender, ItemDraggingEventArgs e)
    {
        if (e.Action == DragAction.Start)
        {
            // The DataItem is already set by the control, you can access it here if needed
            var draggedData = e.DataItem;
        }


    }
    
    private void OnDragStarting(object sender, MauiDragStartingEventArgs e)
    {
        var border = (Element)sender;
        var nodeInfo = (INodeInformation)border.BindingContext;

        // Package the data so the DropGestureRecognizer can see it
        e.Data.Properties.Add("Data", nodeInfo);
    }

    private void OnItemPointerEntered(object? sender, MauiPointerEventArgs e)
    {
#if WINDOWS
        if (sender is VisualElement element)
        {
            TrySetPointerCursor(element, InputSystemCursorShape.Hand);
        }
#endif
    }

    private void OnItemPointerExited(object? sender, MauiPointerEventArgs e)
    {
#if WINDOWS
        if (sender is VisualElement element)
        {
            TrySetPointerCursor(element, InputSystemCursorShape.Arrow);
        }
#endif
    }

#if WINDOWS
    private static void TrySetPointerCursor(VisualElement element, InputSystemCursorShape shape)
    {
        if (element.Handler?.PlatformView is not object platformView)
        {
            return;
        }

        var pointerCursorProperty = platformView.GetType().GetProperty("PointerCursor");
        if (pointerCursorProperty != null)
        {
            var cursor = InputSystemCursor.Create(shape);
            pointerCursorProperty.SetValue(platformView, cursor);
            return;
        }

        var coreWindow = Windows.UI.Core.CoreWindow.GetForCurrentThread();
        if (coreWindow != null)
        {
            coreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(
                shape == InputSystemCursorShape.Hand
                    ? Windows.UI.Core.CoreCursorType.Hand
                    : Windows.UI.Core.CoreCursorType.Arrow,
                0);
        }
    }
#endif
}
