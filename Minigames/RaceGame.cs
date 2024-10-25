using AMysteriousVideogame.Audio;
using ManagedBass;
using static System.Console;

namespace AMysteriousVideogame.Minigames;

internal static class RaceGame
{
    const int gameWidth = 80;
    const int gameHeight = 20;
    const int playerY = gameHeight / 3;
    const int guardY = gameHeight * 2 / 3;

    const int tickRate = 60; // ticks per second
    const int msPerTick = 1000 / tickRate;
    static int guardSpeed = 4; // blocks per second
    const int guardSpeedUpTime = 550;
    const int guardSpeedUpTicks = guardSpeedUpTime * tickRate / 1000; // ticks before guard speeds up

    const int totalLengths = 1;

    public static async Task<bool> Play()
    {
        MediaPlayer cheering = new();
        await cheering.LoadAsync(@"SFX/CheeringOn.mp3");
        cheering.Loop = true;
        cheering.Play();

        guardSpeed = 4;
        int lengths = 0, guardLengths = 0;
        int playerX = 0, guardX = 0;
        bool leftToRight = true, guardLeftToRight = true;

        Game.DrawBorder(gameWidth, gameHeight);
        Game.DrawPlayer(playerX, playerY, gameWidth, gameHeight);
        Game.DrawPlayer(guardX, guardY, gameWidth, gameHeight, color: ConsoleColor.Red);

        int ticksToGuardMove = tickRate / guardSpeed;
        int guardMoveIndex = 1;
        int guradSpeedUpIndex = 1;
        bool keySwitch = false;

        while (lengths < totalLengths && guardLengths < totalLengths)
        {
            var oldX = playerX;

            bool move = false;
            while (KeyAvailable)
            {
                var key = ReadKey(true).Key;
                if (key == (keySwitch ? ConsoleKey.LeftArrow : ConsoleKey.RightArrow) || key == (keySwitch ? ConsoleKey.A : ConsoleKey.D))
                {
                    move = true;
                    keySwitch = !keySwitch;
                }
            }
            if (move)
            {
                if (leftToRight)
                {
                    if (playerX == gameWidth - 1)
                    {
                        lengths++;
                        leftToRight = false;
                    }
                    else
                    {
                        playerX++;
                    }
                }
                else
                {
                    if (playerX == 0)
                    {
                        lengths++;
                        leftToRight = true;
                    }
                    else
                    {
                        playerX--;
                    }
                }

                Game.ErasePlayer(oldX, playerY, gameWidth, gameHeight);
                Game.DrawPlayer(playerX, playerY, gameWidth, gameHeight);
            }

            if (guardMoveIndex >= ticksToGuardMove)
            {
                guardMoveIndex = 1;

                Game.ErasePlayer(guardX, guardY, gameWidth, gameHeight);

                if (guardLeftToRight)
                {
                    if (guardX == gameWidth - 1)
                    {
                        guardLengths++;
                        guardLeftToRight = false;
                    }
                    else
                    {
                        guardX++;
                    }
                }
                else
                {
                    if (guardX == 0)
                    {
                        guardLengths++;
                        guardLeftToRight = true;
                    }
                    else
                    {
                        guardX--;
                    }
                }

                Game.DrawPlayer(guardX, guardY, gameWidth, gameHeight, color: ConsoleColor.Red);
            }
            else guardMoveIndex++;

            if (guradSpeedUpIndex == guardSpeedUpTicks)
            {
                guradSpeedUpIndex = 1;
                guardSpeed++;
                ticksToGuardMove = Math.Max(tickRate / guardSpeed, 1);
            }
            else guradSpeedUpIndex++;

            await Task.Delay(msPerTick);
        }

        cheering.Stop();
        _ = SoundPlayer.Play(@"SFX/Whistle.mp3");

        if (lengths == totalLengths)
        {
            await SoundPlayer.Play(@"SFX/WinRace.mp3");
        }
        else
        {
            await SoundPlayer.Play(@"SFX/Lose.mp3");
        }

        Clear();
        return lengths == totalLengths;
    }
}