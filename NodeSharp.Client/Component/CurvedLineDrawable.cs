using CommunityToolkit.Mvvm.Messaging;

namespace NodeSharp.Client.Component;

public class CurvedLineDrawable : IDrawable
{
    public CurvedLineDrawable()
    {
        Connections.Add((new Point(100, 300), new Point(700, 700)));
        Connections.Add((new Point(200, 200), new Point(800, 800)));
        Connections.Add((new Point(300, 100), new Point(900, 900)));
    }

    private IList<(Point Start, Point End)> Connections { get; set; } = [];

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.StrokeColor = Colors.DarkRed;
        canvas.StrokeSize = 2;

        foreach (var (start, end) in Connections)
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