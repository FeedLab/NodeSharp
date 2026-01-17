using CommunityToolkit.Maui;
using NodeSharp.Nodes.Common.Components;
using NodeSharp.Nodes.Common.ViewModels;

namespace NodeSharp.Nodes.Common;

public static class Startup
{
    public static void Register(IServiceCollection services)
    {
        services.AddSingletonPopup<LastOutputMessageTooltipComponent, LastOutputMessageTooltipViewModel>();
        services.AddTransientPopup<ErrorPopup, ErrorPopupViewModel>();
        
        services.AddTransient<DefaultBoxNodeBodyViewModel>();
    }
}