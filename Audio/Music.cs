namespace AMysteriousVideogame.Audio;

internal static class Music
{
    public static Song TitleTheme { get; } = new("Sneaky Snitch.mp3");
    public static Song QuirkyDog { get; } = new("Quirky Dog.mp3");
    public static Song FeelGood { get; } = new("Feel Good.mp3");
    public static Song KoolKats { get; } = new("Kool Kats.mp3");
    public static Song Desert { get; } = new("Desert.mp3");
    public static Song TheComplex { get; } = new("The Complex.mp3");
    public static Song Now { get; } = new("Now.mp3");
    public static Song Optimistic { get; } = new("Acoustic Optimistic.mp3");
    public static Song Lose { get; } = new("Climb Together Ending.mp3");
    public static Song Win { get; } = new("Pixel Peeker Polka - faster.mp3");
    public static Song Arcadia { get; } = new("Arcadia.mp3");
    public static Song Undertale { get; } = new("Spear Of Justice.mp3");

    public readonly struct Song(string fileName)
    {
        public string FileName { get; } = fileName;

        public static implicit operator string(Song song) => song.FileName;
    }
}
