using System.Text.RegularExpressions;

namespace GameOfLife.Models;

/// <summary>
/// Represents the rules for the Game of Life in B/S notation
/// Example: B3/S23 (Conway's Game of Life)
/// </summary>
public class GameRules
{
    public HashSet<int> BirthNumbers { get; set; }
    public HashSet<int> SurvivalNumbers { get; set; }

    public GameRules()
    {
        BirthNumbers = new HashSet<int>();
        SurvivalNumbers = new HashSet<int>();
    }

    public GameRules(IEnumerable<int> birthNumbers, IEnumerable<int> survivalNumbers)
    {
        BirthNumbers = new HashSet<int>(birthNumbers);
        SurvivalNumbers = new HashSet<int>(survivalNumbers);
    }

    public static GameRules ConwayDefault()
    {
        return new GameRules(new[] { 3 }, new[] { 2, 3 });
    }

    public bool ShouldBeAlive(bool currentlyAlive, int neighbors)
    {
        if (currentlyAlive)
            return SurvivalNumbers.Contains(neighbors);
        else
            return BirthNumbers.Contains(neighbors);
    }

    public override string ToString()
    {
        var birth = string.Join("", BirthNumbers.OrderBy(n => n));
        var survival = string.Join("", SurvivalNumbers.OrderBy(n => n));
        return $"B{birth}/S{survival}";
    }

    public static GameRules Parse(string ruleString)
    {
        if (string.IsNullOrWhiteSpace(ruleString))
            return ConwayDefault();

        // Pattern: B[digits]/S[digits]
        var match = Regex.Match(ruleString.Trim().ToUpper(), @"B(\d*)/S(\d*)");
        if (!match.Success)
            throw new ArgumentException(
                $"Invalid rule format: {ruleString}. Expected format: B3/S23"
            );

        var birthNumbers = match.Groups[1].Value.Select(c => int.Parse(c.ToString()));
        var survivalNumbers = match.Groups[2].Value.Select(c => int.Parse(c.ToString()));

        return new GameRules(birthNumbers, survivalNumbers);
    }

    public static bool TryParse(string ruleString, out GameRules? rules)
    {
        try
        {
            rules = Parse(ruleString);
            return true;
        }
        catch
        {
            rules = null;
            return false;
        }
    }
}
