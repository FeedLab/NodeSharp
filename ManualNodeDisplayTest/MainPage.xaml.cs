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
        // var storage = new Storage();
        // var baseNodeList = new BaseNodeList();
        //
        // var nodeInject = new NodeInject(baseNodeList, "121212", "Inject", "Test", true, true, 0, 0, storage);
        // nodeInject.Parameters.Add(new Parameter("Any number", "Number", "Primitive", "128"));
        // nodeInject.Parameters.Add(new Parameter("Any string", "string", "Primitive", "Node Sharp"));
        // nodeInject.Parameters.Add(new Parameter("Any boolean", "Boolean", "Primitive", "False"));
        // baseNodeList.Add(nodeInject);
        //
        // _ = nodeInject.DisplayNodeConfigurationPopup().ConfigureAwait(false);
    }
}