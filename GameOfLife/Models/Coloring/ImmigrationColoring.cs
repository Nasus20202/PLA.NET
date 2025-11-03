using System.Windows.Media;

namespace GameOfLife.Models.Coloring;

/// <summary>
/// Immigration variant - cells have colors based on which neighbor caused them to be born
/// </summary>
public class ImmigrationColoring : IColoringModel
{
    private Dictionary<(int, int), Color> _cellColors = new();
    private Color[] _colors = new[] { Colors.Red, Colors.Yellow };

    public string Name => "Immigration";
    public string Description =>
        "Cells get different colors based on which neighbor caused them to be born";

    public Color GetCellColor(int x, int y, bool isAlive, int age, int neighbors)
    {
        if (!isAlive)
            return Colors.Black;

        var key = (x, y);
        if (_cellColors.ContainsKey(key))
            return _cellColors[key];

        return _colors[0];
    }

    public void InitializeColorsForGrid(bool[,] gridState)
    {
        // Assign random colors to all alive cells
        _cellColors.Clear();
        var random = new Random();
        int width = gridState.GetLength(0);
        int height = gridState.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (gridState[x, y])
                {
                    int colorIndex = random.Next(_colors.Length);
                    _cellColors[(x, y)] = _colors[colorIndex];
                }
            }
        }
    }

    public void OnCellsBorn(List<(int x, int y)> newCells, bool[,] currentState)
    {
        // Assign color to newly born cells based on majority color of alive neighbors
        int width = currentState.GetLength(0);
        int height = currentState.GetLength(1);

        foreach (var (x, y) in newCells)
        {
            // Count colors of alive neighbors
            var colorCounts = new Dictionary<Color, int>();

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0)
                        continue;

                    int nx = x + dx;
                    int ny = y + dy;

                    if (nx >= 0 && nx < width && ny >= 0 && ny < height && currentState[nx, ny])
                    {
                        var key = (nx, ny);
                        Color neighborColor = _cellColors.ContainsKey(key)
                            ? _cellColors[key]
                            : _colors[0];

                        if (!colorCounts.ContainsKey(neighborColor))
                            colorCounts[neighborColor] = 0;
                        colorCounts[neighborColor]++;
                    }
                }
            }

            // Find majority color
            Color cellColor = _colors[0];
            if (colorCounts.Count > 0)
            {
                var majorityColor = colorCounts.OrderByDescending(kvp => kvp.Value).First();
                cellColor = majorityColor.Key;
            }

            _cellColors[(x, y)] = cellColor;
        }
    }

    public void OnCellsDead(List<(int x, int y)> deadCells) { }

    public void NextGeneration() { }

    public void SetCellColor(int x, int y, Color color)
    {
        _cellColors[(x, y)] = color;
    }

    public void Clear()
    {
        _cellColors.Clear();
    }
}
