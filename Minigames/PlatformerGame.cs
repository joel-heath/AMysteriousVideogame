using ManagedBass;
using static System.Console;

namespace AMysteriousVideogame.Minigames;

internal class PlatformerGame
{
    const int gameWidth = 80;
    const int gameHeight = 20;
    const int maxLives = 4;
    const int rounds = 3;

    //const double gravity = -0.2;
    //const double jumpPower = 2.5; // initial velocity upon jump

    const double gravity = -0.05;
    const double jumpPower = 1.5; // initial velocity upon jump

    //const int maxXSpeed = 2;
    const int maxYSpeed = 1; // (descending speed, the jump power can be greater)

    //const int xIncrement = 2;

    static readonly ConsoleColor[] mapColors = [ConsoleColor.Gray, ConsoleColor.Red, ConsoleColor.Green];

    static readonly MediaPlayer nice1 = new(), nice2 = new(), fall1 = new(), fall2 = new(), fall3 = new();
    static int niceIndex = 0;
    static int fallIndex = 0;

    public static async Task<bool> Play()
    {
        await Task.WhenAll(nice1.LoadAsync(@"SFX/Nice3.mp3"), nice2.LoadAsync(@"SFX/Nice4.mp3"), fall1.LoadAsync(@"SFX/Fall1.mp3"), fall2.LoadAsync(@"SFX/Fall2.mp3"), fall3.LoadAsync(@"SFX/Fall3.mp3"));

        bool won = true;
        for (int i = 0; i < rounds; i++)
        {
            // true means there is a wall, false means there is no wall
            bool[,] map = BitmapReader.Read($"Minigames/PlatformerMap{i+1}.bmp"); // (0, 0) is top-left corner, used for drawing
            if (!await PlayRound(map, mapColors[i]))
            {
                won = false;
                Clear();
                break;
            }
            if (niceIndex == 0)
            {
                nice1.Play();
                niceIndex++;
            }
            else
            {
                nice2.Play();
                niceIndex = 0;
            }
            Clear();
        }

        return won;
    }

    static async Task<bool> PlayRound(bool[,] drawingMap, ConsoleColor mapColor)
    {
        bool[,] map = VerticalFlip(drawingMap); // (0, 0) is bottom-left corner, used for collision detection

        int start = FindTopPlatform(map, true) + 1, end = FindTopPlatform(map, false) + 1;

        int x = 0;
        int y = start;
        int vx;
        double vy = 0;
        int lives = maxLives;

        ForegroundColor = ConsoleColor.White;
        Game.DrawBorder(gameWidth, gameHeight);
        ForegroundColor = mapColor;
        DrawMap(drawingMap);
        Game.DrawPlayer(x, gameHeight - y - 1, gameWidth, gameHeight);
        Game.DrawLives(lives, maxLives, gameWidth, gameHeight);
        while (lives > 0 && !(x == gameWidth - 1 && y == end))
        {
            if (y == 0)
            {
                Game.ErasePlayer(x, gameHeight - y - 1, gameWidth, gameHeight);
                if (fallIndex == 0)
                {
                    fall1.Play();
                    fallIndex++;
                }
                else if (fallIndex == 1)
                {
                    fall2.Play();
                    fallIndex++;
                }
                else
                {
                    fall3.Play();
                    fallIndex = 0;
                }

                lives--;
                vy = 0;
                x = 0;
                y = start;
                Game.DrawLives(lives, maxLives, gameWidth, gameHeight);
                await Task.Delay(1000);
                Game.DrawPlayer(x, gameHeight - y - 1, gameWidth, gameHeight);
                ConsoleUtils.ClearKeyBuffer();
            }

            int oldX = x, oldY = y;

            HeldKeys heldKeys = 0;
            if (KeyAvailable)
            {
                var key = ReadKey(true).Key;
                if ((key == ConsoleKey.Spacebar || key == ConsoleKey.W || key == ConsoleKey.UpArrow) && (y == 0 || map[x, y - 1]))
                    heldKeys |= HeldKeys.Spacebar;
                else if (key == ConsoleKey.A)
                    heldKeys |= HeldKeys.BigLeft;
                else if (key == ConsoleKey.LeftArrow)
                    heldKeys |= HeldKeys.Left;
                else if (key == ConsoleKey.D)
                    heldKeys |= HeldKeys.BigRight;
                else if (key == ConsoleKey.RightArrow)
                    heldKeys |= HeldKeys.Right;
            }
                //ConsoleUtils.ClearKeyBuffer();

            if (heldKeys.HasFlag(HeldKeys.Spacebar))
            {
                vy = jumpPower;
            }
            if (heldKeys.HasFlag(HeldKeys.BigLeft))
            {
                vx = -2;
                //if (vx > -maxXSpeed)
                //vx -= xIncrement;
            }
            else if (heldKeys.HasFlag(HeldKeys.Left))
            {
                vx = -1;
            }
            else if (heldKeys.HasFlag(HeldKeys.BigRight))
            {
                vx = 2;
                //if (vx < maxXSpeed)
                //    vx += xIncrement;
            }
            else if (heldKeys.HasFlag(HeldKeys.Right))
            {
                vx = 1;
            }
            else vx = 0;
            /*else
            {
                if (vx > 0)
                    vx -= 1;
                else if (vx < 0)
                    vx += 1;
            }*/

            // x collision detection

            if (vx < 0) // leftward collision detection
            {
                for (int i = 0; i >= vx; i--)
                {
                    if (x + i - 1 < 0 || map[x + i - 1, y])
                    {
                        vx = 0;
                        x += i;
                        break;
                    }
                }
                if (vx < 0) x += vx;
            }
            else if (vx > 0) // rightward collision detection
            {
                for (int i = 0; i <= vx; i++)
                {
                    if (x + i + 1 == gameWidth || map[x + i + 1, y])
                    {
                        vx = 0;
                        x += i;
                        break;
                    }
                }
                if (vx > 0) x += vx;
            }


            /*
            if (x + vx < 0)
            {
                x = 0;
                //if (vx < 0) vx = 0;
            }
            else if (x + vx >= gameWidth)
            {
                x = gameWidth - 1;
                //if (vx > 0) vx = 0;
            }
            else
            {
                x += vx;
            }*/

            if (y > 0 && !map[x, y - 1]) // gravity
            {
                if (vy > -maxYSpeed)
                    vy += gravity;
            }

            if (vy < 0) // falling down collision detection
            {
                for (int i = 0; i >= vy; i--)
                {
                    if (y + i - 1 < 0 || map[x, y + i - 1])
                    {
                        vy = 0;
                        y += i;
                        break;
                    }
                }
                if (vy < 0) y += (int)Math.Round(vy, MidpointRounding.ToZero);
            }
            else if (vy > 0) // jumping up collision detection
            {
                for (int i = 0; i <= vy; i++)
                {
                    if (y + i + 1 == gameHeight || map[x, y + i + 1])
                    {
                        vy = 0;
                        y += i;
                        break;
                    }
                }
                if (vy > 0) y += (int)Math.Round(vy, MidpointRounding.ToZero);
            }

            if (oldX != x || oldY != y)
            {
                Game.ErasePlayer(oldX, gameHeight - oldY - 1, gameWidth, gameHeight);
                Game.DrawPlayer(x, gameHeight - y - 1, gameWidth, gameHeight);
            }

            await Task.Delay(1000 / 60);
        }

        return lives > 0;
    }

    static int FindTopPlatform(bool[,] map, bool start)
    {
        for (int y = gameHeight - 1; y >= 0; y--)
        {
            if (map[start ? 0 : gameWidth - 1, y])
            {
                return y;
            }
        }
        return -1;
    }

    static bool[,] VerticalFlip(bool[,] map)
    {
        var newMap = new bool[gameWidth, gameHeight];

        for (int x = 0; x < gameWidth; x++)
        {
            for (int y = 0; y < gameHeight; y++)
            {
                newMap[x, y] = map[x, gameHeight - y - 1];
            }
        }

        return newMap;
    }

    static void DrawMap(bool[,] map)
    {
        var leftPadding = (WindowWidth - gameWidth) / 2;
        CursorTop = (WindowHeight - gameHeight) / 2;

        CursorLeft = leftPadding;
        for (int y = 0; y < gameHeight; y++)
        {
            for (int x = 0; x < gameWidth; x++)
            {
                Write(map[x, y] ? '#' : ' ');
            }
            CursorTop++;
            CursorLeft = leftPadding;
        }
    }

    [Flags] enum HeldKeys { Spacebar = 1, Left = 2, Right = 4, BigLeft = 8, BigRight = 16 }
}