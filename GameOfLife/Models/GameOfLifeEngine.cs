using System;
using System.Collections.Generic;
using System.Linq;

namespace GameOfLife.Models;

/// <summary>
/// Core engine for Conway's Game of Life with configurable rules
/// </summary>
public class GameOfLifeEngine
{
    private bool[,] _currentState;
    private bool[,] _nextState;
    
    public int Width { get; private set; }
    public int Height { get; private set; }
    public GameRules Rules { get; set; }
    public long Generation { get; private set; }
    public long BornCells { get; private set; }
    public long DeadCells { get; private set; }

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

    public bool GetCell(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return false;
        return _currentState[x, y];
    }

    public void SetCell(int x, int y, bool alive)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height)
        {
            _currentState[x, y] = alive;
        }
    }

    public void ToggleCell(int x, int y)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height)
        {
            _currentState[x, y] = !_currentState[x, y];
        }
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
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                _currentState[x, y] = random.NextDouble() < probability;
            }
        }
        Generation = 0;
        BornCells = 0;
        DeadCells = 0;
    }

    public void NextGeneration()
    {
        long born = 0;
        long died = 0;

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                int neighbors = CountNeighbors(x, y);
                bool currentlyAlive = _currentState[x, y];
                bool nextAlive = Rules.ShouldBeAlive(currentlyAlive, neighbors);
                
                _nextState[x, y] = nextAlive;

                if (!currentlyAlive && nextAlive)
                    born++;
                else if (currentlyAlive && !nextAlive)
                    died++;
            }
        }

        // Swap states
        var temp = _currentState;
        _currentState = _nextState;
        _nextState = temp;

        Generation++;
        BornCells += born;
        DeadCells += died;
    }

    private int CountNeighbors(int x, int y)
    {
        int count = 0;
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                
                int nx = x + dx;
                int ny = y + dy;
                
                if (nx >= 0 && nx < Width && ny >= 0 && ny < Height)
                {
                    if (_currentState[nx, ny])
                        count++;
                }
            }
        }
        return count;
    }

    public int GetLivingCellsCount()
    {
        int count = 0;
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (_currentState[x, y])
                    count++;
            }
        }
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
}

