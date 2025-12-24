using Microsoft.Extensions.Logging;

namespace NodeSharp.Client.ViewModel;

public class MainPageModel
{
    private readonly NodeToolListComponentModel nodeToolListComponentModel;

    public MainPageModel(ILogger<MainPageModel> logger,  NodeToolListComponentModel nodeToolListComponentModel)
    {
        this.nodeToolListComponentModel = nodeToolListComponentModel;
    }

    public void Init()
    {
        nodeToolListComponentModel.Init();
    }
}