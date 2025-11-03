using System.Windows.Media;

namespace GameOfLife.Models.Coloring;

/// <summary>
///     Entropy-based coloring - colors based on neighbor count
/// </summary>
public class EntropyColoring : IColoringModel
{
    public string Name => "Entropy";
    public string Description => "Color based on number of neighbors (0-8) - Blue to Red gradient";

    public Color GetCellColor(int x, int y, bool isAlive, int age, int neighbors)
    {
        if (!isAlive)
            return Colors.Black;

        // Map neighbor count (0-8) to color gradient from Blue to Red
        // 0 neighbors = Blue (0, 0, 255)
        // 8 neighbors = Red (255, 0, 0)
        var ratio = neighbors / 8.0; // 0.0 to 1.0

        var red = (byte)(ratio * 255); // 0 to 255
        byte green = 0;
        var blue = (byte)((1 - ratio) * 255); // 255 to 0

        return Color.FromRgb(red, green, blue);
    }

    public void NextGeneration() { }

    public void InitializeColorsForGrid(bool[,] gridState) { }

    public void OnCellsBorn(List<(int x, int y)> newCells, bool[,] currentState) { }

    public void OnCellsDead(List<(int x, int y)> deadCells) { }

    public void Clear() { }
}
