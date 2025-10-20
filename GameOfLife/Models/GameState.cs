using System;
using System.IO;
using System.Text;

namespace GameOfLife.Models;

/// <summary>
/// Represents a complete game state for saving/loading
/// </summary>
public class GameState
{
    public int Width { get; set; }
    public int Height { get; set; }
    public bool[,] Cells { get; set; }
    public GameRules Rules { get; set; }
    public long Generation { get; set; }

    public GameState(int width, int height, bool[,] cells, GameRules rules, long generation = 0)
    {
        Width = width;
        Height = height;
        Cells = cells;
        Rules = rules;
        Generation = generation;
    }

    public void SaveToFile(string filePath)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# Game of Life State");
        sb.AppendLine($"# Saved: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Width: {Width}");
        sb.AppendLine($"Height: {Height}");
        sb.AppendLine($"Rules: {Rules}");
        sb.AppendLine($"Generation: {Generation}");
        sb.AppendLine("# Grid (O = alive, . = dead):");
        
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                sb.Append(Cells[x, y] ? 'O' : '.');
            }
            sb.AppendLine();
        }

        File.WriteAllText(filePath, sb.ToString());
    }

    public static GameState LoadFromFile(string filePath)
    {
        var lines = File.ReadAllLines(filePath);
        int width = 0, height = 0;
        long generation = 0;
        GameRules rules = GameRules.ConwayDefault();
        
        int gridStartLine = 0;
        
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            
            if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line))
                continue;
                
            if (line.StartsWith("Width:", StringComparison.OrdinalIgnoreCase))
            {
                width = int.Parse(line.Substring(6).Trim());
            }
            else if (line.StartsWith("Height:", StringComparison.OrdinalIgnoreCase))
            {
                height = int.Parse(line.Substring(7).Trim());
            }
            else if (line.StartsWith("Rules:", StringComparison.OrdinalIgnoreCase))
            {
                var ruleStr = line.Substring(6).Trim();
                rules = GameRules.Parse(ruleStr);
            }
            else if (line.StartsWith("Generation:", StringComparison.OrdinalIgnoreCase))
            {
                generation = long.Parse(line.Substring(11).Trim());
            }
            else if (line.Contains("O") || line.Contains("."))
            {
                gridStartLine = i;
                break;
            }
        }

        if (width == 0 || height == 0)
            throw new InvalidDataException("Invalid file format: missing width or height");

        var cells = new bool[width, height];
        int y = 0;
        
        for (int i = gridStartLine; i < lines.Length && y < height; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                continue;
                
            for (int x = 0; x < Math.Min(width, line.Length); x++)
            {
                cells[x, y] = (line[x] == 'O' || line[x] == 'o');
            }
            y++;
        }

        return new GameState(width, height, cells, rules, generation);
    }
}

