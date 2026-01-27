using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NodeSharp.Nodes.Common.Extension;

namespace NodeSharp.Nodes.Inject.ViewModel
{
    [SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "MVVMTK0045:Using [ObservableProperty] on fields is not AOT compatible for WinRT")]
    public partial class InjectConfigurePopupViewModel : ObservableObject, IQueryAttributable
    {
        [ObservableProperty] private NodeInject? selectedNode;
        [ObservableProperty] private bool isSaveEnabled;
        [ObservableProperty] private bool allowNull;
        [ObservableProperty] private int delayValue;
        [ObservableProperty] private int intervalValue;
        [ObservableProperty] private string timeScaleDelay;
        [ObservableProperty] private string timeScaleInterval;
        
        
        private readonly IPopupService popupService;
        private readonly NodeInjectParameterViewModel injectConfigurePopupViewModel;

        /// <inheritdoc/>
        public InjectConfigurePopupViewModel(IPopupService popupService, NodeInjectParameterViewModel injectConfigurePopupViewModel)
        {
            this.popupService = popupService;
            this.injectConfigurePopupViewModel = injectConfigurePopupViewModel;

            TimeScaleDelay = "None";
            TimeScaleInterval = "None";
        }

        [RelayCommand(CanExecute = nameof(CanSave))]
        async Task Save()
        {
            if (SelectedNode is null)
            {
                throw new InvalidOperationException("SelectedNode cannot be null when saving inject parameters.");
            }
            
            SelectedNode.Parameters.Clear();
            AddDelay();
            AddRepeat();
            
            foreach (var item in injectConfigurePopupViewModel.Items)
            {
                SelectedNode.Parameters.Add(new Parameter(item.Name, item.Type, item.Source, item.Value));
            }
            
            await popupService.ClosePopupAsync(Shell.Current);
        }

        [RelayCommand(CanExecute = nameof(CanCancel))]
        async Task Cancel()
        {
            await popupService.ClosePopupAsync(Shell.Current);
        }

        bool CanSave()
        {
            return injectConfigurePopupViewModel.Items.Count > 0;
        }

        bool CanCancel()
        {
            return true;
        }
        
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            SelectedNode = (NodeInject)query[nameof(NodeInject)];
            
            if (SelectedNode is null)
            {
                throw new InvalidOperationException("SelectedNode cannot be null when ApplyQueryAttributes.");
            }

            injectConfigurePopupViewModel.InitializeFromNode(SelectedNode);

            AddDelay(SelectedNode.ActivateAfter);
            AddRepeat(SelectedNode.Repeat);

            CancelCommand.NotifyCanExecuteChanged();
            SaveCommand.NotifyCanExecuteChanged();
        }
        
        private void AddRepeat(Repeat interval)
        {
            IntervalValue = interval.Value;
            TimeScaleInterval = interval.Type.ToTitleCase();
        }
        
        private void AddRepeat()
        {
            if (SelectedNode is null)
            {
                return;
            }
            
            SelectedNode.Repeat.Value = IntervalValue;
            SelectedNode.Repeat.Type = TimeScaleInterval;
        }

        private void AddDelay(ActivateAfter delay)
        {
            DelayValue = delay.Value;
            TimeScaleDelay = delay.Type.ToTitleCase();
        }
        
        private void AddDelay()
        {
            if (SelectedNode is null)
            {
                return;
            }
            
            SelectedNode.ActivateAfter.Value = DelayValue;
            SelectedNode.ActivateAfter.Type = TimeScaleDelay;
        }
    }
}