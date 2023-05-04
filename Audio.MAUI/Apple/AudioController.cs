#if IOS || MACCATALYST
using AudioToolbox;
using AVFoundation;
using Foundation;
using ObjCRuntime;

namespace Audio.MAUI;


public partial class AudioController : IAudioController
{
    private AVCaptureDevice[] micDevices;
    private AVAudioRecorder recorder;
    private AVAudioPlayer player;

    private void InitDevices() 
    {
        var aSession = AVCaptureDeviceDiscoverySession.Create(new AVCaptureDeviceType[] { AVCaptureDeviceType.BuiltInMicrophone }, AVMediaTypes.Audio, AVCaptureDevicePosition.Unspecified);
        micDevices = aSession.Devices;
        foreach (var device in micDevices)
            Microphones.Add(new MicrophoneInfo { Name = device.LocalizedName, DeviceId = device.UniqueID });
        Microphone = Microphones.FirstOrDefault();
        aSession.Dispose();
    }

    private bool StartRec(string file)
    {
        if (File.Exists(file)) File.Delete(file);
        File.Create(file).Close();
        var settings = new AudioSettings()
        {
            Format = RecordConfiguration.AudioFormat switch
            {
                AudioFormat.M4A => AudioFormatType.MPEG4AAC,
                _ => AudioFormatType.LinearPCM
            },
            LinearPcmBitDepth = (int)RecordConfiguration.BitDepth,
            AudioQuality = RecordConfiguration.Quality switch
            {
                AudioQuality.High => AVAudioQuality.High,
                AudioQuality.Medium => AVAudioQuality.Medium,
                _ => AVAudioQuality.Low
            },
            SampleRate = RecordConfiguration.SampleRate,
            NumberChannels = (int)RecordConfiguration.Channels
        };
        recorder = AVAudioRecorder.Create(NSUrl.FromFilename(file), settings, out NSError error);
        bool result;
        if (error is null)
        {
            var session = AVAudioSession.SharedInstance();
            session.SetCategory(AVAudioSessionCategory.Record);
            result = recorder.PrepareToRecord();
            if (result)
                recorder.Record();
        }
        else
            result = false;

        return result;
    }
    private void PauseRec()
    {
        recorder.Pause();
    }
    private bool ResumeRec()
    {
        return recorder.Record();
    }
    private void StopRec()
    {
        recorder.Stop();
        recorder.Dispose();
    }
    private TimeSpan GetRecordTime()
    {
        return TimeSpan.FromSeconds(recorder.CurrentTime);
    }
    private bool InitPlayer(string file)
    {
        if (player != null)
        {
            player.FinishedPlaying -= Player_FinishedPlaying;
            player.Dispose();
        }
        player = new AVAudioPlayer(NSUrl.FromFilename(file), null, out NSError error);
        if (error is null)
        {
            var session = AVAudioSession.SharedInstance();
            session.SetCategory(AVAudioSessionCategory.Playback);
            player.PrepareToPlay();
            player.FinishedPlaying += Player_FinishedPlaying;
            player.EnableRate = true;
            player.Rate = (float)playSpeed;
            player.Volume = (float)volume;
        }
        else
            player = null;

        return error == null;
    }
    private void Player_FinishedPlaying(object sender, AVStatusEventArgs e)
    {
        Status = AudioControllerStatus.Idle;
        StatusChanged?.Invoke(this, new AudioControllerStatusChangedEventArgs(AudioControllerStatus.Playing, Status));
    }
    private void PlayPlayer()
    {
        player.Play();
    }
    private void PausePlayer()
    {
        player.Pause();
    }
    private void ResumePlayer()
    {
        player.Play();
    }
    private void StopPlayer()
    {
        player.Stop();
        player.CurrentTime = 0;
    }
    private void SetVolume()
    {
        player.Volume = (float)volume;
    }
    private void SetSpeed()
    {
        player.Rate = (float)playSpeed;
    }
    private TimeSpan GetDuration()
    {
        return TimeSpan.FromSeconds(Math.Max(0, player.Duration));
    }
    private TimeSpan GetPlayTime()
    {
        return TimeSpan.FromSeconds(player.CurrentTime);
    }
    private void SeekToPosition(TimeSpan position)
    {
        player.CurrentTime = position.TotalSeconds;
    }

}
#endif
