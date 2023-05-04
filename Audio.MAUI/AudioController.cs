using System.ComponentModel;
using System.Runtime.Versioning;
using System.Windows.Markup;

namespace Audio.MAUI;

public partial class AudioController : IAudioController, INotifyPropertyChanged
{
    /// <summary>
    /// List of available microphones in the device.
    /// </summary>
    public List<MicrophoneInfo> Microphones { get; private set; } = new List<MicrophoneInfo>();
    /// <summary>
    /// Set the microphone to use by the controler. Only available for Windows
    /// </summary>
    public MicrophoneInfo Microphone { get; set; }
    /// <summary>
    /// Set properties for record file output
    /// </summary>
    public RecordConfiguration RecordConfiguration { get; set; } = new RecordConfiguration();
    double volume = 0.5;
    /// <summary>
    /// Set the volume for audio playing
    /// </summary>
    public double Volume
    {
        get => volume;
        set
        {
            volume = Math.Clamp(value, 0, 1);
            SetVolume();
            OnPropertyChanged(nameof(Volume));
        }
    }
    private double playSpeed = 1;
    /// <summary>
    /// Set the speed for audio playing
    /// </summary>
    public double PlaySpeed
    {
        get => playSpeed;
        set
        {
            playSpeed = Math.Clamp(value, 0.5, 2);
            SetSpeed();
            OnPropertyChanged(nameof(PlaySpeed));
        }
    }
    private AudioControllerStatus status = AudioControllerStatus.Idle;
    /// <summary>
    /// Indicates the actual status of the controller
    /// </summary>
    public AudioControllerStatus Status 
    { 
        get => status; 
        private set
        {
            status = value;
            OnPropertyChanged(nameof(Status));
        } 
    }
    public delegate void AudioControllerStatusChangedHandler(object sender, AudioControllerStatusChangedEventArgs args);
    /// <summary>
    /// Invoked every time the controller Status changes
    /// </summary>
    public event AudioControllerStatusChangedHandler StatusChanged;
    private bool playerInit = false;
    /// <summary>
    /// Total duration of playback
    /// </summary>
    public TimeSpan Duration { get; private set; } = TimeSpan.Zero;
    /// <summary>
    /// Elapsed time of the current recording or playback
    /// </summary>
    public TimeSpan ElapsedTime { get; private set; } = TimeSpan.Zero;
    public event PropertyChangedEventHandler PropertyChanged;
    private readonly System.Timers.Timer timer;
    private void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public AudioController()
    {
        InitDevices();        
        timer = new(100);
        timer.Elapsed += Timer_Elapsed;
    }
    /// <summary>
    /// Start recording an audio file async. "RecordConfiguration" property must not be null.
    /// <paramref name="file"/> Full path to file where audio will be stored.
    /// </summary>
    public async Task<AudioControllerResult> StartRecordAsync(string file)
    {
        if (RecordConfiguration != null)
        {
            if (Status == AudioControllerStatus.Idle)
            {
                if (DeviceInfo.Current.Platform == DevicePlatform.WinUI && Microphone == null)
                    return AudioControllerResult.MicrophoneNotSelected;
                if (await RequestPermissions())
                {
                    if (StartRec(file))
                    {
                        ElapsedTime = TimeSpan.Zero;
                        OnPropertyChanged(nameof(ElapsedTime));
                        timer.Start();
                        Status = AudioControllerStatus.Recording;
                        StatusChanged?.Invoke(this, new AudioControllerStatusChangedEventArgs(AudioControllerStatus.Idle, Status));
                        return AudioControllerResult.Success;
                    }
                    else
                        return AudioControllerResult.ErrorCreatingRecorder;
                }
                return AudioControllerResult.AccessDenied;
            }
            else
                return AudioControllerResult.NotInCorrectStatus;
        }
        else
            return AudioControllerResult.AudioConfigurationError;
    }
    /// <summary>
    /// Pause the current recording async.
    /// </summary>
    public Task PauseRecordAsync()
    {
        return Task.Run(() =>
        {
            if (Status == AudioControllerStatus.Recording)
            {
                PauseRec();
                timer.Stop();
                Status = AudioControllerStatus.PauseRecording;
                StatusChanged?.Invoke(this, new AudioControllerStatusChangedEventArgs(AudioControllerStatus.Recording, Status));
            }
        });
    }
    /// <summary>
    /// Resume the current recording async from pause state.
    /// </summary>
    public Task ResumeRecordAsync()
    {
        return Task.Run(() =>
        {
            if (Status == AudioControllerStatus.PauseRecording)
            {
                ResumeRec();
                timer.Start();
                Status = AudioControllerStatus.Recording;
                StatusChanged?.Invoke(this, new AudioControllerStatusChangedEventArgs(AudioControllerStatus.PauseRecording, Status));
            }
        });
    }
    /// <summary>
    /// Stop the current recording async.
    /// </summary>
    public Task StopRecordAsync()
    {
        return Task.Run(() =>
        {
            if (Status == AudioControllerStatus.Recording || Status == AudioControllerStatus.PauseRecording)
            {
                StopRec();
                timer.Stop();
                var oldStatus = Status;
                Status = AudioControllerStatus.Idle;
                StatusChanged?.Invoke(this, new AudioControllerStatusChangedEventArgs(oldStatus, Status));
            }
        });
    }
    /// <summary>
    /// Initializes a new Player with the specific file.
    /// <paramref name="file"/> Full path to the audio file.
    /// </summary>
    public AudioControllerResult NewPlay(string file)
    {
        playerInit = false;
        if (File.Exists(file))
        {
            if (InitPlayer(file))
            {
                playerInit = true;
                Duration = GetDuration();
                OnPropertyChanged(nameof(Duration));
                return AudioControllerResult.Success;
            }
            else
                return AudioControllerResult.ErrorCreatingPlayer;
        }
        else
            return AudioControllerResult.FileNotFound;
    }
    /// <summary>
    /// Starts the current playback
    /// </summary>
    public AudioControllerResult Play()
    {
        if (playerInit)
        {
            if (Status == AudioControllerStatus.Idle)
            {
                PlayPlayer();
                timer.Start();
                Status = AudioControllerStatus.Playing;
                StatusChanged?.Invoke(this, new AudioControllerStatusChangedEventArgs(AudioControllerStatus.Idle, Status));
                return AudioControllerResult.Success;
            }
            else
                return AudioControllerResult.NotInCorrectStatus;
        }
        else
            return AudioControllerResult.PlayerNotInitiated;
    }
    /// <summary>
    /// Pause the current playback
    /// </summary>
    public void Pause()
    {
        if (Status == AudioControllerStatus.Playing)
        {
            PausePlayer();
            timer.Stop();
            Status = AudioControllerStatus.PausePlaying;
            StatusChanged?.Invoke(this, new AudioControllerStatusChangedEventArgs(AudioControllerStatus.Playing, Status));
        }
    }
    /// <summary>
    /// Resume the current playback
    /// </summary>
    public void Resume()
    {
        if (Status == AudioControllerStatus.PausePlaying)
        {
            ResumePlayer();
            timer.Start();
            Status = AudioControllerStatus.Playing;
            StatusChanged?.Invoke(this, new AudioControllerStatusChangedEventArgs(AudioControllerStatus.PausePlaying, Status));
        }
    }
    /// <summary>
    /// Stops the current playback
    /// </summary>
    public void Stop()
    {
        if (Status == AudioControllerStatus.Playing || Status == AudioControllerStatus.PausePlaying)
        {
            StopPlayer();
            timer.Stop();
            var oldStatus = Status;
            Status = AudioControllerStatus.Idle;
            StatusChanged?.Invoke(this, new AudioControllerStatusChangedEventArgs(oldStatus, Status));
        }
    }
    /// <summary>
    /// Seeks the current playback to the time position indicated
    /// <paramref name="position"/> Time position.
    /// </summary>
    public void SeekTo(TimeSpan position)
    {
        SeekToPosition(position);
    }
    private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        if (Status == AudioControllerStatus.Recording)
            ElapsedTime = GetRecordTime();
        else if (Status == AudioControllerStatus.Playing)
            ElapsedTime = GetPlayTime();
        OnPropertyChanged(nameof(ElapsedTime));
    }
    /// <summary>
    /// Request permission for use the microphone
    /// </summary>
    public static async Task<bool> RequestPermissions()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.Microphone>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.Microphone>();
            if (status != PermissionStatus.Granted) return false;
        }
        return true;
    }

#if !IOS && !MACCATALYST && !ANDROID && !WINDOWS
    private void InitDevices() {}
    private bool StartRec(string file)
    {
        return false;
    }
    private void PauseRec()
    {
    }
    private bool ResumeRec()
    {
        return false;
    }
    private void StopRec()
    {
    }
    private bool InitPlayer(string file)
    {
        return false;
    }
    private void PlayPlayer()
    {
    }
    private void PausePlayer()
    {
    }
    private void ResumePlayer()
    {
    }
    private void StopPlayer()
    {
    }
    private void SetVolume()
    {
    }
    private void SetSpeed()
    {
    }
    private TimeSpan GetDuration()
    {
        return TimeSpan.Zero;
    }
    private TimeSpan GetPlayTime()
    {
        return TimeSpan.Zero;
    }
    private TimeSpan GetRecordTime()
    {
        return TimeSpan.Zero;
    }
    private void SeekToPosition(TimeSpan position)
    {
    }
#endif
}