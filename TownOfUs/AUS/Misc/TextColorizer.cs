using System.Text.RegularExpressions;

namespace AmongUsSalem.Misc;

public static class TextColorizer
{
    private static readonly Dictionary<string, Func<string>> colorMap = new()
    {
        // Crewmate
        { "Crewmate", () => "#b3ffff" },
        
        // Neutral
        { "Neutral", () => "#a9a9a9" },
        { "Survivor", () => "#dddd00" },

        // Impostor
        { "Impostor", () => "#ff0000" },
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