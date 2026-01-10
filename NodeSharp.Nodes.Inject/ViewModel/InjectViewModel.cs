using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NodeSharp.Nodes.Common.Helper;

namespace NodeSharp.Nodes.Inject.ViewModel
{
    [SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator",
        "MVVMTK0045:Using [ObservableProperty] on fields is not AOT compatible for WinRT")]
    [SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator",
        "MVVMTK0034:Direct field reference to [ObservableProperty] backing field")]
    [SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator",
        "MVVMTK0007:Invalid RelayCommand method signature")]
    public partial class InjectViewModel(IPopupService popupService)
        : ObservableObject, IQueryAttributable
    {
        [ObservableProperty] private NodeInject? selectedNode;
        [ObservableProperty] private string codeText = string.Empty;
        [ObservableProperty] private bool isSaveEnabled;
        [ObservableProperty] private bool hasCompilerError;
        [ObservableProperty] private bool hasBeenValidated;
        [ObservableProperty] private string? compilerOutput;

        [RelayCommand(CanExecute = nameof(CanSave))]
        async Task Save()
        {
            await popupService.ClosePopupAsync(Shell.Current);
        }

        [RelayCommand(CanExecute = nameof(CanCancel))]
        async Task Cancel()
        {
            await popupService.ClosePopupAsync(Shell.Current);
        }

        bool CanSave()
        {
            return false;
        }

        bool CanCancel()
        {
            return true;
        }
        
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            SelectedNode = (NodeInject)query[nameof(NodeInject)];
        }
    }
}