namespace ConsoleApp1;

public class BaseNodeList : List<BaseNode>
{
    public void ValidateInputAndOutputNodes()
    {
        foreach (var node in this)
        {
            node.ValidateInputAndOutput();
        }
    }

    public async Task Run()
    {
        foreach (var node in this.Where(node => node is { ActivateOnStart: true, IsEnabled: true }))
        {
            await node.Run();
        }
    }
}