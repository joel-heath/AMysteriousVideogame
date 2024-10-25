using AMysteriousVideogame.Audio;
using AMysteriousVideogame.Minigames;
using ManagedBass;

namespace AMysteriousVideogame;

internal class Program
{
    static string playerName = string.Empty;
    static readonly int[] stats = new int[10];
    static bool firstTime = true;

    static async Task Main()
    {
        //Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.CursorVisible = false;
        
        LoadStats();

        if (await TitleScreen())
        {
            Person joelheath24 = new("joelheath24", ConsoleColor.Yellow, !firstTime);
            Person slingshotP = new("Slingshot P", ConsoleColor.Blue, !firstTime);
            Person player = new(playerName == string.Empty ? "You" : playerName, ConsoleColor.Cyan, !firstTime);

            async Task<bool> TrueWrapper(Func<Person, Person, Person, Task> a) // LavaPit, Temple, and Ravine all return void, since you can't lose in them.
            {
                await a(joelheath24, slingshotP, player);
                return true;
            }

            bool playing = true;
            while (playing)
            {
                if (playerName == "")
                    await Intro(joelheath24, slingshotP, player);

                if (!firstTime)
                {
                    joelheath24.DialogueSkippable = true; slingshotP.DialogueSkippable = true; player.DialogueSkippable = true;
                    await MusicPlayer.Play(Music.QuirkyDog, 0);
                }
                firstTime = false;

                stats[9]++;
                
                await Chapter0(joelheath24, slingshotP, player);
                int choice = await Chapter1(joelheath24, slingshotP, player);

                playing = choice switch
                {
                    -1 => await TrueWrapper(LavaPit),
                    0 => await TrueWrapper(Temple),
                    1 => await Ravine(joelheath24, slingshotP, player),
                    2 => await Mansion(joelheath24, slingshotP, player),
                    3 => await SwimmingPool(joelheath24, slingshotP, player),
                    _ => false
                };

                if (playing)
                    playing = await Beacon(joelheath24, slingshotP, player);

                SaveStats();

                if (playing)
                    await NewHome();
                playing = await TitleScreen();
            }
        }

        await MusicPlayer.FadeOut(100);
        MusicPlayer.Dispose();
    }

    static async Task NewHome()
    {
        await MusicPlayer.Play(Music.Win, 0.4);
        ConsoleUtils.Title("YOU WON (what did you win? some ESSENTIALS)", "Welcome to new home!", "Press any key to return to the title screen");
        ConsoleUtils.ClearKeyBuffer();
        Console.ReadKey(true);

        Console.Clear();
        await MusicPlayer.FadeOut(3000);
    }

    static async Task<bool> Beacon(Person joelheath24, Person slingshotP, Person player)
    {
        _ = SoundPlayer.Play(@"SFX/Walking.mp3", 0.1);
        await Task.Delay(6000);

        await MusicPlayer.Play(Music.Arcadia, 0.4);

        await Task.Delay(1000);

        await joelheath24.Say("Woah! Look at that beacon!", 23);
        await slingshotP.Say("It's glorious!", 19);
        await player.Say("And now it's ours!");
        await Task.Delay(1000);

        var thieves = Enumerable.Range(1, 5).Select(n => new Person("Thief " + n, ConsoleColor.DarkBlue, dialogueSkippable: false)).ToList();
        var crooks = Enumerable.Range(1, 5).Select(n => new Person("Crook " + n, ConsoleColor.DarkGreen, dialogueSkippable: false)).ToList();

        MusicPlayer.Pause();
        _ = SoundPlayer.Play("SFX/ThatsOurBeacon.mp3");
        await Task.Delay(1892);

        foreach (var t in thieves)
        {
            await t.Say("That's our beacon!", sleep: -1);
        }
        await Task.Delay(1202); // 3094 - 1892
        foreach (var c in crooks)
        {
            await c.Say("No, that's our beacon!", sleep: -1);
        }

        await Task.Delay(1776); // 4870 - 3094 = 1776

        await MusicPlayer.Play(Music.Undertale, 0.4);
        Console.Clear();
        ConsoleUtils.Title("Defeat the enemies!", "Hit the spacebar to land an attack. Time it correctly to deal critical hits!", "Press any key to begin.");
        ConsoleUtils.ClearKeyBuffer();
        Console.ReadKey(true);
        Console.Clear();
        int winCode = await BattleGame.Play();
        await MusicPlayer.FadeOut(2000);

        switch (winCode)
        {
            case 0:
                await MusicPlayer.Play(Music.Lose);
                ConsoleUtils.Title("You died", "Press any key to restart");
                ConsoleUtils.ClearKeyBuffer();
                Console.ReadKey(true);

                stats[3]++;
                Console.Clear();
                return false;
            case 1:
                //thieves.ForEach(t =  await t.Say("We'll be back!", sleep: -1));
                //crooks.ForEach(t =  await t.Say("We'll be back!", sleep: -1));
                await joelheath24.Say("We did it! The beacon is ours!", @"Voice/joelheath24/24a.mp3");
                stats[0]++;
                break;
            case 2:
                await joelheath24.Say("Hooray, we can all share the beacon!", @"Voice/joelheath24/24b.mp3");
                stats[0]++;
                stats[1]++;
                break;
            case -1:
                await joelheath24.Say("Phew, we made it out alive!", @"Voice/joelheath24/24c.mp3");
                stats[0]++;
                stats[2]++;
                break;
        }
        await slingshotP.Say("Let's get out of here and celebrate!", 20);
        await player.Say("Woah, what's this place over here?");
        await slingshotP.Say("Is that... a NEW HOME!?", 21);

        Console.Clear();
        return true;
    }

    static async Task<bool> SwimmingPool(Person joelheath24, Person slingshotP, Person player)
    {
        await MusicPlayer.Play(Music.Optimistic, 0.4);

        await joelheath24.Say("Woah! It's a swimming pool!", 19);
        await slingshotP.Say("Let's take a dip!", 15);

        Person guard = new("Lifeguard", ConsoleColor.Red, joelheath24.DialogueSkippable);

        await guard.Say("Sorry, only those under 3ft may swim here.", 1);

        await slingshotP.Say("WHAT!?", 16);
        await joelheath24.Say("Hmm... maybe we can bribe the guard to let us in.", 20);
        await player.Say("I know, I'll challenge him to a swimming race!");
        await slingshotP.Say("Great idea!", 17);
        await guard.Say("Fine, I accept your challenge. You're not gonna win.", 2);

        await MusicPlayer.FadeOut(1000);
        await MusicPlayer.Play(@"SFX/SwimmingPool.mp3", 0.2);
        Console.Clear();
        ConsoleUtils.Title("Alternate the left and right arrow keys to swim to the end of the pool.", "Press any key to begin");
        ConsoleUtils.ClearKeyBuffer();
        Console.ReadKey(true);
        Console.Clear();

        if (!await RaceGame.Play())
        {
            await guard.Say("Hahaha, I knew you were too slow. Now get out!", 4);
            await MusicPlayer.FadeOut(500);
            await MusicPlayer.Play(Music.Lose);
            ConsoleUtils.Title("You lost", "Press any key to restart");
            ConsoleUtils.ClearKeyBuffer();
            Console.ReadKey(true);
            Console.Clear();
            return false;
        }

        await guard.Say("What!? How did you win!?", 3);
        await joelheath24.Say("Great job! Let's dive in!", 21);
        await SoundPlayer.Play(@"SFX/Submerge.mp3");
        await Task.Delay(1000);
        await SoundPlayer.Play(@"SFX/Surface.mp3");
        await slingshotP.Say("Hey do you guys see that tunnel at the bottom of the pool?", 18);
        await joelheath24.Say("Oh yeah, let's explore!", 22);
        await SoundPlayer.Play(@"SFX/Submerge.mp3");

        await MusicPlayer.FadeOut(1000);
        Console.Clear();
        await SoundPlayer.Play(@"SFX/Surface.mp3");

        stats[8]++;
        return true;
    }

    static async Task<bool> Mansion(Person joelheath24, Person slingshotP, Person player)
    {
        await joelheath24.Say("Woah! There's a mansion out here!", 17);
        await slingshotP.Say("AND IT'S ON FIRE! RUN!", 12, sleep:20);

        await MusicPlayer.Play(Music.Now, 0, TimeSpan.FromMilliseconds(10327));
        await MusicPlayer.FadeIn(500, 0.2);
        Console.Clear();
        ConsoleUtils.Title("Use the up and down arrow keys to dodge obstacles.", "Press any key to begin");
        ConsoleUtils.ClearKeyBuffer();
        Console.ReadKey(true);
        Console.Clear();
        
        if (!await FireGame.Play())
        {
            await MusicPlayer.Play(Music.Lose);
            ConsoleUtils.Title("You died", "Press any key to restart");
            ConsoleUtils.ClearKeyBuffer();
            Console.ReadKey(true);

            Console.Clear();
            stats[3]++;
            return false;
        }

        await joelheath24.Say("Phew! We made it out alive!", 18);
        await slingshotP.Say("Let's take shelter in the cave over there.", 13);
        await player.Say("Woah, do you guys see that tunnel at the back of the cave?");
        await slingshotP.Say("Yeah, let's explore!", 14);

        Console.Clear();
        stats[7]++;
        return true;
    }

    static async Task<bool> Ravine(Person joelheath24, Person slingshotP, Person player)
    {
        await MusicPlayer.Play(Music.TheComplex, 0.4);

        await joelheath24.Say("Woah! Check out this ravine!", 16);
        await slingshotP.Say("There's an old stone path down there! Let's investigate!", 10);
        await player.Say("I'd say there's more lack of path than there is path...");
        await slingshotP.Say("Guess it's time to whip out my epic parkour skills!", 11);

        Console.Clear();
        ConsoleUtils.Title("Use the WASD and the spacebar to navigate through the ravine.", "Use the arrow keys to move one block at a time, WASD to move two blocks at a time.", "Don't hold down keys, tap them one at a time.", "Press any key to begin.");
        ConsoleUtils.ClearKeyBuffer();
        Console.ReadKey(true);
        Console.Clear();

        bool won = await PlatformerGame.Play();
        await MusicPlayer.FadeOut(1000);
        if (!won)
        {
            await MusicPlayer.Play(Music.Lose);
            ConsoleUtils.Title("You died", "Press any key to restart");
            ConsoleUtils.ClearKeyBuffer();
            Console.ReadKey(true);

            Console.Clear();
            stats[3]++;
            return false;
        }

        stats[6]++;
        return true;
    }

    static async Task Temple(Person joelheath24, Person slingshotP, Person player) 
    {
        await MusicPlayer.Play(Music.Desert, 0.4, TimeSpan.FromMinutes(3) + TimeSpan.FromSeconds(47));

        await joelheath24.Say("Woah! Is that a temple?", 14);
        await slingshotP.Say("Let's explore!", 9);

        // rumbling sound
        _ = SoundPlayer.Play(@"SFX/TempleRumble.mp3"); // crash at 4840
        await MusicPlayer.FadeOut(1000);
        await player.Say("What's that sound?"); // 19 characters = 1045ms
        await Task.Delay(2800); // 4840 - 1045 - 1000 = 2795
        await ConsoleUtils.Narrate("The ground suddenly caves in, causing you to descend to the catacombs.", sleep: -1, skippable: !firstTime);
        await joelheath24.Say("AHHH!!!", sleep: -1);
        await slingshotP.Say("AHHH!!!", sleep: -1);
        await player.Say("AHHH!!!", sleep: -1);
        _ = SoundPlayer.Play(@"SFX/Falling.mp3");
        await Task.Delay(4850);
        await joelheath24.Say("YOUCH!", sleep: -1);
        await slingshotP.Say("YOUCH!", sleep: -1);
        await player.Say("YOUCH!", sleep: -1);
        await Task.Delay(2000);
        await joelheath24.Say("Now we're gonna have to find our way out of here!", 15);

        await MusicPlayer.FadeIn(1000, 0.4);
        Console.Clear();
        ConsoleUtils.Title("Use the arrow keys to escape the catacombs.", "Press any key to begin");
        ConsoleUtils.ClearKeyBuffer();
        Console.ReadKey(true);
        Console.Clear();

        await MazeGame.Play();
        await MusicPlayer.FadeOut(1000);
        Console.Clear();
        stats[5]++;
    }

    static async Task LavaPit(Person joelheath24, Person slingshotP, Person player)
    {
        await MusicPlayer.Play(Music.KoolKats, 0.4);

        await joelheath24.Say("Woah! There's a room down here?", 10);
        await slingshotP.Say("I can't believe we survived falling into a lava pit!", 6);

        await ConsoleUtils.Narrate("You see an obsidian wall blocking the path forward.", skippable: !firstTime);
        await player.Say("Does anyone have a pickaxe?");
        await joelheath24.Say("I do, here you go.", 11);
        await slingshotP.Say("You'll have to hit the weak points to break it.", 7);
        await joelheath24.Say("And do it quick enough! Or else your progress will reset.", 12);

        Console.Clear();
        ConsoleUtils.Title("Use the arrow keys to hit the weak points.", "Press any key to begin");
        ConsoleUtils.ClearKeyBuffer();
        Console.ReadKey(true);
        Console.Clear();

        await TargetGame.Play();
        Console.Clear();

        await joelheath24.Say("Great job! Let's keep going!", 13);
        await slingshotP.Say("I wonder where this path will lead us...", 8);

        await MusicPlayer.FadeOut(1000);
        Console.Clear();
        stats[4]++;
    }

    static async Task<int> Chapter1(Person joelheath24, Person slingshotP, Person player)
    {
        await MusicPlayer.Play(Music.FeelGood, 0.4);

        await Task.Delay(1000);
        await joelheath24.Say("Hmm.. it appears we've reached a crossroads.", 8);

        await slingshotP.Say("Which way should we go?", 4);

        await player.Say("", newLines: 0);

        SuperString[] options = ["Dexter", "Pursue", "Recede", "Sinister"];
        string[] meanings = ["right", "forward", "backwards", "left"];

        var cts = new CancellationTokenSource();
        int choice = -1;
        MediaPlayer mp = new() { Volume = 2 };
        await mp.LoadAsync(@"SFX/LavaPitJumpScare.mp3");

        mp.Play();
        cts.CancelAfter(5000);

        try
        {
            choice = await ConsoleUtils.Choose(options, escapable:!firstTime, ct:cts.Token);
            if (choice == -1)
            {
                mp.Position = TimeSpan.FromSeconds(5);
                cts.Cancel();
                cts.Token.ThrowIfCancellationRequested();
            }
            mp.Pause();

            await player.Say($"Let's go in a {options[choice].ToString().ToLower()} sort of direction", printName: false);

            await slingshotP.Say("Huh???", 5);

            await joelheath24.Say($"So you mean {meanings[choice]}?", @$"Voice/joelheath24/9{(char)(choice + 'a')}.mp3");
            await Task.Delay(300);

            await player.Say("Uhh... yeah. That.");
            await Task.Delay(700);
        }
        catch (OperationCanceledException e) when (e.CancellationToken == cts.Token)
        {
            Console.Clear();
            MusicPlayer.Pause();
            await ConsoleUtils.Narrate("Herobrine suddenly appears, making you all jump, catastrophically causing you to fall into a pit of lava", sleep: -1, skippable: !firstTime);
            await Task.Delay(900);
            await joelheath24.Say("AHHH!!!", sleep:-1);
            await slingshotP.Say("AHHH!!!", sleep: -1);
            await player.Say("AHHH!!!", sleep: -1);
            await Task.Delay(4000);
        }
        finally
        {
            cts.Dispose();
            mp.Dispose();
        }

        Console.Clear();
        return choice;
    }

    static async Task Chapter0(Person joelheath24, Person slingshotP, Person player)
    {
        MusicPlayer.Pause();
        await Task.Delay(1000);
        await slingshotP.Say("you need to wake up", 2, sleep: 70);
        await Task.Delay(2000);
        if (MusicPlayer.Volume == 0)
        {
            MusicPlayer.Volume = 0.2;
            MusicPlayer.Position = TimeSpan.FromMilliseconds(5591);
        }
        MusicPlayer.Play();

        await joelheath24.Say("Haha, you're so funny, Slingshot!", 4);

        await joelheath24.Say("Guys, I think it's high time we went on another adventure!", 5);

        await slingshotP.Say("Great idea! Why don't we ask our new friend to join us?", 3);

        await joelheath24.Say($"Yeah! Say, {player.Name}, which direction should we go?", 6);
        await Task.Delay(300);

        await player.Say("Let's start by following the path ahead.");
        await Task.Delay(700);

        await joelheath24.Say("Alright then, let's go!", 7);

        await MusicPlayer.FadeOut(2000);
        Console.Clear();
    }

    static async Task Intro(Person joelheath24, Person slingshotP, Person player)
    {
        await MusicPlayer.Play(Music.QuirkyDog);

        await Task.Delay(3000);
        await MusicPlayer.FadeOut(2000, 0.2);

        _ = SoundPlayer.Play(@"Voice/joelheath24/1.mp3", 2);
        await Task.Delay(3600);

        await joelheath24.Say("Hey! ", sleep: 20, newLines: 0);
        await Task.Delay(600);
        await joelheath24.Say("Who are you!?", sleep: 30, printName: false);
        await Task.Delay(1000);

        await player.Say("Hi! My name is ", newLines: 0);

        ConsoleUtils.ClearKeyBuffer();
        player.Name = ConsoleUtils.ReadLine();
        playerName = player.Name;
        Console.CursorLeft = 0;
        await player.Say($"Hi! My name is {player.Name}.{new string(' ', Math.Max(0, 3 - player.Name.Length))}", sleep: -1);

        await joelheath24.Say("What a lovely name!", 2);
        await Task.Delay(300);

        await joelheath24.Say("Hey, Slingshot, come and meet my new friend!", 3);

        await slingshotP.Say("Oh, alright then.", 1);
        await Task.Delay(1000);
    }

    static async Task<bool> TitleScreen()
    {
        await MusicPlayer.Play(Music.TitleTheme);

        List<SuperString> options = (playerName == "") ? ["New Game", "Exit"] : ["Continue", "Stats", "Reset Game", "Exit"];

        ConsoleUtils.ClearKeyBuffer();
        int choice;
        do
        {
            ConsoleUtils.Title("A Mysterious Videogame");
            choice = await ConsoleUtils.Choose(options, softClear: false);
            if (options.Count > 2)
            {
                if (choice == 1)
                {
                    Console.Clear();
                    Console.WriteLine("Statistics");
                    Console.WriteLine($"Games played: {stats[9]}");
                    Console.WriteLine($"Wins: {stats[0]}");
                    if (stats[1] > 0)
                        Console.WriteLine($"True wins: {stats[1]}");
                    if (stats[2] > 0)
                        Console.WriteLine($"Wins by fleeing: {stats[2]}");
                    Console.WriteLine($"Deaths: {stats[3]}");

                    Console.WriteLine($"Times broken obsidian: {stats[4]}");
                    Console.WriteLine($"Times escaped a temple: {stats[5]}");
                    Console.WriteLine($"Times traversed a ravine: {stats[6]}");
                    Console.WriteLine($"Times escaped a burning mansion: {stats[7]}");
                    Console.WriteLine($"Times beat a lifeguard in a swimming race: {stats[8]}");

                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("Press any key to return");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.ReadKey(true);

                    Console.Clear();
                }
                if (choice == 2)
                {
                    playerName = "";
                    SaveStats();
                    options = ["New Game", "Exit"];
                }
            }
        }
        while (choice != 0 && choice != options.Count - 1);

        Console.Clear();

        if (choice == 0)
        {
            MusicPlayer.Pause();
            _ = SoundPlayer.Play(@"SFX/StartGame.mp3");
            await Task.Delay(1000);
        }
        return choice == 0;
    }

    static void LoadStats()
    {
        if (File.Exists("stats.dat"))
        {
            firstTime = false;
            using BinaryReader br = new(File.OpenRead("stats.dat"));
            playerName = br.ReadString();
            for (int i = 0; br.BaseStream.Position != br.BaseStream.Length && i < stats.Length; i++)
                stats[i] = br.ReadInt32();
        }
    }

    static void SaveStats()
    {
        using BinaryWriter bw = new(File.OpenWrite("stats.dat"));
        bw.Write(playerName);
        foreach (int stat in stats)
            bw.Write(stat);
    }
}