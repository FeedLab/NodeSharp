using NodeSharp.NodeEngine.Node;

namespace NodeSharp.NodeEngine;

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
        foreach (var candidate in this)
        {
            if (!ShouldRunOnStart(candidate))
            {
                continue;
            }

            await candidate.Run().ConfigureAwait(false);
        }
    }

    private static bool ShouldRunOnStart(BaseNode candidate) =>
        candidate is { ActivateOnStart: true, IsEnabled: true };
}