using Android.Content;
using Android.Media;
using Java.Nio;
using System;

namespace Audio.MAUI;

public partial class AudioController : IAudioController
{
    private MediaRecorder mediaRecorder;
    private AudioRecord audioRecord;
    private MediaPlayer mediaPlayer;
    private DateTime recordStartTime;
    private TimeSpan recordElapsedTime;
    private bool recording = false, pauseRecording = false, recordTaskRunning = false;
    private int bufferSize;
    List<byte> recordData;
    FileStream recordFs;
    private void InitDevices()
    {
        if (OperatingSystem.IsAndroidVersionAtLeast(28))
        {
            var audioManager = (AudioManager)Android.App.Application.Context.GetSystemService(Context.AudioService);
            Microphones.Clear();
            foreach (var device in audioManager.Microphones)
            {
                Microphones.Add(new MicrophoneInfo { Name = "Microphone " + device.Type.ToString() + " " + device.Address, DeviceId = device.Id.ToString() });
            }
        }
    }
    private bool StartRec(string file)
    {   
        try
        {
            if (RecordConfiguration.AudioFormat == AudioFormat.M4A)
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
            }
            else
            {
                Encoding encoding = RecordConfiguration.BitDepth switch
                {
                    8 => Encoding.Pcm8bit,
                    32 => Encoding.Pcm32bit,
                    _ => Encoding.Pcm16bit
                };
                ChannelIn channels = RecordConfiguration.Channels switch
                {
                    1 => ChannelIn.Mono,
                    _ => ChannelIn.Stereo
                };
                mediaRecorder = null;
                bufferSize = AudioRecord.GetMinBufferSize((int)RecordConfiguration.SampleRate, channels, encoding);
                audioRecord = new AudioRecord(AudioSource.Mic, (int)RecordConfiguration.SampleRate, channels, encoding, bufferSize);
                pauseRecording = false;
                recording = true;
                recordFs = File.Open(file, FileMode.Create);
                recordData = new();
                if (RecordConfiguration.AudioFormat == AudioFormat.WAV)
                    recordData.AddRange(WavFileHeader());
                var recordTask = new Task(() =>
                {
                    recordTaskRunning = true;
                    while (recording)
                    {
                        byte[] buffer = new byte[bufferSize];
                        int bytes = audioRecord.Read(buffer, 0, bufferSize);
                        if (bytes > 0 && !pauseRecording)
                        {
                            for (int i = 0; i < bytes; i++)
                                recordData.Add(buffer[i]);
                        }
                    }
                    recordTaskRunning = false;
                });
                audioRecord.StartRecording();
                recordTask.Start();
            }
            recordStartTime = DateTime.Now;
            recordElapsedTime = TimeSpan.Zero;

            return true;
        } catch { }
        return false;
    }
    private void PauseRec()
    {
        mediaRecorder?.Pause();
        pauseRecording = true;
        recordElapsedTime += DateTime.Now - recordStartTime;
    }
    private bool ResumeRec()
    {
        try
        {
            mediaRecorder?.Resume();
            pauseRecording = false;
            recordStartTime = DateTime.Now;
            return true;
        }
        catch { }
        return false;
    }
    private void StopRec()
    {
        if (RecordConfiguration.AudioFormat == AudioFormat.M4A)
        {
            mediaRecorder.Stop();
            mediaRecorder.Dispose();
        }
        else
        {
            recording = false;
            audioRecord.Stop();
            while (recordTaskRunning) Task.Delay(TimeSpan.FromMilliseconds(10)).Wait();
            if (RecordConfiguration.AudioFormat == AudioFormat.WAV)
                UpdateWavHeaderInformation();
            recordFs.Write(recordData.ToArray());
            recordFs.Flush();
            recordFs.Close();
            recordFs.Dispose();
            audioRecord.Dispose();
        }
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
            mediaPlayer.Reset();
            mediaPlayer.SetDataSource(file);
            mediaPlayer.Prepare();
            return true;
        }
        catch(Java.Lang.Exception ex) 
        { 
           System.Diagnostics.Debug.WriteLine(ex.StackTrace);
        }
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
        return TimeSpan.FromMilliseconds(Math.Max(0, mediaPlayer.Duration));
    }
    private TimeSpan GetPlayTime()
    {
        return TimeSpan.FromMilliseconds(mediaPlayer.CurrentPosition);
    }
    private void SeekToPosition(TimeSpan position)
    {
        mediaPlayer.SeekTo((int)position.TotalMilliseconds);
    }

    private byte[] WavFileHeader()
    {
        int headerSize = 44;
        var header = new byte[headerSize];

        header[0] = ((byte)'R');
        header[1] = ((byte)'I');
        header[2] = ((byte)'F');
        header[3] = ((byte)'F');

        header[4] = 0 & 0xff; // Size of the overall file, 0 because unknown
        header[5] = (0 >>> 8 & 0xff);
        header[6] = (0 >>> 16 & 0xff);
        header[7] = (0 >>> 24 & 0xff);

        header[8] = ((byte)'W');
        header[9] = ((byte)'A');
        header[10] = ((byte)'V');
        header[11] = ((byte)'E');

        header[12] = ((byte)'f'); // 'fmt ' chunk
        header[13] = ((byte)'m');
        header[14] = ((byte)'t');
        header[15] = ((byte)' ');

        header[16] = (byte)RecordConfiguration.BitDepth; // Length of format data
        header[17] = 0;
        header[18] = 0;
        header[19] = 0;

        header[20] = 1; // Type of format (1 is PCM)
        header[21] = 0;

        header[22] = (byte)RecordConfiguration.Channels;
        header[23] = 0;

        header[24] = (byte)(RecordConfiguration.SampleRate & 0xff); // Sampling rate
        header[25] = (byte)(RecordConfiguration.SampleRate >>> 8 & 0xff);
        header[26] = (byte)(RecordConfiguration.SampleRate >>> 16 & 0xff);
        header[27] = (byte)(RecordConfiguration.SampleRate >>> 24 & 0xff);

        var byteRate = RecordConfiguration.SampleRate * RecordConfiguration.Channels * RecordConfiguration.BitDepth / 8;
        header[28] = (byte)(byteRate & 0xff);
        header[29] = (byte)(byteRate >>> 8 & 0xff);
        header[30] = (byte)(byteRate >>> 16 & 0xff);
        header[31] = (byte)(byteRate >>> 24 & 0xff);

        header[32] = (byte)(RecordConfiguration.Channels * RecordConfiguration.BitDepth / 8);
        header[33] = 0;

        header[34] = (byte)RecordConfiguration.BitDepth;
        header[35] = 0;

        header[36] = ((byte)'d');
        header[37] = ((byte)'a');
        header[38] = ((byte)'t');
        header[39] = ((byte)'a');

        header[40] = (0 & 0xff);
        header[41] = (0 >>> 8 & 0xff);
        header[42] = (0 >>> 16 & 0xff);
        header[43] = (0 >>> 24 & 0xff);

        return header;
    }
    private void UpdateWavHeaderInformation()
    {
        var fileSize = recordData.Count;
        var contentSize = fileSize - 44;

        recordData[4] = (byte)(fileSize & 0xff); // Size of the overall file
        recordData[5] = (byte)(fileSize >>> 8 & 0xff);
        recordData[6] = (byte)(fileSize >>> 16 & 0xff);
        recordData[7] = (byte)(fileSize >>> 24 & 0xff);

        recordData[40] = (byte)(contentSize & 0xff); // Size of the data section.
        recordData[41] = (byte)(contentSize >>> 8 & 0xff);
        recordData[42] = (byte)(contentSize >>> 16 & 0xff);
        recordData[43] = (byte)(contentSize >>> 24 & 0xff);
    }
}
