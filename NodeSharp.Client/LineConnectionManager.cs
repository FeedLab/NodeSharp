using NodeSharp.Client.ViewModel;
using NodeSharp.NodeEngine;

namespace NodeSharp.Client;

public class LineConnection
{
    public Point Start { get; set; }
    public Point End { get; set; }

    public bool IsSelected { get; set; }


    public LineConnection(Point from, Point fo)
    {
        Start = from;
        End = fo;
    }
}

public class LineConnectionManager(NodeIo nodeIo)
{
    public IList<LineConnection> Connections { get; set; } = [];
    // private IList<(Point Start, Point End)> Connections { get; set; } = [];

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

    public IList<LineConnection> RecalculateLines(IEnumerable<BoxNode> boxNodes)
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
                    Connections.Add(new LineConnection(cpFrom, cpTo));
                }
            }
        }

        System.Diagnostics.Debug.WriteLine($"📊 Total connections: {Connections.Count}");
        return Connections;
    }

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
            if (IsPointNearLine(clickPoint, connection.Start, connection.End, tolerance))
            {
                System.Diagnostics.Debug.WriteLine($"🎯 Line found near click point: {connection.Start} -> {connection.End}");
                return connection;
            }
        }

        System.Diagnostics.Debug.WriteLine($"❌ No line found near click point: {clickPoint}");
        return null;
    }

    /// <summary>
    /// Calculates whether a point is near a line segment within a given tolerance.
    /// Uses perpendicular distance from point to line segment.
    /// </summary>
    private static bool IsPointNearLine(Point point, Point lineStart, Point lineEnd, double tolerance)
    {
        var distance = DistanceFromPointToLineSegment(point, lineStart, lineEnd);
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