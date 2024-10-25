using ManagedBass;
using System.ComponentModel;

namespace AMysteriousVideogame;

internal class Person(string name, ConsoleColor color, bool dialogueSkippable = false)
{
    public string Name { get; set; } = name;
    public ConsoleColor Color { get; set; } = color;
    public bool DialogueSkippable { get; set; } = dialogueSkippable;

    public async Task Say(string contents, int audio, bool printName = true, int newLines = 1, int x = -1, int y = -1, ConsoleColor highlight = ConsoleColor.Black, int sleep = 55)
        => await Say(contents, $"Voice/{Name}/{audio}.mp3", printName, newLines, x, y, highlight, sleep);

    public async Task Say(string contents, string audio, bool printName = true, int newLines = 1, int x = -1, int y = -1, ConsoleColor highlight = ConsoleColor.Black, int sleep = 55)
    {
        MediaPlayer player = new();
        if (!await player.LoadAsync(audio))
        {
            player.Dispose();
            throw new FileNotFoundException($"Failed to play media");
        };
        player.Volume = 2;
        player.Play();

        TaskCompletionSource audioTCS = new(); // TaskCreationOptions.RunContinuationsAsynchronously

        void Handler(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(player.State) && player.State == PlaybackState.Stopped)
            {
                player.PropertyChanged -= Handler; // Unsubscribe before disposing
                audioTCS.SetResult();
                player.Dispose();
            }
        }

        player.PropertyChanged += Handler;

        CancellationTokenSource cts = new();
        Task writer = Task.Run(async () =>
        {
            if (printName)
            {
                await ConsoleUtils.Write(Name, 0, Color, highlight, x, y, -1);
                await ConsoleUtils.Write(": ", 0, ConsoleColor.White, highlight, x, y, -1);
            }

            await ConsoleUtils.Print(contents, newLines, x, y, sleep, ConsoleColor.White, highlight, cts.Token);
        });

        if (!DialogueSkippable)
        {
            await Task.WhenAll(writer, audioTCS.Task);
            cts.Dispose();
            audioTCS.Task.Dispose();
            return;
        }

        ConsoleUtils.ClearKeyBuffer();
        while (!writer.IsCompleted || !audioTCS.Task.IsCompleted)
        {
            await Task.Delay(15);

            while (Console.KeyAvailable)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Spacebar || key.Key == ConsoleKey.Enter)
                {
                    player.Stop();
                    player.Dispose();
                    cts.Cancel();
                    break;
                }
            }
        }

        await writer; // must await to ensure the writer has noticed and written all newlines before we start writing new text
        cts.Dispose();
        if (!audioTCS.Task.IsCompleted) audioTCS.SetResult();
        await audioTCS.Task;
        audioTCS.Task.Dispose();
    }

    public async Task Say(string contents, bool printName = true, int newLines = 1, int x = -1, int y = -1, ConsoleColor highlight = ConsoleColor.Black, int sleep = 55)
    {
        CancellationTokenSource cts = new();
        Task writer = Task.Run(async () =>
        {
            if (printName)
            {
                await ConsoleUtils.Write(Name, 0, Color, highlight, x, y, -1);
                await ConsoleUtils.Write(": ", 0, ConsoleColor.White, highlight, x, y, -1);
            }

            await ConsoleUtils.Print(contents, newLines, x, y, sleep, ConsoleColor.White, highlight, cts.Token);
        });

        if (!DialogueSkippable)
        {
            await writer;
            cts.Dispose();
            return;
        }

        ConsoleUtils.ClearKeyBuffer();
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