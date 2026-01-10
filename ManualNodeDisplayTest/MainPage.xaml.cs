using NodeSharp.Nodes.Common;
using NodeSharp.Nodes.Common.Model;
using NodeSharp.Nodes.Inject;

namespace ManualNodeDisplayTest;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }


    private void Inject_OnClicked(object? sender, EventArgs e)
    {
        var storage = new Storage();
        var baseNodeList = new BaseNodeList();

        var nodeInject = new NodeInject(baseNodeList, "121212", nameof(NodeInject), "Test", true, true, 0, 0, storage);
        baseNodeList.Add(nodeInject);
        
        _ = nodeInject.DisplayNodeConfigurationPopup().ConfigureAwait(false);
    }
}