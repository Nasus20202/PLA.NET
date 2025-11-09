using System.IO;
using System.Text;

namespace GameOfLife.Models;

public class GameState(int width, int height, bool[,] cells, GameRules rules, long generation = 0)
{
    public int Width { get; set; } = width;
    public int Height { get; set; } = height;
    public bool[,] Cells { get; set; } = cells;
    public GameRules Rules { get; set; } = rules;
    private long Generation { get; set; } = generation;

    public string ColoringModelName { get; set; } = "Standard";
    public List<string> ColoringData { get; set; } = new();

    public void SaveToFile(string filePath)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Game of Life State");
        sb.AppendLine($"# Saved: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Width: {Width}");
        sb.AppendLine($"Height: {Height}");
        sb.AppendLine($"Rules: {Rules}");
        sb.AppendLine($"Generation: {Generation}");
        sb.AppendLine($"ColoringModel: {ColoringModelName}");

        // Save coloring data
        if (ColoringData.Count > 0)
        {
            sb.AppendLine("# Coloring Data:");
            foreach (var line in ColoringData)
            {
                sb.AppendLine($"ColorData: {line}");
            }
        }

        sb.AppendLine("# Grid (O = alive, . = dead):");

        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
                sb.Append(Cells[x, y] ? 'O' : '.');
            sb.AppendLine();
        }

        File.WriteAllText(filePath, sb.ToString());
    }

    public static GameState LoadFromFile(string filePath)
    {
        var lines = File.ReadAllLines(filePath);
        int width = 0,
            height = 0;
        long generation = 0;
        var rules = GameRules.ConwayDefault();
        var coloringModelName = "Standard";
        var coloringData = new List<string>();

        var gridStartLine = 0;

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();

            if (line.StartsWith('#') || string.IsNullOrWhiteSpace(line))
                continue;

            if (line.StartsWith("Width:", StringComparison.OrdinalIgnoreCase))
            {
                width = int.Parse(line[6..].Trim());
            }
            else if (line.StartsWith("Height:", StringComparison.OrdinalIgnoreCase))
            {
                height = int.Parse(line[7..].Trim());
            }
            else if (line.StartsWith("Rules:", StringComparison.OrdinalIgnoreCase))
            {
                var ruleStr = line[6..].Trim();
                rules = GameRules.Parse(ruleStr);
            }
            else if (line.StartsWith("Generation:", StringComparison.OrdinalIgnoreCase))
            {
                generation = long.Parse(line[11..].Trim());
            }
            else if (line.StartsWith("ColoringModel:", StringComparison.OrdinalIgnoreCase))
            {
                coloringModelName = line[14..].Trim();
            }
            else if (line.StartsWith("ColorData:", StringComparison.OrdinalIgnoreCase))
            {
                coloringData.Add(line[10..].Trim());
            }
            else if (line.Contains('O') || line.Contains('.'))
            {
                gridStartLine = i;
                break;
            }
        }

        if (width == 0 || height == 0)
            throw new InvalidDataException("Invalid file format: missing width or height");

        var cells = new bool[width, height];
        var y = 0;

        for (var i = gridStartLine; i < lines.Length && y < height; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            for (var x = 0; x < Math.Min(width, line.Length); x++)
                cells[x, y] = line[x] == 'O' || line[x] == 'o';
            y++;
        }

        var gameState = new GameState(width, height, cells, rules, generation)
        {
            ColoringModelName = coloringModelName,
            ColoringData = coloringData,
        };

        return gameState;
    }
}
