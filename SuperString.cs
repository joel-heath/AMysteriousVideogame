namespace AMysteriousVideogame;

/// <summary>
/// Represents a multicolored string
/// </summary>
/// <param name="contents">A string with color codes indicated by §c, where c is a hexadecimal numeral representing the color.</param>
/// <param name="newLines">Number of new lines to be written after contents</param>
/// <param name="highlight">What color to set Console.BackgroundColor to</param>
public class SuperString(string contents, int newLines = 1, ConsoleColor highlight = ConsoleColor.Black)
{
    /// <summary>
    /// Contents of the SuperString including color codes (for string without color codes, use ToString())
    /// </summary>
    public string Contents { get; set; } = contents;

    private readonly (string message, ConsoleColor color)[] chunks = FormatParser(contents);
    public (string message, ConsoleColor color)[] Chunks => chunks;
    public int NewLines { get; set; } = newLines;
    public ConsoleColor Highlight { get; set; } = highlight;
    public int Length => Contents.Length - 2 * Contents.Count(c => c == '§');
    public ConsoleColor FirstColor => chunks.Length == 0 ? throw new FormatException() : chunks[0].color;

    public SuperString(string contents, ConsoleColor color, ConsoleColor highlight = ConsoleColor.Black)
        : this('§' + ((int)color).ToString("X") + contents, 1, highlight) { }

    public SuperString(params (string, ConsoleColor)[] contents)
        : this(string.Concat(contents.Select(c => '§' + ((int)c.Item2).ToString("X") + c.Item1))) { }

    public static implicit operator SuperString(string s) => new(s);

    /// <summary>
    /// Contents of the SuperString excluding color codes (for string with color codes, use Contents)
    /// </summary>
    public override string ToString() => RemoveColorCodes(Contents);

    private static (string, ConsoleColor)[] FormatParser(string textToParse)
    {
        var chunks = textToParse.Split('§');
        if (textToParse.StartsWith('§'))
        {
            return [..chunks.Skip(1).Select(c => c.Length == 0 ? throw new FormatException() :
                    (c[1..], (ConsoleColor)Convert.ToInt32($"{c[0]}", 16)))];
        }
        return [(chunks[0], (ConsoleColor)(-1)),
                ..chunks.Skip(1).Select(c => c.Length == 0 ? throw new FormatException() :
                    (c[1..], (ConsoleColor)Convert.ToInt32($"{c[0]}", 16)))];
    }

    private static string RemoveColorCodes(string textToStrip)
    {
        var chunks = textToStrip.Split('§');
        if (textToStrip.StartsWith('§'))
        {
            return string.Concat(chunks.Skip(1).Select(c => c.Length == 0 ? throw new FormatException() : c[1..]));
        }
        return chunks[0] + string.Concat(chunks.Skip(1).Select(c => c.Length == 0 ? throw new FormatException() : c[1..]));
    }
}