﻿using System;
using System.Collections.Generic;

namespace GameOfLife.Models;

/// <summary>
/// Preset patterns for Game of Life (from Wikipedia and other sources)
/// </summary>
public class PresetPatterns
{
    public string Name { get; set; }
    public string Description { get; set; }
    public bool[,] Pattern { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public PresetPatterns(string name, string description, bool[,] pattern)
    {
        Name = name;
        Description = description;
        Pattern = pattern;
        Width = pattern.GetLength(0);
        Height = pattern.GetLength(1);
    }

    /// <summary>
    /// Gets a collection of built-in preset patterns
    /// </summary>
    public static Dictionary<string, PresetPatterns> GetAllPatterns()
    {
        var patterns = new Dictionary<string, PresetPatterns>();

        // Still Lifes
        patterns["Block"] = CreateBlock();
        patterns["Beehive"] = CreateBeehive();
        patterns["Loaf"] = CreateLoaf();
        patterns["Boat"] = CreateBoat();
        patterns["Tub"] = CreateTub();

        // Oscillators
        patterns["Blinker"] = CreateBlinker();
        patterns["Beacon"] = CreateBeacon();
        patterns["Toad"] = CreateToad();
        patterns["Pulsar"] = CreatePulsar();
        patterns["Pent-decathlon"] = CreatePentDecathlon();

        // Spaceships
        patterns["Glider"] = CreateGlider();
        patterns["LWSS"] = CreateLWSS();
        patterns["MWSS"] = CreateMWSS();
        patterns["HWSS"] = CreateHWSS();

        // Methuselahs
        patterns["Acorn"] = CreateAcorn();
        patterns["R-pentomino"] = CreateRPentomino();

        return patterns;
    }

    #region Still Lifes

    private static PresetPatterns CreateBlock()
    {
        bool[,] pattern = new bool[2, 2]
        {
            { true, true },
            { true, true }
        };
        return new PresetPatterns("Block", "Simplest still life (2x2 square)", pattern);
    }

    private static PresetPatterns CreateBeehive()
    {
        bool[,] pattern = new bool[,]
        {
            { false, true, false },
            { true, false, true }
        };
        return new PresetPatterns("Beehive", "Common still life", pattern);
    }

    private static PresetPatterns CreateLoaf()
    {
        bool[,] pattern = new bool[,]
        {
            { false, true, false },
            { true, false, true },
            { false, true, false }
        };
        return new PresetPatterns("Loaf", "Still life pattern", pattern);
    }

    private static PresetPatterns CreateBoat()
    {
        bool[,] pattern = new bool[,]
        {
            { true, true, false },
            { true, false, true }
        };
        return new PresetPatterns("Boat", "Still life pattern", pattern);
    }

    private static PresetPatterns CreateTub()
    {
        bool[,] pattern = new bool[,]
        {
            { false, true, false },
            { true, false, true },
            { false, true, false }
        };
        return new PresetPatterns("Tub", "Still life pattern", pattern);
    }

    #endregion

    #region Oscillators

    private static PresetPatterns CreateBlinker()
    {
        bool[,] pattern = new bool[,]
        {
            { true, true, true }
        };
        return new PresetPatterns("Blinker", "Period 2 oscillator (simplest)", pattern);
    }

    private static PresetPatterns CreateBeacon()
    {
        bool[,] pattern = new bool[,]
        {
            { true, true },
            { false, false }
        };
        return new PresetPatterns("Beacon", "Period 2 oscillator", pattern);
    }

    private static PresetPatterns CreateToad()
    {
        bool[,] pattern = new bool[,]
        {
            { false, true, true, true },
            { true, true, true, false }
        };
        return new PresetPatterns("Toad", "Period 2 oscillator", pattern);
    }

    private static PresetPatterns CreatePulsar()
    {
        bool[,] pattern = new bool[,]
        {
            { false, false, true, true, true, false, false, false, true, true, true, false, false },
            { false, false, false, false, false, false, false, false, false, false, false, false, false },
            { true, false, false, false, true, false, false, false, true, false, false, false, true },
            { true, false, false, false, true, false, false, false, true, false, false, false, true },
            { true, false, false, false, true, false, false, false, true, false, false, false, true },
            { false, false, true, true, true, false, false, false, true, true, true, false, false },
            { false, false, false, false, false, false, false, false, false, false, false, false, false },
            { false, false, true, true, true, false, false, false, true, true, true, false, false },
            { true, false, false, false, true, false, false, false, true, false, false, false, true },
            { true, false, false, false, true, false, false, false, true, false, false, false, true },
            { true, false, false, false, true, false, false, false, true, false, false, false, true },
            { false, false, false, false, false, false, false, false, false, false, false, false, false },
            { false, false, true, true, true, false, false, false, true, true, true, false, false }
        };
        return new PresetPatterns("Pulsar", "Period 3 oscillator", pattern);
    }

    private static PresetPatterns CreatePentDecathlon()
    {
        bool[,] pattern = new bool[,]
        {
            { false, true, false },
            { true, false, true },
            { false, true, false },
            { false, true, false },
            { false, true, false },
            { false, true, false },
            { true, false, true },
            { false, true, false }
        };
        return new PresetPatterns("Pent-decathlon", "Period 15 oscillator", pattern);
    }

    #endregion

    #region Spaceships

    private static PresetPatterns CreateGlider()
    {
        bool[,] pattern = new bool[,]
        {
            { false, true, false },
            { false, false, true },
            { true, true, true }
        };
        return new PresetPatterns("Glider", "Smallest spaceship (period 4, moves diagonally)", pattern);
    }

    private static PresetPatterns CreateLWSS()
    {
        bool[,] pattern = new bool[,]
        {
            { true, false, false, true, false },
            { false, false, false, false, true },
            { true, false, false, false, true },
            { false, true, true, true, true }
        };
        return new PresetPatterns("LWSS", "Lightweight spaceship (moves horizontally)", pattern);
    }

    private static PresetPatterns CreateMWSS()
    {
        bool[,] pattern = new bool[,]
        {
            { false, true, false, false, true, false },
            { true, false, false, false, false, true },
            { true, false, false, false, false, true },
            { true, true, true, true, true, false }
        };
        return new PresetPatterns("MWSS", "Middleweight spaceship (moves horizontally)", pattern);
    }

    private static PresetPatterns CreateHWSS()
    {
        bool[,] pattern = new bool[,]
        {
            { false, true, false, false, false, true, false },
            { true, false, false, false, false, false, true },
            { true, false, false, false, false, false, true },
            { true, true, true, true, true, true, false }
        };
        return new PresetPatterns("HWSS", "Heavyweight spaceship (moves horizontally)", pattern);
    }

    #endregion

    #region Methuselahs

    private static PresetPatterns CreateAcorn()
    {
        bool[,] pattern = new bool[,]
        {
            { false, true, false, false, false, false, false },
            { true, false, true, true, false, true, true }
        };
        return new PresetPatterns("Acorn", "Methuselah (5206 generations)", pattern);
    }

    private static PresetPatterns CreateRPentomino()
    {
        bool[,] pattern = new bool[,]
        {
            { false, true, true },
            { true, true, false },
            { false, true, false }
        };
        return new PresetPatterns("R-pentomino", "Methuselah (thousands of generations)", pattern);
    }

    #endregion
}

