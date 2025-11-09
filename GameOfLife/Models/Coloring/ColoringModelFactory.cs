namespace GameOfLife.Models.Coloring;

/// <summary>
///     Factory for creating coloring models
/// </summary>
public class ColoringModelFactory
{
    /// <summary>
    ///     Enum for available coloring models
    /// </summary>
    public enum ColoringModelType
    {
        Standard,
        Immigration,
        QuadLife,
        Entropy,
        Rainbow,
    }

    public static IColoringModel CreateColoring(
        ColoringModelType type,
        int gridWidth = 100,
        int gridHeight = 100
    )
    {
        return type switch
        {
            ColoringModelType.Standard => new StandardColoring(),
            ColoringModelType.Immigration => new ImmigrationColoring(),
            ColoringModelType.QuadLife => new QuadLifeColoring(),
            ColoringModelType.Entropy => new EntropyColoring(),
            ColoringModelType.Rainbow => new RainbowColoring(gridWidth, gridHeight),
            _ => new StandardColoring(),
        };
    }

    public static IColoringModel CreateColoring(
        string name,
        int gridWidth = 100,
        int gridHeight = 100
    )
    {
        if (Enum.TryParse<ColoringModelType>(name, out var type))
            return CreateColoring(type, gridWidth, gridHeight);
        return new StandardColoring();
    }

    public static List<string> GetAvailableColorings()
    {
        return Enum.GetValues<ColoringModelType>().Select(type => type.ToString()).ToList();
    }
}
