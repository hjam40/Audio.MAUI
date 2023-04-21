using System.Diagnostics;

namespace Audio.MAUI.Test
{
    public partial class MainPage : ContentPage
    {
        AudioController aController;

        public MainPage()
        {
            InitializeComponent();
            aController = new AudioController();
            micPicker.BindingContext = aController;
            micPicker.SetBinding(Picker.ItemsSourceProperty, nameof(aController.Microphones));
            micPicker.SetBinding(Picker.SelectedItemProperty, nameof(aController.Microphone), mode:BindingMode.TwoWay);
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            var result = aController.StartRecord(Path.Combine(FileSystem.Current.CacheDirectory, "prueba.wav"));
            Debug.WriteLine("Start recording result " + result.ToString());
        }

        private void Button_Clicked_1(object sender, EventArgs e)
        {
            aController.PauseRecord();
        }

        private void Button_Clicked_2(object sender, EventArgs e)
        {
            aController.ResumeRecord();
        }

        private void Button_Clicked_3(object sender, EventArgs e)
        {
            aController.StopRecord();
            var result = aController.NewPlay(Path.Combine(FileSystem.Current.CacheDirectory, "prueba.wav"));
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