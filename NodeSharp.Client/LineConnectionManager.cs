using CommunityToolkit.Mvvm.Messaging;
using NodeSharp.Client.ViewModel;

namespace NodeSharp.Client;

public class LineConnection
{
    public AnchorPoint StartAnchor { get; }
    public AnchorPoint EndAnchor { get; }
    public Point Start { get; set; }
    public Point End { get; set; }

    public bool IsSelected { get; set; }


    public LineConnection(AnchorPoint startAnchor, AnchorPoint endAnchor, Point from, Point fo)
    {
        StartAnchor = startAnchor;
        EndAnchor = endAnchor;
        Start = from;
        End = fo;
    }
}

public class LineConnectionManager()
{
    public IList<LineConnection> Connections { get; set; } = [];
    // private IList<(Point Start, Point End)> Connections { get; set; } = [];
    
    public AnchorPoint? DragStartAnchor { get; set; }
    public AnchorPoint? DragEndAnchor { get; set; }
    
    public Point DragEndPoint { get; set; }
    public Point? DragCurrentPoint { get; set; }
    public bool IsDragging => DragStartAnchor != null;

    public void StartDragging(AnchorPoint anchor)
    {
        DragStartAnchor = anchor;
        DragCurrentPoint = new Point(anchor.X, anchor.Y);
    }

    public void UpdateDragPosition(Point point)
    {
        DragCurrentPoint = point;
    }

    public void EndDragging(AnchorPoint? targetAnchor)
    {
        if (DragStartAnchor != null && targetAnchor != null && DragStartAnchor != targetAnchor)
        {
            System.Diagnostics.Debug.WriteLine(
                $"🔗 EndDragging - Start: {DragStartAnchor.BoxNode.Node.Id}, Target: {targetAnchor.BoxNode.Node.Id}");

            DragEndAnchor = targetAnchor;
            
            // Create connection between anchors
            // Connection should go from output anchor to input anchor
            AnchorPoint outputAnchor;
            AnchorPoint inputAnchor;

            if (DragStartAnchor.ConnectionType == InOrOutConnection.In)
            {
                inputAnchor = DragStartAnchor;
                outputAnchor = DragEndAnchor;
            }
            else if (DragStartAnchor.ConnectionType == InOrOutConnection.Out)
            {
                outputAnchor = DragStartAnchor;
                inputAnchor = DragEndAnchor;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("⚠️ Could not determine anchor types");
                DragStartAnchor = null;
                DragCurrentPoint = null;
                return;
            }

            inputAnchor.ConnectsToNode(outputAnchor.Id);
            outputAnchor.ConnectsToNode(inputAnchor.Id);

            WeakReferenceMessenger.Default.Send(new RebuildAnchorPointStatus { IsAnchorAdded = true });
            
            // inputAnchor.OriginalInput.ConnectsToParentNodeId.Add(outputAnchor.Id);
            // outputAnchor.OriginalOutput.ConnectsToNodeId.Add(inputAnchor.Id);
            // inputAnchor.ConnectsToNodeId.Add(outputAnchor);
            // outputAnchor.ConnectsToNodeId.Add(inputAnchor);

            // Get the source and target nodes
            // var sourceNode = outputAnchor.BoxNode.Node;
            // var targetNodeId = inputAnchor.BoxNode.Node.Id;
            //
            // outputAnchor.AddConnection(inputAnchor);

            // Find the output that corresponds to this anchor and add the connection
            // var anchorIndex = outputAnchor.BoxNode.OutputNodes.IndexOf(outputAnchor);
            // if (anchorIndex >= 0 && anchorIndex < sourceNode.Outputs.Count)
            // {
            //     var output = sourceNode.Outputs[anchorIndex];
            //     if (!output.ConnectsToNodeId.Contains(targetNodeId))
            //     {
            //         output.ConnectsToNodeId.Add(targetNodeId);
            //         // outputAnchor.OutputConnection.ConnectsToNodeId.Add();
            //
            //         // // Also add to the anchor's Ids list so RecalculateLines can find it
            //         // if (!boxNodeOutput.OutputNodes.SingleOrDefault(s => s.).Contains(targetNodeId))
            //         // {
            //         //     outputAnchor.BoxNode.OutputNodes.Add(new AnchorPoint(targetNodeId));
            //         // }
            //
            //         System.Diagnostics.Debug.WriteLine(
            //             $"✅ Connection created: {sourceNode.Id} (anchor {anchorIndex}) -> {targetNodeId}");
            //     }
            //     else
            //     {
            //         System.Diagnostics.Debug.WriteLine(
            //             $"⚠️ Connection already exists: {sourceNode.Id} -> {targetNodeId}");
            //     }
            // }
            // else
            // {
            //     System.Diagnostics.Debug.WriteLine(
            //         $"⚠️ Invalid anchor index: {anchorIndex} (Outputs count: {sourceNode.Outputs.Count})");
            // }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine(
                $"🔗 EndDragging - No connection (Start: {DragStartAnchor?.BoxNode.Node.Id}, Target: {targetAnchor?.BoxNode.Node.Id})");
        }

        DragStartAnchor = null;
        DragCurrentPoint = null;
    }
    
    public void RebuildAnchorPointConnections(IList<BoxNode> boxNodes)
    {
        foreach (var boxNode in boxNodes)
        {
            boxNode.RebuildAnchorPointConnections(boxNodes);
        }
    }

    // public IList<LineConnection> RecalculateLines(IEnumerable<BoxNode> boxNodes)
    // {
    //     Connections.Clear();
    //     System.Diagnostics.Debug.WriteLine($"📊 RecalculateLines - Processing {boxNodes.Count()} nodes");
    //
    //     var recalculateLines = boxNodes.ToList();
    //     var boxNodeDictionary = recalculateLines.ToDictionary(bn => bn.NodeId);
    //
    //     foreach (var boxNodeFrom in recalculateLines)
    //     {
    //         // boxNodeFrom.Connections.Clear();
    //         // boxNodeFrom.InputNodes.Clear();
    //
    //         // CalculateInputNodePositions(boxNodeFrom);
    //
    //         var fromPt = boxNodeFrom.;
    //
    //         foreach (var output in boxNodeFrom.Node.Outputs)
    //         {
    //             // CalculateOutputNodePositions(boxNodeFrom);
    //
    //             foreach (var nodeToId in output.ConnectsToNodeId)
    //             {
    //                 if (boxNodeDictionary.TryGetValue(nodeToId, out var boxNodeTo))
    //                 {
    //                     var toPt = boxNodeTo.PtCenter;
    //                     boxNodeFrom.Connections.Add((fromPt, toPt));
    //                 }
    //             }
    //         }
    //     }

    //     foreach (var boxNode in recalculateLines)
    //     {
    //         System.Diagnostics.Debug.WriteLine($"  Node {boxNode.NodeId}: {boxNode.Node.Outputs.Count} outputs");
    //         foreach (var output in boxNode.Node.Outputs)
    //         {
    //             System.Diagnostics.Debug.WriteLine(
    //                 $"    Output connects to: {string.Join(", ", output.ConnectsToNodeId)}");
    //         }
    //     }
    //
    //     foreach (var anchorPoint in recalculateLines.SelectMany(s => s.OutputNodes))
    //     {
    //         var cpFrom = new Point(anchorPoint.AbsoluteCenterX, anchorPoint.AbsoluteCenterY);
    //
    //         foreach (var targetNodeId in anchorPoint.Ids)
    //         {
    //             if (boxNodeDictionary.TryGetValue(targetNodeId, out var boxNodeTo))
    //             {
    //                 var target = boxNodeTo.InputNodes[0];
    //                 var cpTo = new Point(target.AbsoluteCenterX, target.AbsoluteCenterY);
    //
    //                 System.Diagnostics.Debug.WriteLine(
    //                     $"📍 Adding connection: {anchorPoint.BoxNode.NodeId} -> {targetNodeId}");
    //                 Connections.Add(new LineConnection(cpFrom, cpTo));
    //             }
    //         }
    //     }
    //
    //     System.Diagnostics.Debug.WriteLine($"📊 Total connections: {Connections.Count}");
    //     return Connections;
    // }

    /// <summary>
    /// Finds a connection line near the specified point within a tolerance threshold.
    /// </summary>
    /// <param name="clickPoint">The point where the user clicked</param>
    /// <param name="tolerance">Maximum distance in pixels to consider "near" (default: 10)</param>
    /// <returns>The connection line if found, otherwise null</returns>
    public LineConnection? FindLineAtPoint(Point clickPoint, double tolerance = 10.0)
    {
        foreach (var connection in Connections)
        {
            if (IsPointNearCurve(clickPoint, connection.Start, connection.End, tolerance))
            {
                System.Diagnostics.Debug.WriteLine(
                    $"🎯 Line found near click point: {connection.Start} -> {connection.End}");
                return connection;
            }
        }

        System.Diagnostics.Debug.WriteLine($"❌ No line found near click point: {clickPoint}");
        return null;
    }

    /// <summary>
    /// Calculates whether a point is near the drawn curve within a given tolerance.
    /// Uses sampled distance to the cubic Bezier segments.
    /// </summary>
    private static bool IsPointNearCurve(Point point, Point start, Point end, double tolerance)
    {
        GetBezierControlPoints(start, end, out var c1, out var c2);
        var distance = DistanceFromPointToBezier(point, start, c1, c2, end);
        return distance <= tolerance;
    }

    /// <summary>
    /// Calculates the minimum distance from a point to a line segment.
    /// </summary>
    private static double DistanceFromPointToLineSegment(Point point, Point lineStart, Point lineEnd)
    {
        // Vector from lineStart to lineEnd
        var dx = lineEnd.X - lineStart.X;
        var dy = lineEnd.Y - lineStart.Y;

        // Handle degenerate case where start and end are the same point
        if (Math.Abs(dx) < 0.001 && Math.Abs(dy) < 0.001)
        {
            return Distance(point, lineStart);
        }

        // Calculate the parameter t that represents the projection of the point onto the line
        // t = 0 means the projection is at lineStart, t = 1 means it's at lineEnd
        var t = ((point.X - lineStart.X) * dx + (point.Y - lineStart.Y) * dy) / (dx * dx + dy * dy);

        // Clamp t to [0, 1] to stay within the line segment
        t = Math.Max(0, Math.Min(1, t));

        // Find the closest point on the line segment
        var closestX = lineStart.X + t * dx;
        var closestY = lineStart.Y + t * dy;
        var closestPoint = new Point(closestX, closestY);

        // Return the distance from the point to the closest point on the segment
        return Distance(point, closestPoint);
    }

    private static double DistanceFromPointToBezier(Point point, Point p0, Point p1, Point p2, Point p3)
    {
        const int samples = 30;
        var previous = p0;
        var min = double.MaxValue;

        for (var i = 1; i <= samples; i++)
        {
            var t = i / (double)samples;
            var current = BezierPoint(p0, p1, p2, p3, t);
            var distance = DistanceFromPointToLineSegment(point, previous, current);
            if (distance < min)
            {
                min = distance;
            }

            previous = current;
        }

        return min;
    }

    private static Point BezierPoint(Point p0, Point p1, Point p2, Point p3, double t)
    {
        var u = 1 - t;
        var tt = t * t;
        var uu = u * u;
        var uuu = uu * u;
        var ttt = tt * t;

        var x = (uuu * p0.X) + (3 * uu * t * p1.X) + (3 * u * tt * p2.X) + (ttt * p3.X);
        var y = (uuu * p0.Y) + (3 * uu * t * p1.Y) + (3 * u * tt * p2.Y) + (ttt * p3.Y);
        return new Point(x, y);
    }

    private static void GetBezierControlPoints(Point start, Point end, out Point c1, out Point c2)
    {
        var dx = Math.Clamp(Math.Abs(end.X - start.X), 40, 200);
        c1 = new Point(start.X + dx, start.Y);
        c2 = new Point(end.X - dx, end.Y);
    }

    /// <summary>
    /// Calculates the Euclidean distance between two points.
    /// </summary>
    private static double Distance(Point p1, Point p2)
    {
        var dx = p2.X - p1.X;
        var dy = p2.Y - p1.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    public LineConnection? SelectLineAtPoint(Point position)
    {
        var lineAtPoint = FindLineAtPoint(position, tolerance: 15.0);

        if (lineAtPoint is null)
        {
            return null;
        }

        foreach (var connection in Connections)
        {
            connection.IsSelected = false;
        }

        lineAtPoint.IsSelected = true;

        return lineAtPoint;
    }

    public bool RemoveSelectedConnection()
    {
        var selected = Connections.FirstOrDefault(c => c.IsSelected);
        if (selected is null)
        {
            return false;
        }

        var startOutput = selected.StartAnchor.OriginalOutput;
        var endInput = selected.EndAnchor.OriginalInput;

        if (startOutput is not null && endInput is not null)
        {
            startOutput.ConnectsToNodeId.Remove(endInput.Id.ToString());
            endInput.ConnectsToParentNodeId.Remove(startOutput.Id.ToString());
            selected.StartAnchor.ConnectsToNodeId.Remove(selected.EndAnchor);
        }
        else
        {
            var startInput = selected.StartAnchor.OriginalInput;
            var endOutput = selected.EndAnchor.OriginalOutput;

            if (startInput is not null && endOutput is not null)
            {
                endOutput.ConnectsToNodeId.Remove(startInput.Id.ToString());
                startInput.ConnectsToParentNodeId.Remove(endOutput.Id.ToString());
                selected.EndAnchor.ConnectsToNodeId.Remove(selected.StartAnchor);
            }
        }

        selected.IsSelected = false;
        return true;
    }

    /// <summary>
    /// Cancels any line selection and clears drag state.
    /// </summary>
    public bool CancelSelection()
    {
        var hasStartAnchor = DragStartAnchor != null;

        // Deselect all lines
        foreach (var connection in Connections)
        {
            connection.IsSelected = false;
        }

        // Cancel any ongoing drag operation
        DragStartAnchor = null;
        DragCurrentPoint = null;

        System.Diagnostics.Debug.WriteLine("❌ Line selection and dragging canceled");

        return hasStartAnchor;
    }
}
