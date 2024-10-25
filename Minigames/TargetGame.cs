using ManagedBass;
using AMysteriousVideogame.Audio;
using static System.Console;

namespace AMysteriousVideogame.Minigames;

internal static class TargetGame
{
    const int gameHeight = 20;
    const int gameWidth = 2 * gameHeight;
    const int winningScore = 10;

    static readonly object targetLock = new();

    public static async Task Play()
    {
        MediaPlayer ding = new(), miss = new(), nice1 = new(), nice2 = new(), tooslow = new(), win = new();
        await Task.WhenAll(ding.LoadAsync(@"SFX/Ding.mp3"), miss.LoadAsync(@"SFX/Miss.mp3"), nice1.LoadAsync(@"SFX/Nice1.mp3"), nice2.LoadAsync(@"SFX/Nice2.mp3"), tooslow.LoadAsync(@"SFX/TooSlow.mp3"), win.LoadAsync(@"SFX/Win.mp3"));

        int score = 0;
        int fails = 0;

        int playerX = gameWidth / 2;
        int playerY = gameHeight / 2;

        (int targetX, int targetY) = GenerateTargetPosition(playerX, playerY);

        bool niceSwitch = false;
        Game.DrawBorder(gameWidth, gameHeight);
        Game.DrawPlayer(playerX, playerY, gameWidth, gameHeight);
        DrawTarget(targetX, targetY);
        while (score < winningScore)
        {
            var oldX = playerX;
            var oldY = playerY;

            var key = ReadKey(true);
            switch (key.Key)
            {
                case ConsoleKey.LeftArrow:
                case ConsoleKey.A:
                    if (playerX > 0)
                        playerX--;
                    break;
                case ConsoleKey.RightArrow:
                case ConsoleKey.D:
                    if (playerX < gameWidth - 1)
                        playerX++;
                    break;
                case ConsoleKey.UpArrow:
                case ConsoleKey.W:
                    if (playerY > 0)
                        playerY--;
                    break;
                case ConsoleKey.DownArrow:
                case ConsoleKey.S:
                    if (playerY < gameHeight - 1)
                        playerY++;
                    break;
            }

            if (oldX != playerX || oldY != playerY)
            {
                Game.ErasePlayer(oldX, oldY, gameWidth, gameHeight);
                Game.DrawPlayer(playerX, playerY, gameWidth, gameHeight);
            }

            if (playerX == targetX && playerY == targetY)
            {
                lock (targetLock)
                {
                    ding.Stop(); ding.Play();
                    score++;
                    if (score < winningScore)
                    {
                        if (niceSwitch) { nice1.Stop(); nice1.Play(); }
                        else { nice2.Stop(); nice2.Play(); }
                        niceSwitch = !niceSwitch;
                        (targetX, targetY) = GenerateTargetPosition(playerX, playerY);
                        DrawTarget(targetX, targetY);
                    }
                }
                
                if (score < winningScore)
                {
                    var currentScore = score;
                    _ = Task.Delay(2000 + 500 * fails).ContinueWith((t) =>
                    {
                        if (currentScore == score)
                        {
                            lock (targetLock)
                            {
                                miss.Stop(); miss.Play();
                                tooslow.Stop(); tooslow.Play();

                                Game.ErasePlayer(targetX, targetY, gameWidth, gameHeight);
                                (targetX, targetY) = GenerateTargetPosition(playerX, playerY);
                                DrawTarget(targetX, targetY);

                                score = 0;
                                fails++;
                            }
                        }
                    });
                }
            }
        }

        _ = SoundPlayer.Play("SFX/BlockBreak.mp3");
        await SoundPlayer.Play(win);

        ding.Dispose();
        miss.Dispose();
        Clear();
    }

    static (int, int) GenerateTargetPosition(int playerX, int playerY)
    {
        int targetX, targetY;
        do
        {
            targetX = Random.Shared.Next(0, gameWidth);
            targetY = Random.Shared.Next(0, gameHeight);
        }
        while (targetX == playerX && targetY == playerY);
        
        return (targetX, targetY);
    }

    static void DrawTarget(int targetX, int targetY)
        => Game.DrawPlayer(targetX, targetY, gameWidth, gameHeight, '#', ConsoleColor.White);
}