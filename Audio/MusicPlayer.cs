using ManagedBass;

namespace AMysteriousVideogame.Audio;

public static class MusicPlayer
{
    private static readonly MediaPlayer player = new() { Loop = true };

    public static TimeSpan Position { get => player.Position; set => player.Position = value; }

    public static double Volume { get => player.Volume; set => player.Volume = value; }

    public static async Task Play(string fileName, double volume = 1, TimeSpan? position = null)
    {
        if (!await player.LoadAsync(fileName))
            await player.LoadAsync(@"Music/" + fileName);
        //            if (!await player.LoadAsync(@"Music/" + fileName))
        //                await player.LoadAsync(@"Music/" + fileName + @".mp3");
        if (player.Duration < TimeSpan.Zero)
        {
            player.Dispose();
            throw new FileNotFoundException($"Failed to play media", fileName);
        };

        player.Play();
        player.Volume = volume;
        if (position is not null)
            player.Position = position.Value;
    }

    public static void Play()
    {
        player.Play();
    }

    public static void Pause()
    {
        player.Pause();
    }

    public static void Restart()
    {
        player.Stop();
        player.Play();
    }

    public static async Task FadeOut(int milliseconds, double volume = 0, int ticksPerSecond = 20)
    {
        var totalTicks = milliseconds * ticksPerSecond / 1000d;
        var tickLength = (Volume - volume) / totalTicks;

        for (int i = 0; i < totalTicks; i++)
        {
            Volume -= tickLength;
            await Task.Delay(1000 / ticksPerSecond);
        }

        if (volume == 0) player.Pause();
    }

    public static async Task FadeIn(int milliseconds, double volume = 1, int ticksPerSecond = 20)
    {
        var totalTicks = milliseconds * ticksPerSecond / 1000d;
        var tickLength = (volume - Volume) / totalTicks;

        player.Play();
        for (int i = 0; i < totalTicks; i++)
        {
            Volume += tickLength;
            await Task.Delay(1000 / ticksPerSecond);
        }
    }

    public static void Dispose()
    {
        player.Dispose();
    }
}