using AMysteriousVideogame.Audio;
using ManagedBass;
using static System.Console;

namespace AMysteriousVideogame.Minigames;

internal static class FireGame
{
    const int gameWidth = 80;
    const int gameHeight = 20;
    const int maxLives = 4;
    const int rounds = 15;

    const int iframesTime = 750; // milliseconds

    const int startingTickSpeed = 50; // milliseconds per tick
    const int finalTickSpeed = 20; // milliseconds per tick
    const int fireSpeed = 500; // milliseconds per frame of fire
    const int fireMultiplier = 10; // The amount the fire speeds up when you are inside it
    //const int fireCount = fireSpeed / tickSpeed; // number of ticks per frame of fire
    const int doorwayDistance = 40; // distance between doorways
    const int doorwayWidth = 9; // width of doorways
    const int minDoorwayLength = 2; // minimum length of doorways
    const int maxDoorwayLength = 4; // max length of doorways

    const int fireWidth = 13;
    const int intendedPlayerX = fireWidth + 2;

    public static async Task<bool> Play()
    {
        MediaPlayer fireWooshMP = new();
        await fireWooshMP.LoadAsync(@"SFX/FireWoosh.mp3");
        MediaPlayer ouch1 = new();
        await ouch1.LoadAsync(@"SFX/Ouch1.mp3");
        MediaPlayer ouch2 = new();
        await ouch2.LoadAsync(@"SFX/Ouch2.mp3");
        MediaPlayer death = new();
        await death.LoadAsync(@"SFX/Ouch3.mp3");

        char[] fireChars = [' ', '.', ',', '#', '-', '_', '+', '='];
        ConsoleColor[] fireColors = [ConsoleColor.Red, ConsoleColor.DarkRed, ConsoleColor.Yellow, ConsoleColor.DarkYellow];

        List<(int start, int length, int x, int width)> doorways = [];
        List<(int width, int height, int x, int y)> obstacles = [];

        //SetWindowSize(gameWidth, gameHeight);

        var currentTickSpeed = startingTickSpeed;
        var tickSpeedDelta = (startingTickSpeed - finalTickSpeed) / rounds;
        int round = 0;

        int playerX = intendedPlayerX; // player can get pushed back by obstacles
        int playerY = gameHeight / 2;
        int fireIndex = fireSpeed / startingTickSpeed;
        int doorwayIndex = doorwayDistance;
        int ouchIndex = 0;

        int lives = maxLives;
        int framesSinceLastHit = 0; // 1 second iframes (adapts to tick speed)

        ForegroundColor = ConsoleColor.White;
        Game.DrawBorder(gameWidth, gameHeight);
        Game.DrawLives(lives, maxLives, gameWidth, gameHeight);
        while (lives > 0 && (round < rounds || obstacles.Count > 0 || doorways.Count > 0))
        {
            if (fireIndex >= fireSpeed / startingTickSpeed)
            {
                DrawFire(fireChars, fireColors, playerX == fireWidth ? playerY : -1);
                fireIndex = 0;
            }

            if (KeyAvailable)
            {
                var key = ReadKey().Key;
                ConsoleUtils.ClearKeyBuffer();

                if ((key == ConsoleKey.UpArrow || key == ConsoleKey.W) && playerY > 0)
                {
                    if (!CheckCollision(playerX, playerY - 1, doorways, obstacles))
                    {
                        Game.ErasePlayer(playerX, playerY, gameWidth, gameHeight);
                        playerY--;
                    }
                }
                else if ((key == ConsoleKey.DownArrow || key == ConsoleKey.S) && playerY < gameHeight - 1)
                {
                    if (!CheckCollision(playerX, playerY + 1, doorways, obstacles))
                    {
                        Game.ErasePlayer(playerX, playerY, gameWidth, gameHeight);
                        playerY++;
                    }
                }
            }

            if (playerX < intendedPlayerX)
            {
                playerX = ManageHorizontalCollision(playerX, playerY, doorways, obstacles, true);
                fireIndex += fireMultiplier - 1;
            }

            doorways = DrawDoorways(doorways);
            obstacles = DrawObstacles(obstacles);

            playerX = ManageHorizontalCollision(playerX, playerY, doorways, obstacles, false);

            if (playerX == intendedPlayerX - 3)
            {
                if (framesSinceLastHit > iframesTime / currentTickSpeed) 
                {
                    framesSinceLastHit = 0;

                    fireWooshMP.Stop();
                    fireWooshMP.Play();

                    if (lives == 1)
                    {
                        death.Stop();
                        death.Play();
                    }
                    else if (ouchIndex == 0)
                    {
                        ouch1.Stop();
                        ouch1.Play();
                    }
                    else
                    {
                        ouch2.Stop();
                        ouch2.Play();
                    }
                    ouchIndex = (ouchIndex + 1) % 2;
                    lives--;
                    Game.DrawLives(lives, maxLives, gameWidth, gameHeight);
                }
            }
            else framesSinceLastHit++;

            Game.DrawPlayer(playerX, playerY, gameWidth, gameHeight);

            if (round < rounds)
            {
                if (doorwayIndex >= doorwayDistance)
                {
                    int length = Random.Shared.Next(minDoorwayLength, maxDoorwayLength + 1);
                    int start = Random.Shared.Next(2, gameHeight - length - 2);
                    doorways.Add((start, length, gameWidth - 1, doorwayWidth));
                    doorwayIndex = 0;

                    round++;
                    currentTickSpeed -= tickSpeedDelta;
                }
                else if (doorwayIndex == 2 * doorwayDistance / 3 || doorwayIndex == doorwayDistance / 3)
                {
                    int width = Random.Shared.Next(1, 5);
                    int height = Random.Shared.Next(1, 5);
                    int x = gameWidth - 1;
                    int y = Random.Shared.Next(0, gameHeight - height - 1);

                    obstacles.Add((width, height, x, y));
                }
            }

            fireIndex++;
            doorwayIndex++;
            await Task.Delay(currentTickSpeed);
        }

        if (lives == 0)
        {
            await DeathAnimation(fireChars, fireColors, playerX, playerY);
            await MusicPlayer.FadeOut(2000);
            await Task.Delay(4000);
        }
        else
        {
            await MusicPlayer.FadeOut(2000);
            Clear();
        }

        fireWooshMP.Dispose();
        ouch1.Dispose();
        ouch2.Dispose();
        death.Dispose();

        return lives > 0;
    }

    static async Task DeathAnimation(char[] fireChars, ConsoleColor[] fireColors, int playerX, int playerY)
    {
        for (int t = 1; t <= fireWidth; t++)
        {
            var leftPadding = (WindowWidth - gameWidth) / 2;
            CursorTop = (WindowHeight - gameHeight) / 2;

            for (int i = 0; i < gameHeight; i++)
            {
                CursorLeft = leftPadding;
                int length = Random.Shared.Next(t, fireWidth + 1);
                for (int j = 0; j < length; j++)
                {
                    ForegroundColor = fireColors[Random.Shared.Next(0, fireColors.Length)];
                    Write(fireChars[Random.Shared.Next(0, fireChars.Length)]);
                }
                Write(new string(' ', fireWidth - length)); // clear old fire
                CursorTop++;
            }
            Game.DrawPlayer(playerX, playerY, gameWidth, gameHeight);
            await Task.Delay(50);
        }
    }

    static bool CheckCollision(int playerX, int playerY, List<(int start, int length, int x, int width)> doorways, List<(int width, int height, int x, int y)> obstacles)
        => CheckCollisionDoorways(playerX, playerY, doorways) || CheckCollisionObstacles(playerX, playerY, obstacles);

    static bool CheckCollisionDoorways(int playerX, int playerY, List<(int start, int length, int x, int width)> doorways)
        => doorways.Any(d => d.x <= playerX && playerX <= d.x + d.width && (playerY < d.start || playerY >= d.start + d.length));

    static bool CheckCollisionObstacles(int playerX, int playerY, List<(int width, int height, int x, int y)> obstacles)
        => obstacles.Any(o => o.x <= playerX && playerX < o.x + o.width && o.y <= playerY && playerY < o.y + o.height);

    static int ManageHorizontalCollision(int playerX, int playerY, List<(int start, int length, int x, int width)> doorways, List<(int width, int height, int x, int y)> obstacles, bool @else)
    {
        if (CheckCollision(playerX, playerY, doorways, obstacles))
        {
            Game.ErasePlayer(playerX, playerY, gameWidth, gameHeight);
            playerX--;
        }
        else if (@else)
        {
            Game.ErasePlayer(playerX, playerY, gameWidth, gameHeight);
            playerX++;
            playerX = ManageHorizontalCollision(playerX, playerY, doorways, obstacles, false);
        }
        return playerX;
    }

    static void DrawFire(char[] fireChars, ConsoleColor[] fireColors, int playerY)
    {
        var leftPadding = (WindowWidth - gameWidth) / 2;
        CursorTop = (WindowHeight - gameHeight) / 2;

        for (int i = 0; i < gameHeight; i++)
        {
            CursorLeft = leftPadding;
            int length = Random.Shared.Next(1, fireWidth + 1);
            for (int j = 0; j < length; j++)
            {
                ForegroundColor = fireColors[Random.Shared.Next(0, fireColors.Length)];
                Write(fireChars[Random.Shared.Next(0, fireChars.Length)]);
            }
            Write(new string(' ', fireWidth - length - (i == playerY ? 1 : 0))); // clear old fire
            CursorTop++;
        }
    }
    /*
    static void ClearFire()
    {
        var leftPadding = (WindowWidth - gameWidth) / 2;
        CursorTop = (WindowHeight - gameHeight) / 2;

        for (int i = 0; i < gameHeight; i++)
        {
            CursorLeft = leftPadding;
            Write(new string(' ', fireWidth));
            CursorTop++;
        }
    }*/

    static List<(int start, int length, int x, int width)> DrawDoorways(List<(int start, int length, int x, int width)> doorways)
    {
        List<(int start, int length, int x, int width)> newDoorways = [];

        var leftPadding = (WindowWidth - gameWidth) / 2;
        var topPadding = (WindowHeight - gameHeight) / 2;

        for (int i = 0; i < doorways.Count; i++)
        {
            var (start, length, x, width) = doorways[i];
            var drawableWidth = Math.Min(width, gameWidth - x);
            var wall = new string('#', drawableWidth);

            SetCursorPosition(leftPadding + x, topPadding);

            // remove old wall
            if (x < gameWidth - doorwayWidth)
            {
                CursorLeft += width;

                for (int j = 0; j < gameHeight; j++)
                {
                    Write(' ');
                    CursorTop++;
                    CursorLeft--;
                }

                CursorLeft -= width;
                CursorTop = topPadding;
            }

            if (width == 0) continue; // need to keep 0 width doorways to remove them

            // draw new wall

            if (x == intendedPlayerX - 2)
            {
                ForegroundColor = ConsoleColor.DarkGray;
                DrawWallLayer(start, length);
                CursorTop = topPadding;

                if (width >= 2)
                {
                    CursorLeft++;
                    ForegroundColor = ConsoleColor.Gray;
                    DrawWallLayer(start, length);
                    CursorTop = topPadding;

                    if (width >= 3)
                    {
                        CursorLeft++;
                        ForegroundColor = ConsoleColor.White;
                        DrawWallLayer(start, length, new string('#', drawableWidth - 2));
                        CursorTop = topPadding;
                    }
                }

                x += 1;
                width -= 1;
            }
            else if (x == intendedPlayerX - 1)
            {
                ForegroundColor = ConsoleColor.Gray;
                DrawWallLayer(start, length);
                CursorTop = topPadding;

                if (width >= 2)
                {
                    ForegroundColor = ConsoleColor.White;
                    DrawWallLayer(start, length, new string('#', drawableWidth - 1));
                    CursorTop = topPadding;
                }
            }
            else
            {
                ForegroundColor = ConsoleColor.White;
                DrawWallLayer(start, length, wall);
            }

            if (width >= 0)
            {
                newDoorways.Add((start, length, x - 1, width));
            }
        }

        return newDoorways;
    }

    static void DrawWallLayer(int start, int length, string wall = "#")
    {
        for (int j = 0; j < start; j++)
        {
            Write(wall);
            CursorTop++;
            CursorLeft -= wall.Length;
        }

        CursorTop += length;
        for (int j = start + length; j < gameHeight; j++)
        {
            Write(wall);
            CursorTop++;
            CursorLeft -= wall.Length;
        }
    }

    static void DrawObstacleLayer(int y, int height, string wall = "#")
    {
        for (int j = y; j < y + height; j++)
        {
            Write(wall);
            CursorTop++;
            CursorLeft -= wall.Length;
        }
    }

    static List<(int width, int height, int x, int y)> DrawObstacles(List<(int width, int height, int x, int y)> obstacles)
    {
        List<(int width, int height, int x, int y)> newObstacles = [];

        var leftPadding = (WindowWidth - gameWidth) / 2;
        var topPadding = (WindowHeight - gameHeight) / 2;

        for (int i = 0; i < obstacles.Count; i++)
        {
            var (width, height, x, y) = obstacles[i];
            var drawableWidth = Math.Min(width, gameWidth - x);
            var hLayer = new string('#', drawableWidth);

            SetCursorPosition(leftPadding + x, topPadding + y);

            // remove old vLayer

            if (x < gameWidth - width)
            {
                CursorLeft += width;

                DrawObstacleLayer(y, height, " ");

                CursorLeft -= width;
                CursorTop = topPadding + y;
            }

            if (width == 0) continue; // need to keep 0 width doorways to remove them

            // draw new obstacles

            if (x == intendedPlayerX - 2)
            {
                ForegroundColor = ConsoleColor.DarkGray;
                DrawObstacleLayer(y, height);
                CursorTop = topPadding + y;

                if (width >= 2)
                {
                    CursorLeft++;
                    ForegroundColor = ConsoleColor.Gray;
                    DrawObstacleLayer(y, height);
                    CursorTop = topPadding + y;

                    if (width >= 3)
                    {
                        CursorLeft++;
                        ForegroundColor = ConsoleColor.White;
                        DrawObstacleLayer(y, height, new string('#', drawableWidth - 2));
                        CursorTop = topPadding + y;
                    }
                }

                x += 1;
                width -= 1;
            }
            else if (x == intendedPlayerX - 1)
            {
                ForegroundColor = ConsoleColor.Gray;
                DrawObstacleLayer(y, height);
                CursorTop = topPadding + y;

                if (width >= 2)
                {
                    ForegroundColor = ConsoleColor.White;
                    DrawObstacleLayer(y, height, new string('#', drawableWidth - 1));
                    CursorTop = topPadding + y;
                }
            }
            else
            {
                ForegroundColor = ConsoleColor.White;
                DrawObstacleLayer(y, height, hLayer);
            }

            if (width >= 0)
            {
                newObstacles.Add((width, height, x - 1, y));
            }
        }

        return newObstacles;
    }
}