using NodeSharp.Nodes.Common.Services;
using NodeSharp.Nodes.Common.ViewModels;

namespace NodeSharp.Nodes.Common.Components;

public partial class LastOutputMessageTooltipComponent : ContentView
{
    public LastOutputMessageTooltipComponent()
    {
        var viewModel = AppService.GetRequiredService<LastOutputMessageTooltipViewModel>();
        
        InitializeComponent();
  
        this.BindingContext = viewModel;
    }
}