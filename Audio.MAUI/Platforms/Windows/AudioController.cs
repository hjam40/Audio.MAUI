using Windows.Devices.Enumeration;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Media.Devices;
using Windows.Media.Playback;
using Windows.Media.Core;
using Windows.Storage;

namespace Audio.MAUI;

public partial class AudioController : IAudioController
{
    private MediaCapture mediaCapture;
    private FileStream recordStream;
    private LowLagMediaRecording mediaRecording;
    private MediaPlayer audioPlayer;
    private DateTime recordStartTime;
    private TimeSpan recordElapsedTime;
    private void InitDevices()
    {
        var aDevices = DeviceInformation.FindAllAsync(DeviceClass.AudioCapture).GetAwaiter().GetResult();
        Microphones.Clear();
        foreach (var device in aDevices)
            Microphones.Add(new MicrophoneInfo { Name = device.Name, DeviceId = device.Id });
    }
    private bool StartRec(string file)
    {
        mediaCapture = new MediaCapture();
        try
        {
            mediaCapture.InitializeAsync(new MediaCaptureInitializationSettings
            {
                MemoryPreference = MediaCaptureMemoryPreference.Cpu,
                AudioDeviceId = Microphone.DeviceId
            }).GetAwaiter().GetResult();
            MediaEncodingProfile profile = MediaEncodingProfile.CreateMp3(
                RecordConfiguration.Quality switch
                {
                    AudioQuality.High => AudioEncodingQuality.High,
                    AudioQuality.Medium => AudioEncodingQuality.Medium,
                    _ => AudioEncodingQuality.Low
                });
            profile.Audio.SampleRate = RecordConfiguration.SampleRate;
            profile.Audio.BitsPerSample = RecordConfiguration.BitDepth;
            profile.Audio.ChannelCount = RecordConfiguration.Channels;
            recordStream = new(file, FileMode.Create);
            mediaRecording = mediaCapture.PrepareLowLagRecordToStreamAsync(profile, recordStream.AsRandomAccessStream()).GetAwaiter().GetResult();
            mediaRecording.StartAsync().GetAwaiter().GetResult();
            recordStartTime = DateTime.Now;
            recordElapsedTime = TimeSpan.Zero;
            return true;
        }
        catch { }

        return false;
    }
    private void PauseRec()
    {
        mediaRecording.PauseAsync(MediaCapturePauseBehavior.RetainHardwareResources).GetAwaiter().GetResult();
        recordElapsedTime += DateTime.Now - recordStartTime;
    }
    private bool ResumeRec()
    {
        try
        {
            mediaRecording.ResumeAsync().GetAwaiter().GetResult();
            recordStartTime = DateTime.Now;
            return true;
        }
        catch { }
        return false;
    }
    private void StopRec()
    {
        mediaRecording.StopAsync().GetAwaiter().GetResult();
    }
    private TimeSpan GetRecordTime()
    {
        return (DateTime.Now - recordStartTime) + recordElapsedTime;
    }
    private bool InitPlayer(string file)
    {
        if (audioPlayer == null)
        {
            audioPlayer = new MediaPlayer();
            audioPlayer.MediaEnded += AudioPlayer_MediaEnded;
        }
        try
        {
            audioPlayer.Source = MediaSource.CreateFromStorageFile(StorageFile.GetFileFromPathAsync(file).GetAwaiter().GetResult());
            return true;
        }
        catch { }
        return false;
    }
    private void AudioPlayer_MediaEnded(MediaPlayer sender, object args)
    {
        Status = AudioControllerStatus.Idle;
        StatusChanged?.Invoke(this, new AudioControllerStatusChangedEventArgs(AudioControllerStatus.Playing, Status));
    }
    private void PlayPlayer()
    {
        audioPlayer.Play();
    }
    private void PausePlayer()
    {
        audioPlayer.Pause();
    }
    private void ResumePlayer()
    {
        audioPlayer.Play();
    }
    private void StopPlayer()
    {
        audioPlayer.Pause();
        audioPlayer.Position = new TimeSpan(0);
    }
    private void SetVolume()
    {
        audioPlayer.Volume = volume;
    }
    private void SetSpeed()
    {
        audioPlayer.PlaybackRate = playSpeed;
    }
    private TimeSpan GetDuration()
    {
        return audioPlayer.NaturalDuration;
    }
    private TimeSpan GetPlayTime()
    {
        return audioPlayer.Position;
    }
}
