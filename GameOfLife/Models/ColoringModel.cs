using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace GameOfLife.Models;

/// <summary>
/// Different coloring models for Game of Life variants
/// </summary>
public abstract class ColoringModel
{
    public string Name { get; protected set; } = "";
    public string Description { get; protected set; } = "";

    /// <summary>
    /// Gets the color for a cell based on its state
    /// </summary>
    public abstract Color GetCellColor(int x, int y, bool isAlive, int age, int neighbors);

    /// <summary>
    /// Should be called when generation advances to update any internal state
    /// </summary>
    public virtual void NextGeneration() { }

    /// <summary>
    /// Clears any internal state
    /// </summary>
    public virtual void Clear() { }
}

/// <summary>
/// Standard Conway's Game of Life coloring (black and white)
/// </summary>
public class StandardColoring : ColoringModel
{
    public StandardColoring()
    {
        Name = "Standard";
        Description = "Standard black and white coloring";
    }

    public override Color GetCellColor(int x, int y, bool isAlive, int age, int neighbors)
    {
        return isAlive ? Colors.White : Colors.Black;
    }
}

/// <summary>
/// Immigration variant - cells have colors based on which neighbor caused them to be born
/// </summary>
public class ImmigrationColoring : ColoringModel
{
    private Dictionary<(int, int), Color> _cellColors = new();
    private Color[] _colors = new[]
    {
        Colors.Red,
        Colors.Green,
        Colors.Blue,
        Colors.Yellow,
        Colors.Cyan,
        Colors.Magenta,
        Colors.Orange
    };

    public ImmigrationColoring()
    {
        Name = "Immigration";
        Description = "Cells get different colors based on which neighbor caused them to be born";
    }

    public override Color GetCellColor(int x, int y, bool isAlive, int age, int neighbors)
    {
        if (!isAlive)
            return Colors.Black;

        var key = (x, y);
        if (_cellColors.ContainsKey(key))
            return _cellColors[key];

        return Colors.White;
    }

    /// <summary>
    /// Set a color for a cell (called when cell is born)
    /// </summary>
    public void SetCellColor(int x, int y, Color color)
    {
        _cellColors[(x, y)] = color;
    }

    /// <summary>
    /// Get color for a cell at a specific position
    /// </summary>
    public Color GetAssignedColor(int x, int y)
    {
        var key = (x, y);
        return _cellColors.ContainsKey(key) ? _cellColors[key] : Colors.White;
    }

    public override void Clear()
    {
        _cellColors.Clear();
    }

    public Color[] GetAvailableColors() => _colors;
}

/// <summary>
/// QuadLife coloring - uses four states/colors based on region or age
/// </summary>
public class QuadLifeColoring : ColoringModel
{
    private Dictionary<(int, int), int> _cellStates = new(); // 0-3 for quadrants/colors
    private Color[] _quadColors = new[]
    {
        Color.FromRgb(255, 0, 0),   // Red
        Color.FromRgb(0, 255, 0),   // Green
        Color.FromRgb(0, 0, 255),   // Blue
        Color.FromRgb(255, 255, 0)  // Yellow
    };

    public QuadLifeColoring()
    {
        Name = "QuadLife";
        Description = "Four different colors based on regions or age mod 4";
    }

    public override Color GetCellColor(int x, int y, bool isAlive, int age, int neighbors)
    {
        if (!isAlive)
            return Colors.Black;

        var key = (x, y);
        if (_cellStates.ContainsKey(key))
            return _quadColors[_cellStates[key] % 4];

        return _quadColors[0];
    }

    /// <summary>
    /// Set state (0-3) for a cell
    /// </summary>
    public void SetCellState(int x, int y, int state)
    {
        _cellStates[(x, y)] = state % 4;
    }

    /// <summary>
    /// Get state for a cell
    /// </summary>
    public int GetCellState(int x, int y)
    {
        var key = (x, y);
        return _cellStates.ContainsKey(key) ? _cellStates[key] : 0;
    }

    public override void Clear()
    {
        _cellStates.Clear();
    }

    public Color[] GetQuadColors() => _quadColors;
}

/// <summary>
/// Age-based coloring - cells have different colors based on their age
/// </summary>
public class AgeBasedColoring : ColoringModel
{
    private Dictionary<(int, int), int> _cellAge = new();

    public AgeBasedColoring()
    {
        Name = "Age-Based";
        Description = "Colors vary based on cell age (younger = brighter)";
    }

    public override Color GetCellColor(int x, int y, bool isAlive, int age, int neighbors)
    {
        if (!isAlive)
            return Colors.Black;

        var key = (x, y);
        if (_cellAge.ContainsKey(key))
        {
            int cellAge = _cellAge[key];
            // Color transitions from bright to dark based on age
            byte brightness = (byte)Math.Max(50, 255 - (cellAge * 5));
            return Color.FromRgb(brightness, brightness, brightness);
        }

        return Colors.White;
    }

    public void SetCellAge(int x, int y, int age)
    {
        _cellAge[(x, y)] = age;
    }

    public void IncrementAllAges()
    {
        var newAges = new Dictionary<(int, int), int>();
        foreach (var kvp in _cellAge)
        {
            newAges[kvp.Key] = kvp.Value + 1;
        }
        _cellAge = newAges;
    }

    public override void Clear()
    {
        _cellAge.Clear();
    }
}

/// <summary>
/// Entropy-based coloring - colors based on neighbor count
/// </summary>
public class EntropyColoring : ColoringModel
{
    public EntropyColoring()
    {
        Name = "Entropy";
        Description = "Color based on number of neighbors (0-8)";
    }

    public override Color GetCellColor(int x, int y, bool isAlive, int age, int neighbors)
    {
        if (!isAlive)
            return Colors.Black;

        // Map neighbor count (0-8) to color
        byte value = (byte)(neighbors * 32); // 0-256
        return Color.FromRgb(255, value, 0); // Gradient from red to orange/yellow
    }
}

/// <summary>
/// Rainbow coloring - based on position in grid
/// </summary>
public class RainbowColoring : ColoringModel
{
    private int _gridWidth = 100;
    private int _gridHeight = 100;

    public RainbowColoring(int gridWidth = 100, int gridHeight = 100)
    {
        Name = "Rainbow";
        Description = "Colors based on cell position in grid";
        _gridWidth = gridWidth;
        _gridHeight = gridHeight;
    }

    public override Color GetCellColor(int x, int y, bool isAlive, int age, int neighbors)
    {
        if (!isAlive)
            return Colors.Black;

        // Create rainbow based on position
        double hue = ((double)x / _gridWidth) * 360;
        return HsvToRgb(hue, 1.0, 1.0);
    }

    private Color HsvToRgb(double hue, double saturation, double value)
    {
        double c = value * saturation;
        double x = c * (1 - Math.Abs((hue / 60) % 2 - 1));
        double m = value - c;

        double r = 0, g = 0, b = 0;

        if (hue < 60)
        {
            r = c; g = x; b = 0;
        }
        else if (hue < 120)
        {
            r = x; g = c; b = 0;
        }
        else if (hue < 180)
        {
            r = 0; g = c; b = x;
        }
        else if (hue < 240)
        {
            r = 0; g = x; b = c;
        }
        else if (hue < 300)
        {
            r = x; g = 0; b = c;
        }
        else
        {
            r = c; g = 0; b = x;
        }

        return Color.FromRgb(
            (byte)((r + m) * 255),
            (byte)((g + m) * 255),
            (byte)((b + m) * 255)
        );
    }
}

/// <summary>
/// Factory for creating coloring models
/// </summary>
public class ColoringModelFactory
{
    public static ColoringModel CreateColoring(string name, int gridWidth = 100, int gridHeight = 100)
    {
        return name switch
        {
            "Standard" => new StandardColoring(),
            "Immigration" => new ImmigrationColoring(),
            "QuadLife" => new QuadLifeColoring(),
            "Age-Based" => new AgeBasedColoring(),
            "Entropy" => new EntropyColoring(),
            "Rainbow" => new RainbowColoring(gridWidth, gridHeight),
            _ => new StandardColoring()
        };
    }

    public static List<string> GetAvailableColorings()
    {
        return new List<string>
        {
            "Standard",
            "Immigration",
            "QuadLife",
            "Age-Based",
            "Entropy",
            "Rainbow"
        };
    }
}

