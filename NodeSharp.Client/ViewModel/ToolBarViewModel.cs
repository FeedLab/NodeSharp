using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace NodeSharp.Client.ViewModel;

public partial class ToolBarViewModel(DiagramViewModel diagramViewModel) : ObservableObject
{
    [ObservableProperty]
    private bool isSaveEnabled = true;
    [ObservableProperty]
    private bool isSaveAsEnabled = true;
    [ObservableProperty]
    private bool isLoadEnabled = true;
    [ObservableProperty]
    private bool isNewEnabled = true;
    [ObservableProperty]
    private bool isQuitEnabled = true;

    [RelayCommand(CanExecute = nameof(CanDoSave))]
    private Task Save()
    {
        Console.WriteLine("Save executed!");
        
        return Task.CompletedTask;
    }
    
    [RelayCommand(CanExecute = nameof(CanDoSaveAs))]
    private Task SaveAs()
    {
        Console.WriteLine("SaveAs executed!");
        
        return Task.CompletedTask;
    }
    
    [RelayCommand(CanExecute = nameof(CanDoLoad))]
    private async void Load()
    {
        Console.WriteLine("Load executed!");

        await PickFileAsync();
    }
    
    [RelayCommand(CanExecute = nameof(CanDoNew))]
    private Task New()
    {
        Console.WriteLine("New executed!");
        
        diagramViewModel.Clear();
        
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
        return IsSaveEnabled; 
    }
    
    private bool CanDoSaveAs()
    {
        return IsSaveAsEnabled; 
    }
    
    private bool CanDoLoad()
    {
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
    
    public async Task PickFileAsync()
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