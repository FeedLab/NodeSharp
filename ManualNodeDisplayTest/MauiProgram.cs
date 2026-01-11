using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using NodeSharp.Nodes.Common;
using NodeSharp.Nodes.Common.Helper;
using NodeSharp.Nodes.Inject;
using NodeSharp.Nodes.Inject.ViewModel;
using Syncfusion.Maui.Core.Hosting;

namespace ManualNodeDisplayTest;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureSyncfusionCore()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });
        
        builder.Services.AddSingletonPopup<InjectConfigurePopupComponent, InjectViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif
        
        RegisterDynamicNodes(builder);

        return builder.Build();
    }
    
    private static void RegisterDynamicNodes(MauiAppBuilder builder)
    {
        var types = AssemblyHelper.FindImplementations<INodeSharp>(AppContext.BaseDirectory);

        foreach (var type in types)
        {
            var instance = (INodeSharp)Activator.CreateInstance(type)!;

            instance.Register(builder.Services);
        }
    }
}