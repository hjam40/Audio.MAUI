using Android.Media;

namespace Audio.MAUI;

public partial class AudioController : IAudioController
{
    private MediaRecorder mediaRecorder;
    private MediaPlayer mediaPlayer;
    private DateTime recordStartTime;
    private TimeSpan recordElapsedTime;
    private void InitDevices()
    {

    }
    private bool StartRec(string file)
    {
        try
        {
            if (OperatingSystem.IsAndroidVersionAtLeast(31))
                mediaRecorder = new MediaRecorder(Android.App.Application.Context);
            else
                mediaRecorder = new MediaRecorder();
            mediaRecorder.SetAudioSource(AudioSource.Mic);
            mediaRecorder.SetOutputFormat(OutputFormat.Mpeg4);
            mediaRecorder.SetOutputFile(file);
            mediaRecorder.SetAudioEncoder(AudioEncoder.Aac);
            mediaRecorder.SetAudioEncodingBitRate((int)RecordConfiguration.BitDepth);
            mediaRecorder.SetAudioSamplingRate((int)RecordConfiguration.SampleRate);
            mediaRecorder.SetAudioChannels((int)RecordConfiguration.Channels);
            mediaRecorder.Prepare();
            mediaRecorder.Start();
            recordStartTime = DateTime.Now;
            recordElapsedTime = TimeSpan.Zero;
            return true;
        } catch { }
        return false;
    }
    private void PauseRec()
    {
        mediaRecorder.Pause();
        recordElapsedTime += DateTime.Now - recordStartTime;
    }
    private bool ResumeRec()
    {
        try
        {
            mediaRecorder.Resume();
            recordStartTime = DateTime.Now;
            return true;
        }
        catch { }
        return false;
    }
    private void StopRec()
    {
        mediaRecorder.Stop();
    }
    private TimeSpan GetRecordTime()
    {
        return (DateTime.Now - recordStartTime) + recordElapsedTime;
    }
    private bool InitPlayer(string file)
    {
        if (mediaPlayer == null)
        {
            mediaPlayer = new MediaPlayer();
            mediaPlayer.Completion += MediaPlayer_Completion;
        }
        try
        {
            mediaPlayer.SetDataSource(file);
            mediaPlayer.Prepare();
            return true;
        }catch { }
        return false;
    }
    private void MediaPlayer_Completion(object sender, EventArgs e)
    {
        Status = AudioControllerStatus.Idle;
        StatusChanged?.Invoke(this, new AudioControllerStatusChangedEventArgs(AudioControllerStatus.Playing, Status));
    }
    private void PlayPlayer()
    {
        mediaPlayer.Start();
    }
    private void PausePlayer()
    {
        mediaPlayer.Pause();
    }
    private void ResumePlayer()
    {
        mediaPlayer.Start();
    }
    private void StopPlayer()
    {
        mediaPlayer.Pause();
        mediaPlayer.SeekTo(0);
    }
    private void SetVolume()
    {
        mediaPlayer.SetVolume((float)volume, (float)volume);
    }
    private void SetSpeed()
    {
        mediaPlayer.PlaybackParams = mediaPlayer.PlaybackParams.SetSpeed((float)playSpeed);
    }
    private TimeSpan GetDuration()
    {
        return TimeSpan.FromMicroseconds(Math.Max(0, mediaPlayer.Duration));
    }
    private TimeSpan GetPlayTime()
    {
        return TimeSpan.FromMicroseconds(mediaPlayer.CurrentPosition);
    }
}
