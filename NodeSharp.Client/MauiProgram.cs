using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;
using Microsoft.Extensions.Logging;
using NodeSharp.Client.ViewModel;
using NodeSharp.NodeEngine.Model;
using Syncfusion.Maui.Core.Hosting;
using Syncfusion.Maui.Toolkit.Hosting;

namespace NodeSharp.Client;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureSyncfusionToolkit()
            .UseMauiCommunityToolkitMarkup()
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
            });
        
        builder.Services.AddSingleton<Storage>();
        builder.Services.AddSingleton<NodeToolListComponentModel>();
        builder.Services.AddScoped<NodeInformationModel>();
        builder.Services.AddScoped<MainPageModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}