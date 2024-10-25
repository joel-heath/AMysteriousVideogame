using AMysteriousVideogame.Minigames;
using ManagedBass;
using System.ComponentModel;

namespace AMysteriousVideogame.Audio;

public static class SoundPlayer
{
    public static async Task Play(MediaPlayer player, double volume = 1, TimeSpan? position = null)
    {
        player.Volume = volume;
        if (position is not null)
        {
            player.Position = position.Value;
        }
        if (player.Duration < TimeSpan.Zero)
        {
            player.Dispose();
            throw new FileNotFoundException($"Failed to play media");
        };
        player.Play();

        TaskCompletionSource tcs = new(); // TaskCreationOptions.RunContinuationsAsynchronously

        void Handler(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(player.State) && player.State == PlaybackState.Stopped)
            {
                player.PropertyChanged -= Handler; // Unsubscribe before disposing
                tcs.SetResult();
                player.Dispose();
            }
        }

        player.PropertyChanged += Handler;

        await tcs.Task;
    }

    public static async Task Play(string fileName, double volume = 1, TimeSpan? position = null)
    {
        MediaPlayer player = new();
        
        await player.LoadAsync(fileName);
        await Play(player, volume, position);
    }
}
