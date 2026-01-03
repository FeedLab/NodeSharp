using NodeSharp.Client.ViewModel;
using NodeSharp.NodeEngine;
using NodeSharp.NodeEngine.Model;

namespace NodeSharp.Client;

public class LineConnectionManager(NodeIo nodeIo)
{
    private IList<(Point Start, Point End)> Connections { get; set; } = [];

    public AnchorPoint? DragStartAnchor { get; set; }
    public Point? DragCurrentPoint { get; set; }
    public bool IsDragging => DragStartAnchor != null;

    public void StartDragging(AnchorPoint anchor)
    {
        DragStartAnchor = anchor;
        DragCurrentPoint = new Point(anchor.AbsoluteCenterX, anchor.AbsoluteCenterY);
    }

    public void UpdateDragPosition(Point point)
    {
        DragCurrentPoint = point;
    }

    public void EndDragging(AnchorPoint? targetAnchor)
    {
        if (DragStartAnchor != null && targetAnchor != null && DragStartAnchor != targetAnchor)
        {
            System.Diagnostics.Debug.WriteLine($"🔗 EndDragging - Start: {DragStartAnchor.BoxNode.NodeId}, Target: {targetAnchor.BoxNode.NodeId}");

            // Create connection between anchors
            // Connection should go from output anchor to input anchor
            AnchorPoint outputAnchor;
            AnchorPoint inputAnchor;

            // Determine which anchor is output and which is input based on their lists
            if (DragStartAnchor.BoxNode.OutputNodes.Contains(DragStartAnchor))
            {
                // Started from output, target should be input
                outputAnchor = DragStartAnchor;
                inputAnchor = targetAnchor;
                System.Diagnostics.Debug.WriteLine("🔗 Direction: Output -> Input");
            }
            else if (targetAnchor.BoxNode.OutputNodes.Contains(targetAnchor))
            {
                // Started from input, target is output (reverse)
                outputAnchor = targetAnchor;
                inputAnchor = DragStartAnchor;
                System.Diagnostics.Debug.WriteLine("🔗 Direction: Input <- Output (reversed)");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("⚠️ Could not determine anchor types");
                DragStartAnchor = null;
                DragCurrentPoint = null;
                return;
            }

            // Get the source and target nodes
            var sourceNode = outputAnchor.BoxNode.Node;
            var targetNodeId = inputAnchor.BoxNode.NodeId;

            // Find the output that corresponds to this anchor and add the connection
            var anchorIndex = outputAnchor.BoxNode.OutputNodes.IndexOf(outputAnchor);
            if (anchorIndex >= 0 && anchorIndex < sourceNode.Outputs.Count)
            {
                var output = sourceNode.Outputs[anchorIndex];
                if (!output.ConnectsToNodeId.Contains(targetNodeId))
                {
                    output.ConnectsToNodeId.Add(targetNodeId);

                    // Also add to the anchor's Ids list so RecalculateLines can find it
                    if (!outputAnchor.Ids.Contains(targetNodeId))
                    {
                        outputAnchor.Ids.Add(targetNodeId);
                    }

                    System.Diagnostics.Debug.WriteLine($"✅ Connection created: {sourceNode.Id} (anchor {anchorIndex}) -> {targetNodeId}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ Connection already exists: {sourceNode.Id} -> {targetNodeId}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Invalid anchor index: {anchorIndex} (Outputs count: {sourceNode.Outputs.Count})");
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"🔗 EndDragging - No connection (Start: {DragStartAnchor?.BoxNode.NodeId}, Target: {targetAnchor?.BoxNode.NodeId})");
        }

        DragStartAnchor = null;
        DragCurrentPoint = null;
    }

    public IList<(Point Start, Point End)> RecalculateLines(IEnumerable<BoxNode> boxNodes)
    {
        Connections.Clear();
        System.Diagnostics.Debug.WriteLine($"📊 RecalculateLines - Processing {boxNodes.Count()} nodes");

        var recalculateLines = boxNodes.ToList();
        var boxNodeDictionary = recalculateLines.ToDictionary(bn => bn.NodeId);

        foreach (var boxNodeFrom in recalculateLines)
        {
            // boxNodeFrom.Connections.Clear();
            // boxNodeFrom.InputNodes.Clear();

            // CalculateInputNodePositions(boxNodeFrom);

            var fromPt = boxNodeFrom.PtCenter;

            foreach (var output in boxNodeFrom.Node.Outputs)
            {
                // CalculateOutputNodePositions(boxNodeFrom);

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

        foreach (var boxNode in recalculateLines)
        {
            System.Diagnostics.Debug.WriteLine($"  Node {boxNode.NodeId}: {boxNode.Node.Outputs.Count} outputs");
            foreach (var output in boxNode.Node.Outputs)
            {
                System.Diagnostics.Debug.WriteLine($"    Output connects to: {string.Join(", ", output.ConnectsToNodeId)}");
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

                    System.Diagnostics.Debug.WriteLine($"📍 Adding connection: {anchorPoint.BoxNode.NodeId} -> {targetNodeId}");
                    Connections.Add((cpFrom, cpTo));
                }
            }
        }

        System.Diagnostics.Debug.WriteLine($"📊 Total connections: {Connections.Count}");
        return Connections;
    }

    // private static void CalculateOutputNodePositions(BoxNode boxNode)
    // {
    //     const double verticalMargin = 8.0;
    //
    //     boxNode.OutputNodes.Clear();
    //
    //     var outputs = boxNode.Node.Outputs;
    //     var availableHeight = boxNode.Height - verticalMargin;
    //     var verticalStep = availableHeight / (outputs.Count + 1);
    //     var yPositionDelta = (verticalMargin / 2);
    //     var index = 1;
    //     foreach (var output in outputs)
    //     {
    //         var connectedIds = output.ConnectsToNodeId.ToList();
    //         var yPosition = (verticalStep * index);// - yPositionDelta;
    //
    //     //    var anchorPoint = new AnchorPoint(connectedIds, 0, yPosition + (verticalMargin / 2), boxNode,
    //         var anchorPoint = new AnchorPoint(connectedIds, 0, yPosition, boxNode, InOrOutConnection.Out);
    //         boxNode.OutputNodes.Add(anchorPoint);
    //
    //         index++;
    //     }
    // }
    //
    // private static void CalculateInputNodePositions(BoxNode boxNode)
    // {
    //     const double fromVerticalMargin = 8.0;
    //
    //     var inputs = boxNode.Node.Inputs;
    //     var verticalStep = (boxNode.Height - fromVerticalMargin) / (inputs.Count + 1);
    //     var yPositionDelta = (fromVerticalMargin / 2) + (verticalStep / 2);
    //
    //     for (var i = 0; i < inputs.Count; i++)
    //     {
    //         var fromSquareYPos = verticalStep * (i + 1); // + (yPositionDelta);
    //         // var ptInputSquare = new AnchorPoint(boxNode.Node.Id, 0, fromSquareYPos + (fromVerticalMargin / 2), boxNode, InOrOutConnection.In);
    //         var ptInputSquare = new AnchorPoint(boxNode.Node.Id, 0, fromSquareYPos, boxNode, InOrOutConnection.In);
    //
    //         boxNode.InputNodes.Add(ptInputSquare);
    //     }
    // }
}