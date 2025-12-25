using Microsoft.Extensions.Logging;
using NodeSharp.NodeEngine;

namespace NodeSharp.Client.ViewModel;

public class MainPageModel
{
    private readonly NodeToolListComponentModel nodeToolListComponentModel;
    private readonly Main main;
    private readonly DiagramViewModel diagramViewModelModel;

    public MainPageModel(ILogger<MainPageModel> logger,  NodeToolListComponentModel nodeToolListComponentModel, Main main, DiagramViewModel diagramViewModelModel)
    {
        this.main = main;
        this.diagramViewModelModel = diagramViewModelModel;
        this.nodeToolListComponentModel = nodeToolListComponentModel;
    }

    public void Init()
    {
        nodeToolListComponentModel.Init();
        
        // Task.Run(async () =>
        // {
        //     await diagramViewModelModel.Init();
        // });

    }
}