using NodeSharp.Client.ViewModel;
using NodeSharp.Nodes.Common.Services;

namespace NodeSharp.Client.Component;

public class CurvedLineDrawable : IDrawable
{
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        var diagramViewModel = AppService.GetRequiredService<DiagramViewModel>();
        var lineConnectionManager = AppService.GetRequiredService<LineConnectionManager>();

        canvas.StrokeColor = Colors.DarkRed;
        canvas.StrokeSize = 2;

        //    var linesx = lineConnectionManager.RecalculateLines(diagramViewModel.BoxNodes);
        var inputAnchorPoints = diagramViewModel.BoxNodes.SelectMany(s => s.InputNodes).ToList();
        var outputAnchorPoints = diagramViewModel.BoxNodes.SelectMany(s => s.OutputNodes).ToList();

        // var lines = outputAnchorPoints.Select(s =>
        //     new LineConnection(new Point(s.RelativePosition!.Value.X, s.RelativePosition.Value.Y),
        //         new Point(s.RelativePosition.Value.X, s.RelativePosition.Value.Y)));

        var lineConnections = new List<LineConnection>();
        
        foreach (var startAnchorPoint in outputAnchorPoints)
        {
            var fromAnchorPt = new Point(startAnchorPoint.RelativePosition!.Value.X,
                startAnchorPoint.RelativePosition.Value.Y);

            foreach (var endAnchorPoint in startAnchorPoint.ConnectsToNodeId)
            {
                var toAnchorPt = new Point(endAnchorPoint.RelativePosition!.Value.X,
                    endAnchorPoint.RelativePosition.Value.Y);
                
                lineConnections.Add(new LineConnection(fromAnchorPt, toAnchorPt));
            }
        }

        foreach (var connection in lineConnections)
        {
            DrawCurve(canvas, connection.Start, connection.End, connection.IsSelected);
        }

        // Draw temporary line while dragging
        if (lineConnectionManager.IsDragging && lineConnectionManager.DragStartAnchor != null &&
            lineConnectionManager.DragCurrentPoint != null)
        {
            canvas.StrokeColor = Colors.Blue;
            canvas.StrokeSize = 2;
            canvas.StrokeDashPattern = new float[] { 5, 5 };

            var start = new Point(lineConnectionManager.DragStartAnchor.X,
                lineConnectionManager.DragStartAnchor.Y);
            var end = lineConnectionManager.DragCurrentPoint.Value;

            var path = new PathF();
            path.MoveTo((float)start.X, (float)start.Y);

            float controlX = (float)((start.X + end.X) / 2);
            float controlY = (float)((start.Y + end.Y) / 2 - 50);

            path.QuadTo(controlX, controlY, (float)end.X, (float)end.Y);

            canvas.DrawPath(path);
            canvas.StrokeDashPattern = null;
        }
    }


    private void DrawCurve(ICanvas canvas, Point start, Point end, bool isSelected)
    {
        // Curve strength (tweakable)
        var dx = (float)Math.Clamp(Math.Abs(end.X - start.X), 40, 200);

        var c1 = new Point(start.X + dx, start.Y);
        var c2 = new Point(end.X - dx, end.Y);

        var path = new PathF();
        path.MoveTo((float)start.X, (float)start.Y);
        path.CurveTo((float)c1.X, (float)c1.Y, (float)c2.X, (float)c2.Y, (float)end.X, (float)end.Y);

        if (isSelected)
        {
            // Draw glow effect for selected curve (wider background stroke)
            canvas.StrokeColor = Color.FromRgba(255, 165, 0, 128); // Semi-transparent orange glow
            canvas.StrokeSize = 8;
            canvas.DrawPath(path);

            // Draw main selected curve on top
            canvas.StrokeColor = Colors.Orange;
            canvas.StrokeSize = 4;
            canvas.DrawPath(path);
        }
        else
        {
            // Draw normal curve
            canvas.StrokeColor = Colors.DeepSkyBlue;
            canvas.StrokeSize = 3;
            canvas.DrawPath(path);
        }
    }
}