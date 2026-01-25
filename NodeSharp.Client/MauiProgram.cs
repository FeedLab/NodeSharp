using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;
using Microsoft.Extensions.Logging;
using NodeSharp.Client.Component;
using NodeSharp.Client.Configuration;
using NodeSharp.Client.ViewModel;
using NodeSharp.NodeEngine;
using NodeSharp.Nodes.Common;
using NodeSharp.Nodes.Common.Helper;
using NodeSharp.Nodes.Common.Model;
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
                fonts.AddFont("Font Awesome 7 Free-Solid-900.otf", "FontSolid");
                fonts.AddFont("MauiSampleFontIcon.ttf", "MauiSampleFontIcon");
                fonts.AddFont("AccordionFontIcons.ttf", "AccordionFontIcons");
                fonts.AddFont("Roboto-Medium.ttf", "Roboto-Medium");
                fonts.AddFont("Roboto-Regular.ttf", "Roboto-Regular");
                fonts.AddFont("TimesNewRoman.ttf", "TimesNewRoman");
                fonts.AddFont("Verdana.ttf", "Verdana");
                fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
                fonts.AddFont("MaterialSymbolsOutlined-Regular.ttf", "MaterialSymbols");
            });

        var settings = NodeSharpSettings.Load("NodeSharp.json");
        builder.Services.AddSingleton(settings);
        
        builder.Services.AddSingleton<NodeToolListModel>();
        builder.Services.AddSingleton<DebugViewModel>();
        builder.Services.AddSingleton<DiagramViewModel>();
        builder.Services.AddScoped<MainPageModel>();
        builder.Services.AddSingleton<NodeIo>();
        builder.Services.AddScoped<ToolBarViewModel>();
        builder.Services.AddSingleton<CurvedLineDrawable>();
        builder.Services.AddSingleton<GridBackgroundDrawable>();
        builder.Services.AddSingleton<LineConnectionManager>();
        

        var storage = new Storage();
        RegisterDynamicNodes(builder.Services, storage);

        builder.Services.AddSingleton(storage);

        Startup.Register(builder.Services);

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        
        return app;
    }

    private static void RegisterDynamicNodes(IServiceCollection serviceCollection, Storage storage)
    {

        var types = AssemblyHelper.FindImplementations<INodeSharp>(AppContext.BaseDirectory);

        foreach (var type in types)
        {

            var instance = (INodeSharp)Activator.CreateInstance(type)!;
            instance.Register(serviceCollection);

            storage.GetNodeInformation().AddType(instance);
        }
    }
}
