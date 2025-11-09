using System.Windows.Media;

namespace GameOfLife.Models.Coloring;

public class QuadLifeColoring : IColoringModel
{
    private readonly Dictionary<(int, int), Color> _cellColors = new();

    private readonly Color[] _quadColors = new[]
    {
        Color.FromRgb(255, 0, 0), // Red
        Color.FromRgb(255, 255, 0), // Yellow
        Color.FromRgb(0, 255, 0), // Green
        Color.FromRgb(0, 0, 255), // Blue
    };

    public string Name => "QuadLife";

    public string Description =>
        "Four different colors - new color based on the majority of neighbors";

    public Color GetCellColor(int x, int y, bool isAlive, int age, int neighbors)
    {
        if (!isAlive)
            return Colors.Black;

        var key = (x, y);
        return _cellColors.TryGetValue(key, out var color) ? color : _quadColors[0]; // Default to Red if not found
    }

    public void InitializeColorsForGrid(bool[,] gridState)
    {
        _cellColors.Clear();
        var random = new Random();
        var width = gridState.GetLength(0);
        var height = gridState.GetLength(1);

        for (var x = 0; x < width; x++)
        for (var y = 0; y < height; y++)
            if (gridState[x, y])
            {
                var colorIndex = random.Next(_quadColors.Length);
                _cellColors[(x, y)] = _quadColors[colorIndex];
            }
    }

    public void OnCellsBorn(List<(int x, int y)> newCells, bool[,] currentState)
    {
        var width = currentState.GetLength(0);
        var height = currentState.GetLength(1);

        foreach (var (x, y) in newCells)
        {
            var colorCounts = new Dictionary<Color, int>();

            for (var dx = -1; dx <= 1; dx++)
            for (var dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0)
                    continue;

                var nx = x + dx;
                var ny = y + dy;

                if (nx < 0 || nx >= width || ny < 0 || ny >= height || !currentState[nx, ny])
                    continue;
                var key = (nx, ny);
                var neighborColor = _cellColors.TryGetValue(key, out var color)
                    ? color
                    : _quadColors[0];

                colorCounts.TryAdd(neighborColor, 0);
                colorCounts[neighborColor]++;
            }

            var cellColor = _quadColors[0];

            switch (colorCounts.Count)
            {
                case 1:
                    cellColor = colorCounts.Keys.First();
                    break;
                case 4:
                {
                    var neighborsWithColor = new HashSet<Color>(colorCounts.Keys);
                    foreach (var color in _quadColors)
                        if (!neighborsWithColor.Contains(color))
                        {
                            cellColor = color;
                            break;
                        }

                    break;
                }
                case > 0:
                {
                    var majorityColor = colorCounts.OrderByDescending(kvp => kvp.Value).First();
                    cellColor = majorityColor.Key;
                    break;
                }
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

    public List<string> Serialize()
    {
        return (
            from kvp in _cellColors
            let color = kvp.Value
            select $"{kvp.Key.Item1},{kvp.Key.Item2}:{color.R},{color.G},{color.B}"
        ).ToList();
    }

    public void Deserialize(List<string> data)
    {
        _cellColors.Clear();
        foreach (var line in data)
        {
            var parts = line.Split(':');
            if (parts.Length != 2)
                continue;
            var coords = parts[0].Split(',');
            var rgb = parts[1].Split(',');
            if (coords.Length != 2 || rgb.Length != 3)
                continue;
            var x = int.Parse(coords[0]);
            var y = int.Parse(coords[1]);
            var r = byte.Parse(rgb[0]);
            var g = byte.Parse(rgb[1]);
            var b = byte.Parse(rgb[2]);
            _cellColors[(x, y)] = Color.FromRgb(r, g, b);
        }
    }
}
