using System.Text.RegularExpressions;

namespace AmongUsSalem.Misc;

public static class TextColorizer
{
    // Creeper: You creeped on {target.Name()} and saw {player.Name()} visit them!
    // Creeper: You creeped on {target.Name()} but they were not visited at night.

    // Define the words and their colors
    private static readonly Dictionary<string, Func<string>> colorMap = new()
    {
        // Town
        { "Villager", () => "#3f7095" },
        { "Bulletproof", () => "#3f7095" },
        { "Cops", () => "#3f7095" },
        { "Sniper", () => "#3f7095" },
        { "Granny", () => "#3f7095" },
        { "Deputy", () => "#3f7095" },
        { "Fairy", () => "#3f7095" },
        { "Cop", () => "#3f7095" },
        { "Doctor", () => "#3f7095" },
        { "Gravedigger", () => "#3f7095" },
        { "Traveler", () => "#3f7095" },
        
        // Town
        { "Fool", () => "#3f7095" },

        // Mafia
        { "Mafia", () => "#903e3f" },
        { "Godfather", () => "#903e3f" },
        { "Interrogator", () => "#903e3f" },
        { "Operator", () => "#903e3f" },
        { "Kamikaze", () => "#903e3f" },
        { "Robber", () => "#903e3f" },
        { "Framer", () => "#903e3f" },
        { "Toaster", () => "#903e3f" },
        { "Henchman", () => "#903e3f" },
        { "Lawyer", () => "#903e3f" },
        { "Maid", () => "#903e3f" },

        // Keywords
        { "Village", () => "#3f7095" },
        { "Independent", () => "#3f7095" },
    };

    // Build a single regex that matches any keyword. Longer keys are listed first to prefer them when overlapping.
    private static readonly Regex KeywordRegex = new Regex(
        string.Join("|", colorMap.Keys.OrderByDescending(k => k.Length).Select(Regex.Escape)),
        RegexOptions.Compiled
    );

    public static string ApplyKeywords(this string input)
    {
        return KeywordRegex.Replace(
            input,
            match =>
            {
                var word = match.Value;
                var color = colorMap[word](); // call the function

                return $"<b><color={color}>{word}</color></b>";
            }
        );
    }
}