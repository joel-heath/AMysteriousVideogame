using static System.Console;

namespace AMysteriousVideogame.Minigames;

internal static class Game
{
    public static void DrawBorder(int gameWidth, int gameHeight, char hChar = '=', string vBorder = "|", bool mazeExit = false, bool threeQuarters = false, int absYOffset = 0, bool fill = false)
    {
        var leftPadding = (WindowWidth - gameWidth) / 2;
        var topPadding = (threeQuarters ? 3 : 2) * (WindowHeight - gameHeight) / 4 + absYOffset;

        SetCursorPosition(leftPadding - vBorder.Length, topPadding - 1);
        Write(new string(hChar, gameWidth + 2 * vBorder.Length));
        for (int i = 0; i < gameHeight; i++)
        {
            SetCursorPosition(leftPadding - vBorder.Length, topPadding + i);
            if (fill)
            {
                Write(vBorder + new string(' ', gameWidth) + vBorder);
            }
            else
            {
                Write(vBorder);
                if (!mazeExit || i != gameHeight - 1)
                {
                    SetCursorPosition(leftPadding + gameWidth, topPadding + i);
                    Write(vBorder);
                }
            }
        }

        SetCursorPosition(leftPadding - vBorder.Length, topPadding + gameHeight);
        Write(new string(hChar, gameWidth + 2 * vBorder.Length));
    }

    public static void DrawPlayer(int playerX, int playerY, int gameWidth, int gameHeight, char character = 'O', ConsoleColor color = ConsoleColor.Cyan)
    {
        var leftPadding = (WindowWidth - gameWidth) / 2;
        var topPadding = (WindowHeight - gameHeight) / 2;

        SetCursorPosition(leftPadding + playerX, topPadding + playerY);
        ForegroundColor = color;
        Write(character);
    }

    public static void ErasePlayer(int playerX, int playerY, int gameWidth, int gameHeight)
    {
        var leftPadding = (WindowWidth - gameWidth) / 2;
        var topPadding = (WindowHeight - gameHeight) / 2;

        SetCursorPosition(leftPadding + playerX, topPadding + playerY);
        Write(" ");
    }

    public static void DrawLives(int lives, int maxLives, int gameWidth, int gameHeight)
    {
        var leftPadding = (WindowWidth - gameWidth) / 2;
        var topPadding = (WindowHeight - gameHeight) / 2;

        SetCursorPosition(leftPadding, topPadding + gameHeight + 2);
        ForegroundColor = ConsoleColor.White;
        Write($"Lives: [");
        ForegroundColor = ConsoleColor.Red;
        Write(new string('o', lives));
        ForegroundColor = ConsoleColor.DarkGray;
        Write(new string('x', maxLives - lives));
        ForegroundColor = ConsoleColor.White;
        Write(']');
    }

}