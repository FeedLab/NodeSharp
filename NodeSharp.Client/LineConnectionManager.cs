using NodeSharp.Client.ViewModel;
using NodeSharp.NodeEngine;
using NodeSharp.NodeEngine.Model;

namespace NodeSharp.Client;

public class LineConnectionManager(NodeIo nodeIo)
{
    public IList<(Point Start, Point End)> Connections { get; set; } = [];

    public IEnumerable<BoxNode> RecalculateLines(IEnumerable<BoxNode> boxNodes)
    {
        var nodeDictionary = nodeIo.Nodes.ToDictionary();

        var recalculateLines = boxNodes.ToList();
        
        foreach (var boxNode in recalculateLines)
        {
            boxNode.Connections.Clear();

            var fromNode = boxNode.Node;
            var fromPt = new Point(fromNode.X, fromNode.Y);

            foreach (var output in fromNode.Outputs)
            {
                foreach (var nodeToId in output.ConnectsToNodeId)
                {
                    var toNode = nodeDictionary[nodeToId];

                    var toPt = new Point(toNode.X, toNode.Y);

                    boxNode.Connections.Add((fromPt, toPt));
                }
            }
        }
        
        return recalculateLines;
    }

    public void RecalculateLinesx()
    {
        Connections.Clear();

        var nodes = nodeIo.Nodes.ToDictionary();

        foreach (var node in nodes.Values)
        {
            //   var nodeHeight = node.wi + 50;
            var from = new Point(node.X, node.Y);

            foreach (var output in node.Outputs)
            {
                foreach (var nodeId in output.ConnectsToNodeId)
                {
                    var toNode = nodes[nodeId];

                    var to = new Point(toNode.X, toNode.Y);

                    Connections.Add((from, to));
                }
            }
        }
    }
}