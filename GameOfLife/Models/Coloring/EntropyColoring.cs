using System.Windows.Media;

namespace GameOfLife.Models.Coloring;

public class EntropyColoring : IColoringModel
{
    public string Name => "Entropy";
    public string Description => "Color based on number of neighbors (0-8) - Blue to Red gradient";

    public Color GetCellColor(int x, int y, bool isAlive, int age, int neighbors)
    {
        if (!isAlive)
            return Colors.Black;

        var ratio = neighbors / 8.0;

        var red = (byte)(ratio * 255);
        const byte green = 0;
        var blue = (byte)((1 - ratio) * 255);

        return Color.FromRgb(red, green, blue);
    }

    public void NextGeneration() { }

    public void InitializeColorsForGrid(bool[,] gridState) { }

    public void OnCellsBorn(List<(int x, int y)> newCells, bool[,] currentState) { }

    public void OnCellsDead(List<(int x, int y)> deadCells) { }

    public void Clear() { }

    public List<string> Serialize()
    {
        return [];
    }

    public void Deserialize(List<string> data) { }
}
