using System.Diagnostics;

namespace Audio.MAUI.Test
{
    public partial class MainPage : ContentPage
    {
        readonly AudioController aController;
        string file = "";

        public MainPage()
        {
            InitializeComponent();
            aController = new AudioController();
            micPicker.BindingContext = aController;

            micPicker.SetBinding(Picker.ItemsSourceProperty, nameof(aController.Microphones));
            micPicker.SetBinding(Picker.SelectedItemProperty, nameof(aController.Microphone), mode: BindingMode.TwoWay);
        }

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
    }
}