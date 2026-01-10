using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace NodeSharp.Nodes.Random.ViewModel
{

    public partial class RandomConfigurePopupViewModel(IPopupService popupService)
        : ObservableObject, IQueryAttributable
    {
        [ObservableProperty] private NodeRandomNumber? selectedNode;
        [ObservableProperty] private bool isSaveEnabled;

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
            SelectedNode = (NodeRandomNumber)query[nameof(NodeRandomNumber)];
        }
    }
}