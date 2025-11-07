using System.Windows.Media;

namespace GameOfLife.Models.Coloring;

/// <summary>
///     Rainbow coloring - based on position in grid
/// </summary>
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

    private Color HsvToRgb(double hue, double saturation, double value)
    {
        var c = value * saturation;
        var x = c * (1 - Math.Abs(hue / 60 % 2 - 1));
        var m = value - c;

        double r,
            g,
            b;

        if (hue < 60)
        {
            r = c;
            g = x;
            b = 0;
        }
        else if (hue < 120)
        {
            r = x;
            g = c;
            b = 0;
        }
        else if (hue < 180)
        {
            r = 0;
            g = c;
            b = x;
        }
        else if (hue < 240)
        {
            r = 0;
            g = x;
            b = c;
        }
        else if (hue < 300)
        {
            r = x;
            g = 0;
            b = c;
        }
        else
        {
            r = c;
            g = 0;
            b = x;
        }

        return Color.FromRgb((byte)((r + m) * 255), (byte)((g + m) * 255), (byte)((b + m) * 255));
    }
}
