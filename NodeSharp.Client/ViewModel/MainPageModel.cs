using Microsoft.Extensions.Logging;
using NodeSharp.NodeEngine;

namespace NodeSharp.Client.ViewModel;

public class MainPageModel
{
    private readonly NodeToolListModel nodeToolListModel;
    private readonly Main main;
    private readonly DiagramViewModel diagramViewModelModel;

    public MainPageModel(ILogger<MainPageModel> logger,  NodeToolListModel nodeToolListModel, Main main, DiagramViewModel diagramViewModelModel)
    {
        this.main = main;
        this.diagramViewModelModel = diagramViewModelModel;
        this.nodeToolListModel = nodeToolListModel;
    }

    public void Init()
    {
        nodeToolListModel.Init();
        
        // Task.Run(async () =>
        // {
        //     await diagramViewModelModel.Init();
        // });

    }
}