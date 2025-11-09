using GameOfLife.Models.Coloring;

namespace GameOfLife.Models;

/// <summary>
///     Core engine for Conway's Game of Life with configurable rules
/// </summary>
public class GameOfLifeEngine
{
    private bool[,] _currentState;
    private bool[,] _nextState;

    public GameOfLifeEngine(int width, int height, GameRules? rules = null)
    {
        Width = width;
        Height = height;
        Rules = rules ?? GameRules.ConwayDefault();
        _currentState = new bool[width, height];
        _nextState = new bool[width, height];
        Generation = 0;
        BornCells = 0;
        DeadCells = 0;
    }

    public int Width { get; }
    public int Height { get; }
    public GameRules Rules { get; set; }
    public long Generation { get; private set; }
    public long BornCells { get; private set; }
    public long DeadCells { get; private set; }

    public bool GetCell(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return false;
        return _currentState[x, y];
    }

    public void SetCell(int x, int y, bool alive)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height)
            _currentState[x, y] = alive;
    }

    public void ToggleCell(int x, int y)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height)
            _currentState[x, y] = !_currentState[x, y];
    }

    public void Clear()
    {
        Array.Clear(_currentState, 0, _currentState.Length);
        Generation = 0;
        BornCells = 0;
        DeadCells = 0;
    }

    public void Randomize(double probability = 0.3)
    {
        var random = new Random();
        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Height; y++)
            _currentState[x, y] = random.NextDouble() < probability;

        Generation = 0;
        BornCells = 0;
        DeadCells = 0;
    }

    public void NextGeneration(IColoringModel? coloringModel = null)
    {
        long born = 0;
        long died = 0;
        var newBornCells = new List<(int x, int y)>();
        var deadCells = new List<(int x, int y)>();

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Height; y++)
        {
            var neighbors = CountNeighbors(x, y);
            var currentlyAlive = _currentState[x, y];
            var nextAlive = Rules.ShouldBeAlive(currentlyAlive, neighbors);

            _nextState[x, y] = nextAlive;

            if (!currentlyAlive && nextAlive)
            {
                born++;
                newBornCells.Add((x, y));
            }
            else if (currentlyAlive && !nextAlive)
            {
                died++;
                deadCells.Add((x, y));
            }
        }

        // Swap states first
        (_currentState, _nextState) = (_nextState, _currentState);

        // Then notify coloring model with the NEW current state
        if (coloringModel != null && newBornCells.Count > 0)
            coloringModel.OnCellsBorn(newBornCells, _currentState);

        if (coloringModel != null && deadCells.Count > 0)
            coloringModel.OnCellsDead(deadCells);


        Generation++;
        BornCells += born;
        DeadCells += died;
    }

    private int CountNeighbors(int x, int y)
    {
        var count = 0;
        for (var dx = -1; dx <= 1; dx++)
        for (var dy = -1; dy <= 1; dy++)
        {
            if (dx == 0 && dy == 0)
                continue;

            var nx = x + dx;
            var ny = y + dy;

            if (nx >= 0 && nx < Width && ny >= 0 && ny < Height)
                if (_currentState[nx, ny])
                    count++;
        }

        return count;
    }

    public int GetLivingCellsCount()
    {
        var count = 0;
        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Height; y++)
            if (_currentState[x, y])
                count++;

        return count;
    }

    public bool[,] GetStateCopy()
    {
        return (bool[,])_currentState.Clone();
    }

    public void SetState(bool[,] state)
    {
        if (state.GetLength(0) != Width || state.GetLength(1) != Height)
            throw new ArgumentException("State dimensions must match grid dimensions");

        Array.Copy(state, _currentState, state.Length);
    }

    /// <summary>
    ///     Place a preset pattern at a specific position on the grid
    /// </summary>
    public void PlacePattern(PresetPatterns pattern, int startX, int startY, bool merge = false)
    {
        for (var py = 0; py < pattern.Height; py++)
        for (var px = 0; px < pattern.Width; px++)
        {
            var gridX = startX + px;
            var gridY = startY + py;

            if (gridX >= 0 && gridX < Width && gridY >= 0 && gridY < Height)
            {
                var patternCell = pattern.Pattern[px, py];
                if (merge)
                    _currentState[gridX, gridY] = _currentState[gridX, gridY] || patternCell;
                else
                    _currentState[gridX, gridY] = patternCell;
            }
        }
    }

    /// <summary>
    ///     Get the number of neighbors for a specific cell
    /// </summary>
    public int GetNeighborCount(int x, int y)
    {
        return CountNeighbors(x, y);
    }
}
