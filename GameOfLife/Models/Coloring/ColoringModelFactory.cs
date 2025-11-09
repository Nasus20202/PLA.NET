namespace GameOfLife.Models.Coloring;

public static class ColoringModelFactory
{
    private enum ColoringModelType
    {
        Standard,
        Immigration,
        QuadLife,
        Entropy,
        Rainbow,
    }

    private static IColoringModel CreateColoring(
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
        return Enum.TryParse<ColoringModelType>(name, out var type)
            ? CreateColoring(type, gridWidth, gridHeight)
            : new StandardColoring();
    }

    public static List<string> GetAvailableColorings()
    {
        return Enum.GetValues<ColoringModelType>().Select(type => type.ToString()).ToList();
    }
}
