using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NodeSharp.NodeEngine;

namespace NodeSharp.Client.ViewModel;

public partial class DebugViewModel : ObservableObject
{
    private readonly NodeIo nodeIo;

    public DebugViewModel(NodeIo nodeIo)
    {
        this.nodeIo = nodeIo;
        DebugInfo = new ObservableCollection<DebugData>();

        FileLogger.OnLogAdded += (sender, tuple) =>
        {
            System.Diagnostics.Debug.WriteLine($"OnLogAdded fired: {tuple.level} - {tuple.message}");
            MainThread.BeginInvokeOnMainThread(() =>
            {
                System.Diagnostics.Debug.WriteLine($"Adding to DebugInfo collection. Count before: {DebugInfo.Count}");
                DebugInfo.Add(new DebugData(tuple.message, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss:}: {tuple.level}", tuple.entry));
                System.Diagnostics.Debug.WriteLine($"Count after: {DebugInfo.Count}");
            });
        };
    }
    public ObservableCollection<DebugData>? DebugInfo { get; }
    
    [RelayCommand(CanExecute = nameof(CanDoRun))]
    private async Task Run()
    {
        Console.WriteLine("Run executed!");

        await nodeIo.Run();
    }
    
    private bool CanDoRun()
    {
        return true;
    }
}

[SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "MVVMTK0045:Using [ObservableProperty] on fields is not AOT compatible for WinRT")]
public partial class DebugData : ObservableObject
{
    [ObservableProperty]
    private string message;
    [ObservableProperty]
    private string level;
    [ObservableProperty]
    private string entry; 

    [ObservableProperty]
    private List<string> messages = ["qwerty", "asdfgh", "zxcvbn"];

    public DebugData(string message, string level, string entry)
    {
        Message = message;
        Level = level;
        Entry = entry;
        
        messages.Insert(0, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss:}: {message}");
    }
}