using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using NodeSharp.Nodes.Common;
using NodeSharp.Nodes.Common.Helper;
using NodeSharp.Nodes.Inject;
using NodeSharp.Nodes.Inject.Component;
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
                fonts.AddFont("OpenSans-SemiBold.ttf", "OpenSansSemiBold");
                fonts.AddFont("MauiMaterialAssets.ttf", "MaterialAssets");
                fonts.AddFont("Font Awesome 6 Free-Solid-900.otf", "FontSolid");
                fonts.AddFont("MauiSampleFontIcon.ttf", "MauiSampleFontIcon");
                fonts.AddFont("AccordionFontIcons.ttf", "AccordionFontIcons");
                fonts.AddFont("Roboto-Medium.ttf", "Roboto-Medium");
                fonts.AddFont("Roboto-Regular.ttf", "Roboto-Regular");
                fonts.AddFont("TimesNewRoman.ttf", "TimesNewRoman");
                fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
                fonts.AddFont("MaterialSymbolsOutlined-Regular.ttf", "MaterialSymbols");
            });
        
        builder.Services.AddSingletonPopup<InjectConfigurePopupComponent, InjectConfigurePopupViewModel>();

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