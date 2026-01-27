using Microsoft.Extensions.Options;
using Microsoft.Maui.Graphics;
using NodeSharp.Nodes.Common.Configuration;
using NodeSharp.Nodes.Common.Services;

namespace NodeSharp.Client.Component;

public class GridBackgroundDrawable : IDrawable
{
    private GridSettings? gridSettings;

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        gridSettings ??= AppService.GetService<IOptions<GridSettings>>()?.Value ?? new GridSettings();
        var grid = gridSettings;

        canvas.SaveState();

        canvas.FillColor = ParseColor(grid.BackgroundColor, Color.FromArgb("#F8FAFC"));
        canvas.FillRectangle(dirtyRect);

        var width = dirtyRect.Width;
        var height = dirtyRect.Height;
        var minorStep = (float)Math.Max(2.0, grid.Size);
        var majorEvery = Math.Max(2, grid.MajorLineEvery);
        var majorStep = minorStep * majorEvery;

        canvas.StrokeSize = 1;
        canvas.StrokeColor = ParseColor(grid.MinorLineColor, Color.FromArgb("#E3E8EF"));
        DrawGrid(canvas, width, height, minorStep);

        canvas.StrokeSize = 1;
        canvas.StrokeColor = ParseColor(grid.MajorLineColor, Color.FromArgb("#CBD5E1"));
        DrawGrid(canvas, width, height, majorStep);

        canvas.RestoreState();
    }

    private static Color ParseColor(string? value, Color fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        try
        {
            return Color.FromArgb(value);
        }
        catch
        {
            return fallback;
        }
    }

    private static void DrawGrid(ICanvas canvas, float width, float height, float step)
    {
        for (float x = 0; x <= width; x += step)
        {
            canvas.DrawLine(x, 0, x, height);
        }

        for (float y = 0; y <= height; y += step)
        {
            canvas.DrawLine(0, y, width, y);
        }
    }
}
