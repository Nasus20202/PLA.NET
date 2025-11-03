using System.Windows.Media;

namespace GameOfLife.Models.Coloring;

/// <summary>
/// QuadLife coloring - uses four states/colors, similar to Immigration but with 4 colors
/// </summary>
public class QuadLifeColoring : IColoringModel
{
    private Dictionary<(int, int), Color> _cellColors = new();
    private Color[] _quadColors = new[]
    {
        Color.FromRgb(255, 0, 0), // Red
        Color.FromRgb(255, 255, 0), // Yellow
        Color.FromRgb(0, 255, 0), // Green
        Color.FromRgb(0, 0, 255), // Blue
    };

    public string Name => "QuadLife";
    public string Description =>
        "Four different colors - nowy kolor na podstawie większości sąsiadów";

    public Color GetCellColor(int x, int y, bool isAlive, int age, int neighbors)
    {
        if (!isAlive)
            return Colors.Black;

        var key = (x, y);
        if (_cellColors.ContainsKey(key))
            return _cellColors[key];

        return _quadColors[0]; // Default to Red if not found
    }

    public void InitializeColorsForGrid(bool[,] gridState)
    {
        // Assign random colors to all alive cells from the 4 available colors
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
                    int colorIndex = random.Next(_quadColors.Length);
                    _cellColors[(x, y)] = _quadColors[colorIndex];
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
                            : _quadColors[0];

                        if (!colorCounts.ContainsKey(neighborColor))
                            colorCounts[neighborColor] = 0;
                        colorCounts[neighborColor]++;
                    }
                }
            }

            // Find majority color
            Color cellColor = _quadColors[0];

            if (colorCounts.Count == 1)
            {
                // Only one color among neighbors
                cellColor = colorCounts.Keys.First();
            }
            else if (colorCounts.Count == 4)
            {
                // All 4 colors present with equal count - assign the remaining/missing color
                // Find which color is NOT in the neighbors
                var neighborsWithColor = new HashSet<Color>(colorCounts.Keys);
                foreach (var color in _quadColors)
                {
                    if (!neighborsWithColor.Contains(color))
                    {
                        cellColor = color;
                        break;
                    }
                }
            }
            else if (colorCounts.Count > 0)
            {
                // Multiple colors - pick majority
                var majorityColor = colorCounts.OrderByDescending(kvp => kvp.Value).First();
                cellColor = majorityColor.Key;
            }

            _cellColors[(x, y)] = cellColor;
        }
    }

    public void OnCellsDead(List<(int x, int y)> deadCells) { }

    public void NextGeneration() { }

    public void Clear()
    {
        _cellColors.Clear();
    }
}
