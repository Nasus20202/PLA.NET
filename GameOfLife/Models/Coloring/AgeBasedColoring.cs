using System.Windows.Media;

namespace GameOfLife.Models.Coloring;

/// <summary>
///     Age-based coloring - cells have different colors based on their age
/// </summary>
public class AgeBasedColoring : IColoringModel
{
    private readonly Dictionary<(int, int), int> _cellAge = new();

    public string Name => "Age-Based";
    public string Description => "Colors vary based on cell age (younger = brighter)";

    public Color GetCellColor(int x, int y, bool isAlive, int age, int neighbors)
    {
        if (!isAlive)
            return Colors.Black;

        var key = (x, y);
        if (_cellAge.ContainsKey(key))
        {
            var cellAge = _cellAge[key];
            // Color transitions from bright to dark based on age
            var brightness = (byte)Math.Max(50, 255 - cellAge * 5);
            return Color.FromRgb(brightness, brightness, brightness);
        }

        return Colors.White;
    }

    public void InitializeColorsForGrid(bool[,] gridState)
    {
        // Initialize all alive cells with age 0
        _cellAge.Clear();
        var width = gridState.GetLength(0);
        var height = gridState.GetLength(1);

        for (var x = 0; x < width; x++)
        for (var y = 0; y < height; y++)
            if (gridState[x, y])
                _cellAge[(x, y)] = 0;
    }

    public void OnCellsBorn(List<(int x, int y)> newCells, bool[,] currentState)
    {
        // Set age 0 for newly born cells
        foreach (var (x, y) in newCells)
            _cellAge[(x, y)] = 0;
    }

    public void OnCellsDead(List<(int x, int y)> deadCells)
    {
        // Remove dead cells from age tracking
        foreach (var (x, y) in deadCells)
        {
            var key = (x, y);
            if (_cellAge.ContainsKey(key))
                _cellAge.Remove(key);
        }
    }

    public void NextGeneration()
    {
        // Increment age of all living cells
        foreach (var key in _cellAge.Keys.ToList())
            _cellAge[key]++;
    }

    public void Clear()
    {
        _cellAge.Clear();
    }

    public void SetCellAge(int x, int y, int age)
    {
        _cellAge[(x, y)] = age;
    }
}
