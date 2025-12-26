using System.Text;
using CommunityToolkit.Maui.Storage;
using NodeSharp.Client.Services;
using NodeSharp.Client.ViewModel;
using NodeSharp.NodeEngine;
using NodeSharp.NodeEngine.Model;

namespace NodeSharp.Client.Component;

public partial class NodeToolListComponent : Microsoft.Maui.Controls.ContentView
{
    private readonly NodeToolListModel viewModel;
    private readonly DiagramViewModel diagramViewModel;
    private readonly Main main;

    public NodeToolListComponent()
    {
        viewModel = AppService.GetRequiredService<NodeToolListModel>();
        diagramViewModel = AppService.GetRequiredService<DiagramViewModel>();
        main = AppService.GetRequiredService<Main>();

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
        var app = Application.Current as App;
        if (app != null)
        {
            app.ToggleTheme();
        }
    }


    private async void OnSaveTapped(object sender, EventArgs e)
    {
        try
        {
            var nodes = diagramViewModel.Nodes;

            foreach (var nodeBox in nodes)
            {
                nodeBox.Node.X = nodeBox.X;
                nodeBox.Node.Y = nodeBox.Y;
            }
            
            if (!string.IsNullOrEmpty(main.FileNameSaved))
            {
                await main.SaveToFileAsync();
            }
        }
        catch (Exception exception)
        {
            throw; // TODO handle exception
        }
    }

    private async void OnSaveAsTapped(object sender, EventArgs e)
    {
        try
        {
            var nodes = diagramViewModel.Nodes;

            foreach (var nodeBox in nodes)
            {
                nodeBox.Node.X = nodeBox.X;
                nodeBox.Node.Y = nodeBox.Y;
            }
            
            var ms = new MemoryStream();
            await main.SaveToFileAsync(ms);
            
            var fileSaverResult = await FileSaver.Default.SaveAsync(
                "Nodes.json", 
                ms,
                CancellationToken.None);

            if (fileSaverResult.IsSuccessful)
            {
                // User picked a location, file saved successfully
                main.FileNameSaved = fileSaverResult.FilePath;
                Console.WriteLine($"File saved at: {main.FileNameSaved}");
            }
            else
            {
                // Handle error or cancellation
                Console.WriteLine($"Error: {fileSaverResult.Exception?.Message}");
            }

            // await main.SaveToFileAsync("Test.json");
        }
        catch (Exception exception)
        {
            throw; // TODO handle exception
        }
    }

    private async void OnLoadTapped(object sender, EventArgs e)
    {
        try
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
                string content = await reader.ReadToEndAsync();

                Console.WriteLine($"File content: {content}");
                
                // await diagramViewModel.Init(reader);
            }
            else
            {
                Console.WriteLine("User canceled file picking.");
            }
            
        }
        catch (Exception exception)
        {
            throw; // TODO handle exception
        }
    }
    



    private void OnNewTapped(object sender, EventArgs e)
    {
        diagramViewModel.Clear();
    }

    private void OnQuitTapped(object sender, EventArgs e)
    {
        Application.Current?.Quit();
    }
}