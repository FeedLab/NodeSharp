using CommunityToolkit.Mvvm.Messaging;
using NodeSharp.Client.Services;
using NodeSharp.Client.ViewModel;

namespace NodeSharp.Client.Component;

public class CurvedLineDrawable : IDrawable
{
    // private readonly LineConnectionManager lineConnectionManager = AppService.GetRequiredService<LineConnectionManager>();

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        var diagramViewModel = AppService.GetRequiredService<DiagramViewModel>();
        var lineConnectionManager = AppService.GetRequiredService<LineConnectionManager>();
        
        canvas.StrokeColor = Colors.DarkRed;
        canvas.StrokeSize = 2;

        var recalculateLines = lineConnectionManager.RecalculateLines(diagramViewModel.BoxNodes);
        
        // var boxNodes = diagramViewModel.BoxNodes;
        
        var allConnections = recalculateLines.SelectMany(node => node.Connections) .ToList();

        foreach (var (start, end) in allConnections)
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