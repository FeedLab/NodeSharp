using NodeSharp.Client.Services;
using NodeSharp.Client.ViewModel;

namespace NodeSharp.Client.Component;

public class CurvedLineDrawable : IDrawable
{
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        var diagramViewModel = AppService.GetRequiredService<DiagramViewModel>();
        var lineConnectionManager = AppService.GetRequiredService<LineConnectionManager>();

        canvas.StrokeColor = Colors.DarkRed;
        canvas.StrokeSize = 2;

        var lines = lineConnectionManager.RecalculateLines(diagramViewModel.BoxNodes);

        foreach (var (start, end) in lines)
        {
            DrawCurve(canvas, start, end);
        }

        // Draw temporary line while dragging
        if (lineConnectionManager.IsDragging && lineConnectionManager.DragStartAnchor != null &&
            lineConnectionManager.DragCurrentPoint != null)
        {
            canvas.StrokeColor = Colors.Blue;
            canvas.StrokeSize = 2;
            canvas.StrokeDashPattern = new float[] { 5, 5 };

            var start = new Point(lineConnectionManager.DragStartAnchor.AbsoluteCenterX,
                lineConnectionManager.DragStartAnchor.AbsoluteCenterY);
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


    private void DrawCurve(ICanvas canvas, Point start, Point end)
    {
        // Curve strength (tweakable)
        var dx = (float)Math.Clamp(Math.Abs(end.X - start.X), 40, 200);

        var c1 = new Point(start.X + dx, start.Y);
        var c2 = new Point(end.X - dx, end.Y);

            var path = new PathF();
            path.MoveTo((float)start.X, (float)start.Y);
            path.CurveTo((float)c1.X, (float)c1.Y, (float)c2.X, (float)c2.Y, (float)end.X, (float)end.Y);

            canvas.StrokeColor = Colors.DeepSkyBlue;
        canvas.StrokeSize = 3;
        canvas.DrawPath(path);
    }
}