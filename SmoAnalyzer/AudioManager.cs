using System;
using System.IO;
using System.Windows.Media;

namespace SmoAnalyzer
{
    public static class AudioManager
    {
        private static MediaPlayer _player = new MediaPlayer();
        private static bool _isInitialized = false;
        private static bool _isPaused = false;
        public static void PlayBackgroundMusic()
        {
            if (_isInitialized) return;

            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Song1.wav");

                if (!File.Exists(path)) return;

                _player.Open(new Uri(path));
                _player.MediaEnded += (s, e) =>
                {
                    _player.Position = TimeSpan.Zero;
                    _player.Play();
                };

                _player.Volume = 0.55;
                _player.Play();

                _isInitialized = true;
                _isPaused = false;
            }
            catch
            {
                _isInitialized = false;
            }
        }

        public static void TogglePause()
        {
            if (!_isInitialized)
            {
                PlayBackgroundMusic();
                return;
            }

            if (_isPaused)
            {
                _player.Play();
                _isPaused = false;
            }
            else
            {
                _player.Pause();
                _isPaused = true;
            }
        }
    }
}