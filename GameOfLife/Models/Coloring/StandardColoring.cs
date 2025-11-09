using System.Windows.Media;

namespace GameOfLife.Models.Coloring;

public class StandardColoring : IColoringModel
{
    public string Name => "Standard";
    public string Description => "Standard black and white coloring";

    public Color GetCellColor(int x, int y, bool isAlive, int age, int neighbors)
    {
        return isAlive ? Colors.White : Colors.Black;
    }

    public void NextGeneration() { }

    public void InitializeColorsForGrid(bool[,] gridState) { }

    public void OnCellsBorn(List<(int x, int y)> newCells, bool[,] currentState) { }

    public void OnCellsDead(List<(int x, int y)> deadCells) { }

    public void Clear() { }

    public List<string> Serialize()
    {
        return [];
    }

    public void Deserialize(List<string> data) { }
}
