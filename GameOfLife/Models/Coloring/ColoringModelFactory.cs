using System.Collections.Generic;

namespace GameOfLife.Models.Coloring;

/// <summary>
/// Factory for creating coloring models
/// </summary>
public class ColoringModelFactory
{
    public static IColoringModel CreateColoring(string name, int gridWidth = 100, int gridHeight = 100)
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

