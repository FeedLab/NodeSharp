using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using NodeSharp.NodeEngine;
using NodeSharp.Nodes.Common.Helper;

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
            WeakReferenceMessenger.Default.Register<NodeActionEvent>(this, (sender, args) =>
            {
                if (args.ActionEventType == NodeActionEventType.Add ||
                    args.ActionEventType == NodeActionEventType.Delete)
                {
                    UpdateToolbarCommandStates();
                }
            });
            
    }
    
    public void UpdateToolbarCommandStates()
    {
        RunCommand.NotifyCanExecuteChanged();
        StopCommand.NotifyCanExecuteChanged();
        ClearCommand.NotifyCanExecuteChanged();
    }
    
    public ObservableCollection<DebugData>? DebugInfo { get; }

    [RelayCommand(CanExecute = nameof(CanDoClear))]
    private void Clear()
    {
        Debug.WriteLine("Clear debug window!");

        DebugInfo?.Clear();
        
        UpdateToolbarCommandStates();
    }


    [RelayCommand(CanExecute = nameof(CanDoRun))]
    private async Task Run()
    {
        Debug.WriteLine("Run executed!");

        await nodeIo.Run();
        
        UpdateToolbarCommandStates();
    }
    
    [RelayCommand(CanExecute = nameof(CanDoStop))]
    private void Stop()
    {
        Debug.WriteLine("Stop executed!");
        
        nodeIo.Abort();
        UpdateToolbarCommandStates();
    }

    private bool CanDoClear()
    {
        return nodeIo.IsFlowRunning;
    }
    private bool CanDoStop()
    {
        return nodeIo.IsFlowRunning;
    }

    private bool CanDoRun()
    {
        return nodeIo.Nodes.Count > 0 && !nodeIo.IsFlowRunning;
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