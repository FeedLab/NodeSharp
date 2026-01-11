using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace NodeSharp.Nodes.Inject.ViewModel
{
    public partial class InjectViewModel : ObservableObject, IQueryAttributable
    {
        [ObservableProperty] private NodeInject? selectedNode;
        [ObservableProperty] private bool isSaveEnabled;
        [ObservableProperty] private bool allowNull;
        [ObservableProperty] private decimal delayValue;
        [ObservableProperty] private string timeScaleDelay;
        [ObservableProperty] private string timeScaleInterval;
        
        
        private readonly IPopupService popupService;

        /// <inheritdoc/>
        public InjectViewModel(IPopupService popupService)
        {
            this.popupService = popupService;

            TimeScaleDelay = "None";
            TimeScaleInterval = "None";
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
            SelectedNode = (NodeInject)query[nameof(NodeInject)];
        }
    }
}