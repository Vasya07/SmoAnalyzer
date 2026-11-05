using System.Windows;
using System.Windows.Controls;

namespace SmoAnalyzer
{
    public partial class MainWindow : Window
    {
        private bool _isPausedUI = false;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        private void MusicToggle_Click(object sender, RoutedEventArgs e)
        {
            AudioManager.TogglePause();
            _isPausedUI = !_isPausedUI;
            var btn = sender as Button;
            if (btn != null)
            {
                if (_isPausedUI)
                    btn.Content = "Музыка играет";
                else
                    btn.Content = "Музыка на паузе";
            }
        }
    }
}