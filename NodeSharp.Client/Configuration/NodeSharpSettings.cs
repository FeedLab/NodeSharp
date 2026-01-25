using System.Text.Json;
using Microsoft.Maui.Storage;

namespace NodeSharp.Client.Configuration;

public sealed class NodeSharpSettings
{
    public GridSettings Grid { get; init; } = new();

    public static NodeSharpSettings Load(string fileName)
    {
        try
        {
            using var stream = FileSystem.OpenAppPackageFileAsync(fileName).GetAwaiter().GetResult();
            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            var settings = JsonSerializer.Deserialize<NodeSharpSettings>(json);
            return settings ?? new NodeSharpSettings();
        }
        catch
        {
            return new NodeSharpSettings();
        }
    }
}

public sealed class GridSettings
{
    public double Size { get; init; } = 10.0;
    public string MinorLineColor { get; init; } = "#E3E8EF";
    public string MajorLineColor { get; init; } = "#CBD5E1";
    public string BackgroundColor { get; init; } = "#F8FAFC";
    public int MajorLineEvery { get; init; } = 5;
}
