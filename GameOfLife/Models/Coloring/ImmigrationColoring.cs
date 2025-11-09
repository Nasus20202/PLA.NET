using System.Windows.Media;

namespace GameOfLife.Models.Coloring;

public class ImmigrationColoring : IColoringModel
{
    private readonly Dictionary<(int, int), Color> _cellColors = new();
    private readonly Color[] _colors = [Colors.Red, Colors.Yellow];

    public string Name => "Immigration";

    public string Description =>
        "Cells get different colors based on which neighbor caused them to be born";

    public Color GetCellColor(int x, int y, bool isAlive, int age, int neighbors)
    {
        if (!isAlive)
            return Colors.Black;

        var key = (x, y);
        return _cellColors.TryGetValue(key, out var color) ? color : _colors[0];
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
                var colorIndex = random.Next(_colors.Length);
                _cellColors[(x, y)] = _colors[colorIndex];
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
                    : _colors[0];

                colorCounts.TryAdd(neighborColor, 0);
                colorCounts[neighborColor]++;
            }

            var cellColor = _colors[0];
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

    public void Clear()
    {
        _cellColors.Clear();
    }

    public void SetCellColor(int x, int y, Color color)
    {
        _cellColors[(x, y)] = color;
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
