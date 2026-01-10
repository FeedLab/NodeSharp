using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace NodeSharp.Nodes.Delay
{
    public partial class DelayConfigurePopupViewModel : ObservableObject, IQueryAttributable
    {
        [ObservableProperty] private NodeDelay? selectedNode;
        [ObservableProperty] private bool isSaveEnabled;
        private readonly IPopupService popupService;

        /// <inheritdoc/>
        public DelayConfigurePopupViewModel(IPopupService popupService)
        {
            this.popupService = popupService;
        }

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
            SelectedNode = (NodeDelay)query[nameof(NodeDelay)];
        }
    }
}