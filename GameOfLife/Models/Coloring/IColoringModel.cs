using System.Windows.Media;

namespace GameOfLife.Models.Coloring;

public interface IColoringModel
{
    string Name { get; }

    string Description { get; }

    Color GetCellColor(int x, int y, bool isAlive, int age, int neighbors);

    void NextGeneration();

    void InitializeColorsForGrid(bool[,] gridState);

    void OnCellsBorn(List<(int x, int y)> newCells, bool[,] currentState);

    void OnCellsDead(List<(int x, int y)> deadCells);

    void Clear();

    List<string> Serialize();

    void Deserialize(List<string> data);
}
