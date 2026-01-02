using System.Collections.ObjectModel;
using NodeSharp.NodeEngine.Node;

namespace NodeSharp.NodeEngine;

public class BaseNodeList : ObservableCollection<BaseNode>
{
    public Dictionary<string, BaseNode> ToDictionary()
    {
        return this.ToDictionary(n => n.Id);
    }
    
    public void ValidateInputAndOutputNodes()
    {
        foreach (var node in this)
        {
            node.ValidateInputAndOutput();
        }
    }

    public Task Run()
    {
        foreach (var candidate in this)
        {
            if (!ShouldRunOnStart(candidate))
            {
                continue;
            }

            _ = candidate.Run();
        }

        return Task.CompletedTask;
    }

    private static bool ShouldRunOnStart(BaseNode candidate) =>
        candidate is { ActivateOnStart: true, IsEnabled: true };

   
    public BaseNode? Find(Func<BaseNode, bool> predicate)
    {
        return this.FirstOrDefault(predicate);
    }
}