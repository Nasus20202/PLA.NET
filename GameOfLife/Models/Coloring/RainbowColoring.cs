using System.Windows.Media;

namespace GameOfLife.Models.Coloring;

public class RainbowColoring(int gridWidth = 100, int gridHeight = 100) : IColoringModel
{
    public string Name => "Rainbow";
    public string Description => "Colors based on cell position in grid";

    public Color GetCellColor(int x, int y, bool isAlive, int age, int neighbors)
    {
        if (!isAlive)
            return Colors.Black;

        var hue = (double)(x + y) / (gridWidth + gridHeight) * 360;
        return HsvToRgb(hue, 1.0, 1.0);
    }

    public void NextGeneration() { }

    public void InitializeColorsForGrid(bool[,] gridState) { }

    public void OnCellsBorn(List<(int x, int y)> newCells, bool[,] currentState) { }

    public void OnCellsDead(List<(int x, int y)> deadCells) { }

    public void Clear() { }

    public List<string> Serialize()
    {
        return new List<string>();
    }

    public void Deserialize(List<string> data) { }

    private Color HsvToRgb(double hue, double saturation, double value)
    {
        var c = value * saturation;
        var x = c * (1 - Math.Abs(hue / 60 % 2 - 1));
        var m = value - c;

        double r,
            g,
            b;

        switch (hue)
        {
            case < 60:
                r = c;
                g = x;
                b = 0;
                break;
            case < 120:
                r = x;
                g = c;
                b = 0;
                break;
            case < 180:
                r = 0;
                g = c;
                b = x;
                break;
            case < 240:
                r = 0;
                g = x;
                b = c;
                break;
            case < 300:
                r = x;
                g = 0;
                b = c;
                break;
            default:
                r = c;
                g = 0;
                b = x;
                break;
        }

        return Color.FromRgb((byte)((r + m) * 255), (byte)((g + m) * 255), (byte)((b + m) * 255));
    }
}
