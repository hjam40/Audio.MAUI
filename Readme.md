# Audio.MAUI

An audio controller for play and record audio in .net Maui applications

## AudioController

A multiplatform control for play and record audio files. 

Properties and Events:

 ```csharp
/// List of available microphones in the device.
public List<MicrophoneInfo> Microphones
/// Set the microphone to use by the controler. Only available for Windows
public MicrophoneInfo Microphone
/// Set properties for record file output
public RecordConfiguration RecordConfiguration
/// Set the volume for audio playing
public double Volume
/// Set the speed for audio playing
public double PlaySpeed
/// Indicates the actual status of the controller
public AudioControllerStatus Status 
/// Total duration of playback
public TimeSpan Duration
/// Elapsed time of the current recording or playback
public TimeSpan ElapsedTime

/// Invoked every time the controller Status changes
public event AudioControllerStatusChangedHandler StatusChanged
 ```

Methods:
 ```csharp
/// Start recording an audio file async. "RecordConfiguration" property must not be null.
public async Task<AudioControllerResult> StartRecordAsync(string file)
/// Pause the current recording async.
public Task PauseRecordAsync()
/// Resume the current recording async from pause state.
public Task ResumeRecordAsync()
/// Stop the current recording async.
public Task StopRecordAsync()

/// Initializes a new Player with the specific file.
public AudioControllerResult NewPlay(string file)
/// Starts the current playback
public AudioControllerResult Play()
/// Pause the current playback
public void Pause()
/// Resume the current playback
public void Resume()
/// Stops the current playback
public void Stop()
/// Seeks the current playback to the time position indicated
public void SeekTo(TimeSpan position)
 ```

### Install and configure AudioController

1. Download and Install [Audio.MAUI](https://www.nuget.org/packages/Audio.MAUI) NuGet package on your application.

1. Add microphone permissions to your application:

#### Android

In your `AndroidManifest.xml` file (Platforms\Android) add the following permission:

```xml
<uses-permission android:name="android.permission.RECORD_AUDIO" />
```

#### iOS/MacCatalyst

In your `info.plist` file (Platforms\iOS / Platforms\MacCatalyst) add the following permission:

```xml
<key>NSMicrophoneUsageDescription</key>
<string>This app needs access to the microphone for record audios</string>
```
Make sure that you enter a clear and valid reason for your app to access the microphone. This description will be shown to the user.

#### Windows

In your Package.appxmanifest file (Platforms\Windows) go to Capabilities and mark Microphone.

For more information on permissions, see the [Microsoft Docs](https://docs.microsoft.com/dotnet/maui/platform-integration/appmodel/permissions).

    ```

### Using AudioController

In your c# file, make sure to add the right using:

```csharp
using Audio.MAUI;
```

Use the control:

```csharp
        private async void Button_Clicked(object sender, EventArgs e)
        {
            file = Path.Combine(FileSystem.Current.CacheDirectory, "prueba.wav");
            aController.RecordConfiguration.AudioFormat = AudioFormat.WAV;
            aController.RecordConfiguration.Channels = 2;
            var result = await aController.StartRecordAsync(file);
            Debug.WriteLine("Start recording result " + result.ToString());
        }
        private async void Button_Clicked_8(object sender, EventArgs e)
        {
            file = Path.Combine(FileSystem.Current.CacheDirectory, "prueba.mp4");
            aController.RecordConfiguration.AudioFormat = AudioFormat.M4A;
            aController.RecordConfiguration.Channels = 2;
            var result = await aController.StartRecordAsync(file);
            Debug.WriteLine("Start recording result " + result.ToString());
        }

        private void Button_Clicked_1(object sender, EventArgs e)
        {
            aController.PauseRecordAsync();
        }

        private void Button_Clicked_2(object sender, EventArgs e)
        {
            aController.ResumeRecordAsync();
        }

        private async void Button_Clicked_3(object sender, EventArgs e)
        {
            await aController.StopRecordAsync();
            var result = aController.NewPlay(file);
            Debug.WriteLine("New Play result " + result.ToString());
        }

        private void Button_Clicked_4(object sender, EventArgs e)
        {
            aController.Play();
        }

        private void Button_Clicked_5(object sender, EventArgs e)
        {
            aController.Pause();
        }

        private void Button_Clicked_6(object sender, EventArgs e)
        {
            aController.Resume();
        }

        private void Button_Clicked_7(object sender, EventArgs e)
        {
            aController.Stop();
        }
```
