using ManagedBass;
using System.Drawing;

namespace AMysteriousVideogame;

internal static class ConsoleUtils
{
    public class EnterException : Exception { }

    static readonly char[] validCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_-.# ".ToCharArray();
    public static void ClearKeyBuffer()
    {
        while (Console.KeyAvailable) { Console.ReadKey(true); } // clear consolekey buffer
    }

    public static async Task Write(string contents, int newLines, ConsoleColor color, ConsoleColor highlight, int x, int y, int sleep, CancellationToken? ct = null)
    {
        // -1 x & y is default code for current cursor position.
        Console.ForegroundColor = color;
        Console.BackgroundColor = highlight;

        if (x > -1) Console.CursorLeft = x;
        if (y > -1) Console.CursorTop = y;

        if (sleep == -1 || (ct is not null && ct.Value.IsCancellationRequested)) Console.Write(contents);
        else
        {
            for (int i = 0; i < contents.Length; i++)
            {
                Console.Write(contents[i]);

                //await Task.Delay(sleep);
                for (int j = 0; j < sleep / 15; j++)
                {
                    if (ct is not null && ct.Value.IsCancellationRequested)
                    {
                        if (i < contents.Length - 1) Console.Write(contents[(i + 1)..]);
                        goto @break;
                    }
                    await Task.Delay(15);
                }
            }
        }
        @break:

        for (int i = 0; i < newLines; i++) Console.WriteLine();
    }

    // Print with multiple colors  §f = White [default] §0 = Black  [ONLY 1 HIGHLIGHT]
    public static async Task Print(SuperString contents, int newLines = 1, int x = -1, int y = -1, int sleep = -1, ConsoleColor initColor = ConsoleColor.White, ConsoleColor highlight = ConsoleColor.Black, CancellationToken? ct = null)
    {
        ConsoleColor color = initColor;
        (string contents, ConsoleColor color)[] texts = contents.Chunks;

        for (int i = 0; i < texts.Length - 1; i++)
        {
            if ((int)texts[i].color > -1) color = texts[i].color;

            await Write(texts[i].contents, 0, color, highlight, x, y, sleep, ct);
        }
        await Write(texts[^1].contents, newLines, color, highlight, x, y, sleep, ct);
    }


    static (string input, int x, int y) HandleKeyPress(string input, ConsoleKeyInfo keyPressed, int margin, int x, int y)
    {
        Console.CursorVisible = false;

        keyPressed.Modifiers.HasFlag(ConsoleModifiers.Control); // Check if control key is pressed

        switch (keyPressed.Key)
        {
            //case ConsoleKey.Escape: throw new EscapeException();
            case ConsoleKey.Enter: throw new EnterException();
            case ConsoleKey.Home: x = 0; break;
            case ConsoleKey.End: x = input.Length; break;
            case ConsoleKey.LeftArrow: x = x == 0 ? x : x - 1; break;             // Don't move back if at start of string
            case ConsoleKey.RightArrow: x = x == input.Length ? x : x + 1; break; // Don't move forward if at end of string

            case ConsoleKey.Backspace: // Backspace is just a delete with the cursor moved back one
                if (x > 0)   // If there is room to do so
                {
                    Console.CursorLeft--; x--; // Go back a space
                    keyPressed = new ConsoleKeyInfo('\0', ConsoleKey.Delete, false, false, false);   // Creating a delete keypress
                    (input, x, y) = HandleKeyPress(input, keyPressed, margin, x, y);                 // Recursive call to delete
                }
                break;

            case ConsoleKey.Delete:
                if (x < input.Length)
                {
                    input = input.Remove(x, 1);     // Remove character at cursor position
                    // MainConsole.Refresh(inputString);   // Refresh screen
                    Console.Write(string.Concat(input[x..]) + ' ');
                    Console.CursorLeft = margin + x; // Move cursor back to where it was
                }
                break;

            default:
                if (validCharacters.Contains(keyPressed.KeyChar))
                {
                    string letter = keyPressed.KeyChar.ToString();
                    input = input.Insert(x, letter);         // Move everything infront of cursor to the right

                    Console.Write(string.Concat(input[x..]) + ' ');

                    x++; // Move cursor one step forward
                    Console.CursorLeft = margin + x; // Move cursor back to where it was

                }
                break;
        }

        Console.CursorLeft = margin + x; // Move cursor back to where it was
        Console.CursorVisible = true;
        return (input, x, y);
    }
    public static string ReadLine()
    {
        Console.CursorVisible = true;

        int startPoint = Console.CursorLeft; // so that cursor does not go beyond starting point of text
        int x = 0, y = Console.CursorTop;
        string input = string.Empty;

        bool complete = false;
        while (!complete)
        {
            ConsoleKeyInfo keyPressed = Console.ReadKey(true);
            try { (input, x, y) = HandleKeyPress(input, keyPressed, startPoint, x, y); }
            catch (EnterException)
            {
                if (input.Length > 0) complete = true;
            }
        }

        Console.CursorVisible = false;
        return input;
    }
    public static async Task<int> Choose(IList<SuperString> options, bool escapable = true, CancellationToken? ct = null, int padding = 3, bool softClear = true, bool nuclearClear = false)
    {
        (int initX, int initY) = Console.GetCursorPosition();

        MediaPlayer mp = new();
        await mp.LoadAsync("SFX/Bleep.mp3");

        ClearKeyBuffer();
        const int spacing = 6; // space between options
        const int boxPadding = 4; // spacing of 2 on each side of option

        int choice = 0;
        int indent = (Console.WindowWidth - (options.Sum(o => o.Length + boxPadding) + (options.Count - 1) * spacing)) / 2;
        int xIndent = indent;
        int yIndent = Console.WindowHeight - (3 + padding);
        bool chosen = false;
        while (!chosen)
        {
            Console.CursorVisible = false;
            xIndent = indent;

            // write all options with current selected highlighted
            for (int i = 0; i < options.Count; i++)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = ConsoleColor.Black;
                Console.SetCursorPosition(xIndent, yIndent);
                Console.WriteLine(new string('-', options[i].Length + boxPadding));
                Console.SetCursorPosition(xIndent, yIndent + 1);
                Console.Write("| ");
                if (choice == i)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.BackgroundColor = ConsoleColor.DarkGray;
                }
                else
                {
                    Console.ForegroundColor = (int)options[i].FirstColor == -1 ? ConsoleColor.White : options[i].FirstColor;
                }
                Console.Write(options[i]);
                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = ConsoleColor.Black;

                Console.WriteLine(" |");
                Console.SetCursorPosition(xIndent, yIndent + 2);
                Console.Write(new string('-', options[i].Length + boxPadding));

                xIndent += options[i].Length + boxPadding + spacing;
            }

            while (!Console.KeyAvailable)
            {
                if (ct is not null && ct.Value.IsCancellationRequested)
                {
                    ct.Value.ThrowIfCancellationRequested();
                }
                await Task.Delay(15);
            }

            switch (Console.ReadKey(true).Key)
            {
                case ConsoleKey.D:
                case ConsoleKey.RightArrow:
                    if (choice < options.Count - 1)
                    {
                        mp.Stop(); mp.Play();
                        choice++;
                    };
                    break;
                case ConsoleKey.A:
                case ConsoleKey.LeftArrow:
                    if (choice > 0)
                    {
                        mp.Stop(); mp.Play();
                        choice--;
                    }
                    break;
                case ConsoleKey.Spacebar:
                case ConsoleKey.Enter:
                    chosen = true;
                    break;
                case ConsoleKey.Escape:
                    if (escapable)
                    {
                        choice = -1;
                        chosen = true;
                    }
                    break;
            }

            //Console.CursorVisible = true;
        }
        if (nuclearClear) Console.Clear();
        else if (softClear)
        {
            xIndent = indent;

            Console.BackgroundColor = ConsoleColor.Black;
            for (int i = 0; i < options.Count; i++)
            {
                Console.SetCursorPosition(xIndent, yIndent);
                Console.WriteLine(new string(' ', options[i].Length + boxPadding));
                Console.SetCursorPosition(xIndent, yIndent + 1);
                Console.Write(new string(' ', 2 + options[i].Contents.Length + 2));
                Console.SetCursorPosition(xIndent, yIndent + 2);
                Console.Write(new string(' ', options[i].Length + boxPadding));

                xIndent += options[i].Length + boxPadding + spacing;
            }
            Console.SetCursorPosition(initX, initY);
        }
        else Console.SetCursorPosition(initX, initY);

        return choice;
    }

    public static void Title(params string[] titles)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.Clear();

        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine("");
        }

        foreach (var title in titles)
        {
            Console.CursorLeft = (Console.WindowWidth - title.Length) / 2;
            Console.WriteLine(title);
            Console.CursorTop++;
        }
    }

    public static async Task Narrate(string contents, int newLines = 1, int x = -1, int y = -1, int sleep = 84, ConsoleColor msgColor = ConsoleColor.Gray, ConsoleColor highlight = ConsoleColor.Black, bool skippable = false)
    {
        CancellationTokenSource cts = new();
        Task writer = Task.Run(async () =>
        {
            await Print(contents, newLines, x, y, sleep, msgColor, highlight, cts.Token);
        });

        if (!skippable)
        {
            await writer;
            cts.Dispose();
            return;
        }

        ClearKeyBuffer();
        while (!writer.IsCompleted)
        {
            await Task.Delay(15);

            while (Console.KeyAvailable)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Spacebar || key.Key == ConsoleKey.Enter)
                {
                    cts.Cancel();
                    break;
                }
            }
        }

        await writer; // must await to ensure the writer has noticed and written all newlines before we start writing new text
        cts.Dispose();
    }
}