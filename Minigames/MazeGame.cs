using AMysteriousVideogame.Audio;
using static System.Console;

namespace AMysteriousVideogame.Minigames;

internal static class MazeGame
{
    const int gameHeight = 21; // must be an odd number
    const int gameWidth = gameHeight;
    const int renderedGameWidth = gameWidth * 2;

    public static async Task Play()
    {
        var maze = GenerateMaze();

        //Game.DrawBorder(renderedGameWidth, gameHeight, '#', "##");
        Game.DrawBorder(renderedGameWidth, gameHeight, mazeExit: true);
        DrawMaze(maze);

        int playerX = 0;
        int playerY = 0;

        Game.DrawPlayer(playerX, playerY, renderedGameWidth, gameHeight);

        while (playerX < 2 * gameWidth)
        {
            var key = ReadKey(true).Key;
            Game.ErasePlayer(playerX, playerY, renderedGameWidth, gameHeight);

            switch (key)
            {
                case ConsoleKey.W:
                case ConsoleKey.UpArrow:
                    if (playerY > 0 && !maze[playerX / 2, playerY - 1].IsWall)
                        playerY--;
                    break;
                case ConsoleKey.S:
                case ConsoleKey.DownArrow:
                    if (playerY < gameHeight - 1 && !maze[playerX / 2, playerY + 1].IsWall)
                        playerY++;
                    break;
                case ConsoleKey.A:
                case ConsoleKey.LeftArrow:
                    if (playerX > 0 && !maze[(playerX - 1) / 2, playerY].IsWall)
                        playerX--;
                    break;
                case ConsoleKey.D:
                case ConsoleKey.RightArrow:
                    if (playerX == 2 * gameWidth - 1 && playerY == gameHeight - 1 || playerX < 2 * gameWidth - 1 && !maze[(playerX + 1) / 2, playerY].IsWall)
                        playerX++;
                    break;
            }

            Game.DrawPlayer(playerX, playerY, renderedGameWidth, gameHeight);
        }


        await SoundPlayer.Play("SFX/Chahoo.mp3");
        Clear();
    }

    static void DrawMaze(Cell[,] maze)
    {
        var leftPadding = (WindowWidth - renderedGameWidth) / 2;
        CursorTop = (WindowHeight - gameHeight) / 2;

        CursorLeft = leftPadding;
        for (int y = 0; y < gameHeight; y++)
        {
            for (int x = 0; x < gameWidth; x++)
            {
                Write(maze[x, y]);
            }
            CursorTop++;
            CursorLeft = leftPadding;
        }
    }

    static Cell[,] GenerateMaze()
    {
        var maze = new Cell[gameWidth, gameHeight];

        for (int y = 0; y < gameHeight; y++)
        {
            for (int x = 0; x < gameWidth; x++)
            {
                maze[x, y] = new Cell(x, y, y % 2 == 1 || x % 2 == 1);
            }
        }

        maze[0, 0].Visited = true;

        var stack = new Stack<Cell>();
        stack.Push(maze[0, 0]);

        while (stack.TryPop(out var current))
        {
            var neighbors = GetNeighbors(current, maze).ToList();

            if (neighbors.Count > 0)
            {
                stack.Push(current);

                var next = neighbors[Random.Shared.Next(neighbors.Count)];
                RemoveWall(current, next, maze);
                next.Visited = true;
                stack.Push(next);
            }
        }

        return maze;
    }

    static IEnumerable<Cell> GetNeighbors(Cell cell, Cell[,] maze)
    {
        if (cell.X > 1 && !maze[cell.X - 2, cell.Y].Visited)
            yield return maze[cell.X - 2, cell.Y];
        if (cell.X < gameWidth - 2 && !maze[cell.X + 2, cell.Y].Visited)
            yield return maze[cell.X + 2, cell.Y];
        if (cell.Y > 1 && !maze[cell.X, cell.Y - 2].Visited)
            yield return maze[cell.X, cell.Y - 2];
        if (cell.Y < gameHeight - 2 && !maze[cell.X, cell.Y + 2].Visited)
            yield return maze[cell.X, cell.Y + 2];
    }

    static void RemoveWall(Cell current, Cell next, Cell[,] maze)
    {
        if (current.X == next.X)
        {
            maze[current.X, (current.Y + next.Y) / 2].IsWall = false;
        }
        else
        {
            maze[(current.X + next.X) / 2, current.Y].IsWall = false;
        }
    }

    class Cell(int x, int y, bool isWall)
    {
        public int X { get; } = x;
        public int Y { get; } = y;
        public bool IsWall { get; set; } = isWall;
        public bool Visited { get; set; }

        public override string ToString() => IsWall ? "##" : "  ";
    }
}
