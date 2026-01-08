using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;
using Microsoft.Extensions.Logging;
using NodeSharp.Client.Component;
using NodeSharp.Client.ViewModel;
using NodeSharp.NodeEngine;
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
                fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
                fonts.AddFont("MaterialSymbolsOutlined-Regular.ttf", "MaterialSymbols");
            });
        
        builder.Services.AddSingleton<Storage>();
        builder.Services.AddSingleton<NodeToolListModel>();
        builder.Services.AddSingleton<DebugViewModel>();
        builder.Services.AddSingleton<DiagramViewModel>();
        builder.Services.AddScoped<NodeInformationModel>();
        builder.Services.AddScoped<MainPageModel>();
        builder.Services.AddSingleton<NodeIo>();
        builder.Services.AddScoped<ToolBarViewModel>();
        builder.Services.AddSingleton<CurvedLineDrawable>();
        builder.Services.AddSingleton<LineConnectionManager>();
        builder.Services.AddSingletonPopup<CodeComponent, CodeViewModel>();
        
        builder.Services.AddTransientPopup<ErrorPopup, ErrorPopupViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}