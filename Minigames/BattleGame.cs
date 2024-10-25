/// heavy inspiration from Toby Fox's "Undertale"

using AMysteriousVideogame.Audio;
using static System.Console;

namespace AMysteriousVideogame.Minigames;

internal class BattleGame
{
    const int enemyMaxHP = 1000;
    const int playerMaxHP = 40;
    const int pelletDamage = 2;

    const int talkCount = 3; // number of times to talk to each group to pacify them

    static readonly string[] fullEnemy = @"
                    _                                                   . _`  
                   / \  _===_                                            * -  
       __       __|>_<|_|,_,|            __       __ \_|_/             /- * `|
|_|_| /  \ |_| /  \    |\   |\         _|  |_    /  |_ |            ##   \^/  
  |\ | >_<| | | *_*|  ~|||  |_|      __|-_- |   |>_< | |  /\/\     #####  |   
  | \|    |/|/|    |\  ||v /  \    __##__   | </|    | | |>_< |   |o_o #  |   
  |  |----| | |----|---|| | >_<|   |>_< | </|\  |----| |/|    |\ /|    |\ |   
  |  |    | | |    |   ||/|    |   |    |\  | \ |    | | |----|   |----| \|   
  |  |----| | |----|---|/ |----|\ /|----| \ |   |----| | |    |   |    |  |   
".Trim('\r', '\n').Split(Environment.NewLine); // courtesy of chatGPT for a pretty terrible starting position, and my own creativity

    static readonly string[] thieves = @"
                    _                                                         
                   / \  _===_                                                 
       __       __|>_<|_|,_,|                                                 
|_|_| /  \ |_| /  \    |\   |\                                                
  |\ | >_<| | | *_*|  ~|||  |_|                                               
  | \|    |/|/|    |\  ||v /  \                                               
  |  |----| | |----|---|| | >_<|                                              
  |  |    | | |    |   ||/|    |                                              
  |  |----| | |----|---|/ |----|\                                             
".Trim('\r', '\n').Split(Environment.NewLine);

    static readonly string[] crooks = @"
                                                                        . _`  
                                                                         * -  
                                         __       __ \_|_/             /- * `|
                                       _|  |_    /  |_ |            ##   \^/  
                                     __|-_- |   |>_< | |  /\/\     #####  |   
                                   __##__   | </|    | | |>_< |   |o_o #  |   
                                   |>_< | </|\  |----| |/|    |\ /|    |\ |   
                                   |    |\  | \ |    | | |----|   |----| \|   
                                  /|----| \ |   |----| | |    |   |    |  |    
".Trim('\r', '\n').Split(Environment.NewLine);

    static readonly string[] damageBar = @"

  ██████████ ██████████  
██ ███ ███ █ █ ███ ███ ██
  ██████████ ██████████  

".Trim('\r', '\n').Split(Environment.NewLine);

    static bool thievesPacified = false, crooksPacified = false;

    // 2: true win (pacifist), 1: win, 0: lose, -1: fleed

    public static async Task<int> Play()
    {
        int playerHP = playerMaxHP;
        int enemyHP = enemyMaxHP;

        int crooksActIndex = 0;
        int thievesActIndex = 0;
        thievesPacified = false; crooksPacified = false;

        bool fleed = false;

        List<(string item, int count, int hpRestore)> items = [("Bandage", 3, 10), ("Butterscotch-cinnamon Pie", 1, 40)];

        while (playerHP > 0 && enemyHP > 0 && (!thievesPacified || !crooksPacified))
        {
            DrawPlayerHealthBar(playerHP);
            DrawEnemyHealthBar(enemyHP);
            ForegroundColor = ConsoleColor.White;
            DrawEnemy();

            List<string> options = ["Fight", "Act", "Item", "Mercy"];

            int actIndex = 1, itemIndex = 2, mercyIndex = 3;

            if (thievesActIndex == talkCount && crooksActIndex == talkCount)
            {
                options.Remove("Act");
                actIndex = -1; itemIndex--; mercyIndex--;
            }
            if (items.Count == 0)
            {
                options.Remove("Item");
                itemIndex = -1; mercyIndex--;
            }

            bool hadTurn = false;
            while (!hadTurn)
            {
                var choice = await ConsoleUtils.Choose(options.Select(o => (SuperString)o).ToList(), padding: 1, escapable: false);

                hadTurn = true;
                if (choice == 0)
                {
                    (int damage, thievesActIndex, crooksActIndex) = await Attack(thievesActIndex, crooksActIndex);
                    enemyHP = Math.Max(0, enemyHP - damage);
                    DrawEnemyHealthBar(enemyHP);
                }
                else if (choice == actIndex)
                {
                    List<SuperString> personOptions = [];
                    if (thievesActIndex != talkCount)
                        personOptions.Add("Thieves");
                    if (crooksActIndex != talkCount)
                        personOptions.Add("Crooks");

                    var personChoice = await ConsoleUtils.Choose(personOptions, padding: 1);
                    if (personChoice == -1)
                    {
                        hadTurn = false;
                        continue;
                    }

                    string message;
                    if (personOptions[personChoice].ToString() == "Thieves")
                    {
                        message = thievesActIndex switch
                        {
                            0 => "You tell the thieves a joke. They laugh and lower their guard.",
                            1 => "You tell the thieves it's okay to not be okay. They are touched and lower their guard.",
                            2 => "You tell the thieves you just want to be friends. The thieves don't want to fight you now.",
                            _ => "You try to reason with the thieves, but they won't listen to you now."
                        };
                        if (thievesActIndex != -1) thievesActIndex++;
                    }
                    else
                    {
                        message = crooksActIndex switch
                        {
                            0 => "You tell the crooks a story. They are intrigued and lower their guard.",
                            1 => "You tell the crooks you understand their pain. They are touched and lower their guard.",
                            2 => "You tell the crooks you don't want to fight them. The crooks don't want to fight you now.",
                            _ => "You try to reason with the crooks, but they won't listen to you now."
                        };
                        if (crooksActIndex != -1) crooksActIndex++;
                    }

                    message = ' ' + message + ' ';

                    Game.DrawBorder(message.Length, 1, threeQuarters: true);
                    SetCursorPosition((WindowWidth - message.Length) / 2, 3 * (WindowHeight - 1) / 4);
                    await WriteSlow(message);
                    ReadKey(true);

                    Clear();
                    DrawPlayerHealthBar(playerHP);
                    DrawEnemyHealthBar(enemyHP);
                    ForegroundColor = ConsoleColor.White;
                    DrawEnemy();
                }
                else if (choice == itemIndex)
                {

                    List<SuperString> itemOptions = items.Select(i => new SuperString($"{i.count}x {i.item}")).ToList();

                    var itemChoice = await ConsoleUtils.Choose(itemOptions, padding: 1);

                    if (itemChoice == -1)
                    {
                        hadTurn = false;
                        continue;
                    }

                    playerHP = Math.Min(playerMaxHP, playerHP + items[itemChoice].hpRestore);
                    items[itemChoice] = (items[itemChoice].item, items[itemChoice].count - 1, items[itemChoice].hpRestore);

                    if (items[itemChoice].count == 0)
                        items.RemoveAt(itemChoice);

                    DrawPlayerHealthBar(playerHP);
                }
                else if (choice == mercyIndex)
                {
                    List<SuperString> personOptions = [];
                    if (!thievesPacified)
                        personOptions.Add(new SuperString("Thieves", thievesActIndex == talkCount ? ConsoleColor.Yellow : ConsoleColor.White));
                    if (!crooksPacified)
                        personOptions.Add(new SuperString("Crooks", crooksActIndex == talkCount ? ConsoleColor.Yellow : ConsoleColor.White));

                    personOptions.Add("Flee");

                    var personChoice = await ConsoleUtils.Choose(personOptions, padding: 1);
                    if (personChoice == -1)
                    {
                        hadTurn = false;
                        continue;
                    }


                    string message = string.Empty;
                    switch (personOptions[personChoice].ToString())
                    {
                        case "Thieves":
                            if (thievesActIndex == talkCount)
                            {
                                thievesPacified = true;
                                message = "The thieves are pacified.";

                            }
                            else
                                message = "You try to reason with the thieves, but they aren't listening to you.";
                            break;
                        case "Crooks":
                            if (crooksActIndex == talkCount)
                            {
                                crooksPacified = true;
                                message = "The crooks are pacified.";
                            }
                            else
                                message = "You try to reason with the crooks, but they aren't listening to you.";
                            break;
                        case "Flee":
                            if ((thievesActIndex == talkCount && crooksActIndex == talkCount) || (crooksPacified && thievesActIndex == talkCount) || (thievesPacified && crooksActIndex == talkCount))
                            {
                                message = "You successfully flee from the enemies.";
                                fleed = true;
                                crooksPacified = true;
                                thievesPacified = true;
                            }
                            message = "You try to flee, but the enemies won't let you escape.";
                            break;
                    }

                    message = ' ' + message + ' ';

                    Game.DrawBorder(message.Length, 1, threeQuarters: true);
                    SetCursorPosition((WindowWidth - message.Length) / 2, 3 * (WindowHeight - 1) / 4);
                    await WriteSlow(message);
                    ReadKey(true);

                    if (!thievesPacified || !crooksPacified)
                    {
                        Clear();
                        DrawPlayerHealthBar(playerHP);
                        DrawEnemyHealthBar(enemyHP);
                        ForegroundColor = ConsoleColor.White;
                        DrawEnemy();
                    }
                }
            }
            /*
            Clear();
            DrawPlayerHealthBar(playerHP);
            DrawEnemyHealthBar(enemyHP);
            ForegroundColor = ConsoleColor.White;
            DrawEnemy();*/

            if (enemyHP > 0 && (!thievesPacified || !crooksPacified))
            {
                ForegroundColor = ConsoleColor.White;
                playerHP = await BeAttacked(playerHP);
            }

            Clear();
        }

        return fleed ? -1 : playerHP > 0 ? enemyHP > 0 ? 2 : 1 : 0;
    }

    static async Task<(int newEnemyHP, int newThieves, int newCrooks)> Attack(int thievesActIndex, int crooksActIndex)
    {
        int width = damageBar[0].Length;
        int height = damageBar.Length;

        var leftPadding = (WindowWidth - width) / 2;
        var topPadding = 3 * (WindowHeight - height) / 4 - 1;

        ForegroundColor = ConsoleColor.White;
        Game.DrawBorder(width, height, threeQuarters: true, absYOffset: -1);

        SetCursorPosition(leftPadding, topPadding);

        foreach (var line in damageBar)
        {
            CursorLeft = leftPadding;
            for (int i = 1; i < width; i++)
            {
                ForegroundColor =
                    i < 3 || i >= width - 3 ? ConsoleColor.DarkRed :
                    i < 7 || i >= width - 7 ? ConsoleColor.Red :
                    i < 11 || i >= width - 11 ? ConsoleColor.Yellow :
                    ConsoleColor.Green;

                Write(line[i]);
            }
            CursorTop++;
        }

        int damage = 0;
        for (int head = 0; head <= width; head++)
        {
            ForegroundColor = ConsoleColor.White;
            CursorTop = topPadding;
            if (head < width)
            {
                for (int i = 0; i < height; i++)
                {
                    CursorLeft = leftPadding + head;
                    /*BackgroundColor =
                        head < 3 || head >= width - 3 ? ConsoleColor.DarkRed :
                        head < 7 || head >= width - 7 ? ConsoleColor.Red :
                        head < 11 || head >= width - 11 ? ConsoleColor.Yellow :
                    ConsoleColor.Green;*/
                    Write('#');
                    CursorTop++;
                }
                //BackgroundColor = ConsoleColor.Black;
            }
            if (head > 0)
            {
                var tail = head - 1;
                ForegroundColor =
                    tail < 3 || tail >= width - 3 ? ConsoleColor.DarkRed :
                    tail < 7 || tail >= width - 7 ? ConsoleColor.Red :
                    tail < 11 || tail >= width - 11 ? ConsoleColor.Yellow :
                    ConsoleColor.Green;

                CursorTop = topPadding;
                for (int i = 0; i < height; i++)
                {
                    CursorLeft = leftPadding + tail;
                    Write(damageBar[i][tail]);
                    CursorTop++;
                }
            }

            
            if (KeyAvailable)
            {
                _ = SoundPlayer.Play(@"SFX/Strike.mp3");

                damage = 10 * (width / 2 - Math.Abs(head - width / 2)) + Random.Shared.Next(-10, 10);
                if (thievesActIndex > 0)
                    damage *= thievesActIndex + 1;
                if (crooksActIndex != -1)
                    damage *= crooksActIndex + 1;

                if (thievesActIndex > 0)
                    thievesActIndex = 0;
                else if (thievesActIndex > 1 || thievesPacified)
                {
                    thievesActIndex = -1;
                    thievesPacified = false;
                }
                if (crooksActIndex == 1 || crooksPacified)
                {
                    crooksActIndex = 0;
                    crooksPacified = false;
                }
                else if (crooksActIndex > 1)
                    crooksActIndex = -1;

                break;
            }

            await Task.Delay(8 * (Math.Abs(head - width / 2) + 1) + 2);
        }

        if (damage > 20)
        {
            for (int i = 0; i < 3; i++)
            {
                CursorTop = topPadding - 3;
                ForegroundColor = i % 2 == 0 ? ConsoleColor.Red : ConsoleColor.White;
                CursorLeft = (WindowWidth - damage.ToString().Length) / 2;
                Write(damage);

                //ForegroundColor = ConsoleColor.Red;
                //ForegroundColor = ConsoleColor.White;
                DrawEnemy((Random.Shared.Next(0, 2) == 0 ? 1 : -1) * i % 2);
                await Task.Delay(500);
            }
            ForegroundColor = ConsoleColor.White;
            DrawEnemy();
        }
        else
        {
            CursorTop = topPadding - 3;
            CursorLeft = (WindowWidth - 5) / 2;
            ForegroundColor = ConsoleColor.White;
            Write("Miss");
            await Task.Delay(3 * 500);
        }

        return (damage, thievesActIndex, crooksActIndex);
    }

    const int gameWidth = 30;
    const int gameHeight = 8;
    const int maxRounds = 100;
    const int tickRate = 17;
    const int msBetweenTicks = 1000 / tickRate;
    const int msBetweenPellets = 100; // pellet generation rate
    const int ticksBetweenPellets = tickRate * msBetweenPellets / 1000; // pellet generation
    const int msBetweenPelletMovement = 150; // pellet movement rate
    const int ticksBetweenPelletMovement = tickRate * msBetweenPelletMovement / 1000; // pellet movement rate

    static async Task<int> BeAttacked(int playerHP)
    {
        Game.DrawBorder(gameWidth, gameHeight, threeQuarters: true, absYOffset: -1, fill: true);
        var leftPadding = (WindowWidth - gameWidth) / 2;
        var topPadding = 3 * (WindowHeight - gameHeight) / 4 - 1;
        int x = gameWidth / 2, y = gameHeight / 2;

        int rounds = 0;
        int addPelletIndex = 1;
        int movePelletIndex = 1;
        int framesNotMoved = 0;
        List<(int x, int y)> pellets = [];

        DrawPlayer(x, y, leftPadding, topPadding);
        while (rounds < maxRounds || pellets.Count > 0)
        {
            ForegroundColor = ConsoleColor.White;
            int oldX = x, oldY = y;

            if (KeyAvailable)
            {
                switch (ReadKey(true).Key)
                {
                    case ConsoleKey.LeftArrow:
                    case ConsoleKey.A:
                        x = Math.Max(0, x - 1);
                        break;
                    case ConsoleKey.RightArrow:
                    case ConsoleKey.D:
                        x = Math.Min(gameWidth - 1, x + 1);
                        break;
                    case ConsoleKey.UpArrow:
                    case ConsoleKey.W:
                        y = Math.Max(0, y - 1);
                        break;
                    case ConsoleKey.DownArrow:
                    case ConsoleKey.S:
                        y = Math.Min(gameHeight - 1, y + 1);
                        break;
                }
                framesNotMoved = 0;
                ConsoleUtils.ClearKeyBuffer();
            }
            else framesNotMoved++;

            if (rounds < maxRounds && addPelletIndex >= ticksBetweenPellets)
            {
                var newPellet = (GenerateRandomX(x, y, ref framesNotMoved), -1);
                pellets.Add(newPellet);
                addPelletIndex = 1;
                rounds++;
            }
            else addPelletIndex++;

            if (movePelletIndex >= ticksBetweenPelletMovement)
            {
                DrawPellets(pellets, leftPadding, topPadding, ' ');
                pellets = pellets.Select(p => (p.x, y:p.y + 1)).Where(p => p.y < gameHeight).ToList();
                movePelletIndex = 1;
                DrawPellets(pellets, leftPadding, topPadding);
            }
            else movePelletIndex++;


            if (pellets.Any(p => p.x == x && p.y == y))
            {
                playerHP = Math.Max(0, playerHP - pelletDamage);
                pellets.Remove((x, y));
                ForegroundColor = ConsoleColor.Red;
                DrawPlayer(oldX, oldY, leftPadding, topPadding, character: ' ');
                DrawPlayer(x, y, leftPadding, topPadding);
                DrawPlayerHealthBar(playerHP);
                if (playerHP == 0)
                {
                    _ = SoundPlayer.Play(@"SFX/Death.mp3");
                    await Task.Delay(500);
                    DrawPlayer(x, y, leftPadding, topPadding, character: ' ');
                    await Task.Delay(1000);
                    return 0;
                }
                else
                {
                    _ = SoundPlayer.Play(@"SFX/DamageTaken.mp3");
                }
            }
            else if (oldX != x || oldY != y)
            {
                DrawPlayer(oldX, oldY, leftPadding, topPadding, ' ');
                DrawPlayer(x, y, leftPadding, topPadding); // '❤'
            }

            await Task.Delay(msBetweenTicks);
        }

        return playerHP;
    }

    static void DrawPlayer(int playerX, int playerY, int leftPadding, int topPadding, char character = 'O', ConsoleColor color = ConsoleColor.Cyan)
    {
        SetCursorPosition(leftPadding + playerX, topPadding + playerY);
        ForegroundColor = color;
        Write(character);
    }

    static void DrawEnemy(int offset = 0)
    {
        var enemy = thievesPacified ? crooks : crooksPacified ? thieves : fullEnemy;

        var leftPadding = (WindowWidth - enemy[0].Length) / 2 + offset - 1; // assuming max offset is 1
        var topPadding = (WindowHeight - enemy.Length) / 4;

        SetCursorPosition(leftPadding, topPadding);

        foreach (var line in enemy)
        {
            CursorLeft = leftPadding;
            Write(' ' + line + ' ');
            CursorTop++;
        }
    }

    static void DrawPlayerHealthBar(int playerHP)
    {
        const int barWidth = 10;
        string msg1 = "HP ";
        string msg2 = $" {playerHP}/{playerMaxHP}";

        int filledAmount = playerHP * barWidth / playerMaxHP;
        

        var leftPadding = (WindowWidth - barWidth - msg1.Length - msg2.Length) / 2;
        var topPadding = 3 * (WindowHeight - damageBar.Length) / 4 + 4;

        SetCursorPosition(leftPadding, topPadding);
        ForegroundColor = ConsoleColor.White;
        Write(msg1);
        ForegroundColor = ConsoleColor.Yellow;
        Write(new string('█', filledAmount));
        ForegroundColor = ConsoleColor.Red;
        Write(new string('█', barWidth - filledAmount));
        ForegroundColor = ConsoleColor.White;
        Write(msg2);
    }

    static void DrawEnemyHealthBar(int enemyHP)
    {
        const int barWidth = 40;

        int filledAmount = enemyHP * barWidth / enemyMaxHP;

        var leftPadding = (WindowWidth - barWidth) / 2;
        var topPadding = (WindowHeight - fullEnemy.Length) / 4 - 3;

        SetCursorPosition(leftPadding, topPadding);
        ForegroundColor = ConsoleColor.Green;
        Write(new string('█', filledAmount));
        ForegroundColor = ConsoleColor.DarkGray;
        Write(new string('█', barWidth - filledAmount));
    }

    static void DrawPellet((int x, int y) pellet, int leftPadding, int topPadding, char character = 'x')
    {
        SetCursorPosition(leftPadding + pellet.x, topPadding + pellet.y);
        Write(character); // '•'
    }

    static void DrawPellets(List<(int x, int y)> pellets, int leftPadding, int topPadding, char character = 'x')
    {
        foreach (var (x, y) in pellets.Where(p => p.y >= 0))
        {
            DrawPellet((x, y), leftPadding, topPadding, character);
        }
    }

    static int GenerateRandomX(int playerX, int playerY, ref int framesNotMoved)
    {
        if (framesNotMoved > tickRate / 3) // could add && playerY < gameHeight - 1, but that would make staying at the top a free win
        {
            framesNotMoved = 0;
            return playerX;
        }
        int x;
        do
        {
            x = Random.Shared.Next(0, gameWidth);
        }
        while (playerY == gameHeight - 1 && x == playerX);

        return x;
    }

    static async Task WriteSlow(string message, int msDelay = 50)
    {
        foreach (var letter in message)
        {
            Write(letter);
            await Task.Delay(msDelay);
        }
    }
}