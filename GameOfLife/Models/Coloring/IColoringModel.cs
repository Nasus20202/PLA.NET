using System.Windows.Media;

namespace GameOfLife.Models.Coloring;

/// <summary>
///     Interface for different coloring models for Game of Life variants
/// </summary>
public interface IColoringModel
{
    string Name { get; }
    string Description { get; }

    /// <summary>
    ///     Gets the color for a cell based on its state
    /// </summary>
    Color GetCellColor(int x, int y, bool isAlive, int age, int neighbors);

    /// <summary>
    ///     Should be called when generation advances to update any internal state
    /// </summary>
    void NextGeneration();

    /// <summary>
    ///     Initialize colors for all cells in the grid based on current alive state
    /// </summary>
    void InitializeColorsForGrid(bool[,] gridState);

    /// <summary>
    ///     Called when new cells are born - allows coloring model to assign colors based on neighbors
    /// </summary>
    void OnCellsBorn(List<(int x, int y)> newCells, bool[,] currentState);

    /// <summary>
    ///     Called when cells die - allows coloring model to clean up state
    /// </summary>
    void OnCellsDead(List<(int x, int y)> deadCells);

    /// <summary>
    ///     Clears any internal state
    /// </summary>
    void Clear();
}
