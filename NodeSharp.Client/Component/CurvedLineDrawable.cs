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
            var path = new PathF();
            path.MoveTo((float)start.X, (float)start.Y);

            float controlX = (float)((start.X + end.X) / 2);
            float controlY = (float)((start.Y + end.Y) / 2 - 50);

            path.QuadTo(controlX, controlY, (float)end.X, (float)end.Y);

            canvas.DrawPath(path);
        }
    }
}