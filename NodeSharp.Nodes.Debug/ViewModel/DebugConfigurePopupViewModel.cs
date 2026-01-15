using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace NodeSharp.Nodes.Debug.ViewModel
{
    public partial class DebugConfigurePopupViewModel(IPopupService popupService) : ObservableObject, IQueryAttributable
    {
        [ObservableProperty] private NodeDebug? selectedNode;
        [ObservableProperty] private bool isSaveEnabled;

        [RelayCommand(CanExecute = nameof(CanSave))]
        private async Task Save()
        {
            await popupService.ClosePopupAsync(Shell.Current);
        }

        [RelayCommand(CanExecute = nameof(CanCancel))]
        private async Task Cancel()
        {
            await popupService.ClosePopupAsync(Shell.Current);
        }

        private bool CanSave()
        {
            return false;
        }

        private bool CanCancel()
        {
            return true;
        }
        
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            SelectedNode = (NodeDebug)query[nameof(NodeDebug)];
        }
    }
}