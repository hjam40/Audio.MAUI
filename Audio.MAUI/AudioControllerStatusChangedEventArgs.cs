namespace Audio.MAUI;

public class AudioControllerStatusChangedEventArgs
{
    public AudioControllerStatus OldStatus { get; private set; }
    public AudioControllerStatus NewStatus { get; private set;}

    public AudioControllerStatusChangedEventArgs(AudioControllerStatus oldStatus, AudioControllerStatus newStatus)
    {
        OldStatus = oldStatus;
        NewStatus = newStatus;
    }
}
