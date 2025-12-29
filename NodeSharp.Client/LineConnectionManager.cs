using NodeSharp.Client.ViewModel;
using NodeSharp.NodeEngine;
using NodeSharp.NodeEngine.Model;

namespace NodeSharp.Client;

public class LineConnectionManager(NodeIo nodeIo)
{
    private IList<(Point Start, Point End)> Connections { get; set; } = [];

    public IList<(Point Start, Point End)> RecalculateLines(IEnumerable<BoxNode> boxNodes)
    {
        Connections.Clear();

        var recalculateLines = boxNodes.ToList();
        var boxNodeDictionary = recalculateLines.ToDictionary(bn => bn.NodeId);

        foreach (var boxNodeFrom in recalculateLines)
        {
            boxNodeFrom.Connections.Clear();
            boxNodeFrom.InputNodes.Clear();

            CalculateInputNodePositions(boxNodeFrom);

            var fromPt = boxNodeFrom.PtCenter;

            foreach (var output in boxNodeFrom.Node.Outputs)
            {
                CalculateOutputNodePositions(boxNodeFrom);

                foreach (var nodeToId in output.ConnectsToNodeId)
                {

                    if (boxNodeDictionary.TryGetValue(nodeToId, out var boxNodeTo))
                    {
                        var toPt = boxNodeTo.PtCenter;
                        boxNodeFrom.Connections.Add((fromPt, toPt));
                    }
                }
            }
        }

        foreach (var anchorPoint in recalculateLines.SelectMany(s => s.OutputNodes))
        {
            var cpFrom = new Point(anchorPoint.AbsoluteX, anchorPoint.AbsoluteCenterY);

            foreach (var targetNodeId in anchorPoint.Ids)
            {
                if (boxNodeDictionary.TryGetValue(targetNodeId, out var boxNodeTo))
                {
                    var target = boxNodeTo.InputNodes[0];
                    var cpTo = new Point(target.AbsoluteCenterX, target.AbsoluteCenterY);

                    Connections.Add((cpFrom, cpTo));
                }
            }
        }

        return Connections;
    }

    private static void CalculateOutputNodePositions(BoxNode boxNode)
    {
        const double verticalMargin = 8.0;

        boxNode.OutputNodes.Clear();

        var outputs = boxNode.Node.Outputs;
        var availableHeight = boxNode.Height - verticalMargin;
        var verticalStep = availableHeight / (outputs.Count + 1);
        var yPositionDelta = (verticalMargin / 2);
        var index = 1;
        foreach (var output in outputs)
        {
            var connectedIds = output.ConnectsToNodeId.ToList();
            var yPosition = (verticalStep * index);// - yPositionDelta;

        //    var anchorPoint = new AnchorPoint(connectedIds, 0, yPosition + (verticalMargin / 2), boxNode,
            var anchorPoint = new AnchorPoint(connectedIds, 0, yPosition, boxNode, InOrOutConnection.Out);
            boxNode.OutputNodes.Add(anchorPoint);

            index++;
        }
    }

    private static void CalculateInputNodePositions(BoxNode boxNode)
    {
        const double fromVerticalMargin = 8.0;

        var inputs = boxNode.Node.Inputs;
        var verticalStep = (boxNode.Height - fromVerticalMargin) / (inputs.Count + 1);
        var yPositionDelta = (fromVerticalMargin / 2) + (verticalStep / 2);

        for (var i = 0; i < inputs.Count; i++)
        {
            var fromSquareYPos = verticalStep * (i + 1); // + (yPositionDelta);
            // var ptInputSquare = new AnchorPoint(boxNode.Node.Id, 0, fromSquareYPos + (fromVerticalMargin / 2), boxNode, InOrOutConnection.In);
            var ptInputSquare = new AnchorPoint(boxNode.Node.Id, 0, fromSquareYPos, boxNode, InOrOutConnection.In);

            boxNode.InputNodes.Add(ptInputSquare);
        }
    }
}