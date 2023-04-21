namespace Audio.MAUI;

public class RecordConfiguration
{
    public AudioQuality Quality { get; set; } = AudioQuality.High;
    public uint BitDepth { get; set; } = 16;
    public uint SampleRate { get; set; } = 44100;
    public uint Channels { get; set; } = 1;
}
