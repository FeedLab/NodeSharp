using CommunityToolkit.Maui;
using Microsoft.Extensions.Options;
using NodeSharp.Nodes.Common.Components;
using NodeSharp.Nodes.Common.Configuration;
using NodeSharp.Nodes.Common.ViewModels;

namespace NodeSharp.Nodes.Common;
using Options = Microsoft.Extensions.Options.Options;

public static class Startup
{
    public static void Register(IServiceCollection services, NodeSharpSettings settings)
    {
        services.AddSingleton(settings);
        services.AddSingleton(Options.Create(settings.Grid));
        services.AddSingleton(Options.Create(settings.Directories));

        services.AddSingletonPopup<LastOutputMessageTooltipComponent, LastOutputMessageTooltipViewModel>();
        services.AddTransientPopup<ErrorPopup, ErrorPopupViewModel>();
        services.AddTransientPopup<NodeExplanationsPopup, NodeExplanationsPopupViewModel>();

        services.AddTransient<DefaultBoxNodeBodyViewModel>();
    }
}