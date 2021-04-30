using System;
using System.IO;
using System.Threading.Tasks;
using AudioSamplePlayer.Services;
using NAudio.Wave;

namespace AudioPlayerConsole
{
    class Program
    {
        static float currentVolume = 1.0f;
        static PlaybackState currentPlaybackState = PlaybackState.Stopped;
        static AudioSamplePlayerService audioSamplePlayer;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var myMusicDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);

            var files = Directory.GetFiles(myMusicDirectory, "*.mp3");

            for (int i = 0; i < files.Length; i++)
                Console.WriteLine($"{i + 1} - {files[i]}");

            string filePath = null;
            while(filePath == null)
            {
                Console.WriteLine("\r\nChoose a file number.");
                var number = Console.ReadLine();
                int intNumb = -1;
                if(int.TryParse(number, out intNumb))
                {
                    if(intNumb > 0 && intNumb < files.Length + 2)
                        filePath = files[intNumb - 1];
                }
            }

            audioSamplePlayer = new AudioSamplePlayerService(filePath, currentVolume);
            
            Console.WriteLine("Audio File Loaded..");

            audioSamplePlayer.Play(currentPlaybackState, currentVolume);
                
            currentPlaybackState = PlaybackState.Playing;

            Console.WriteLine("Current State: Playing");
            Console.Write("Enter a command:");

            Task.Run(updateUI);

            while (true)
            {
                var line = Console.ReadLine().ToLower();

                if (line == "pause")
                {
                    audioSamplePlayer.Pause();
                    currentPlaybackState = PlaybackState.Paused;
                }
                if (line == "stop")
                {
                    audioSamplePlayer.Stop();
                    currentPlaybackState = PlaybackState.Stopped;
                }
                if (line == "play")
                {
                    audioSamplePlayer.Play(currentPlaybackState, currentVolume);
                    currentPlaybackState = PlaybackState.Playing;
                }

                if (line == "exit")
                    break;
            }

            audioSamplePlayer.Dispose();
        }

        static string currentUiStatus = null;
        static void updateUI()
        {
            while(true)
            {
                System.Threading.Thread.Sleep(33);

                string newUiStatus = "";
                if(currentPlaybackState == PlaybackState.Playing)
                {
                    var position = TimeSpan.FromSeconds(audioSamplePlayer.GetPositionInSeconds());
                    var length = TimeSpan.FromSeconds(audioSamplePlayer.GetLengthInSeconds());

                    newUiStatus = $"Playing:{position.Minutes}:{position:ss}/{length.Minutes}:{length:ss}";
                }
                else if (currentPlaybackState == PlaybackState.Paused)
                {
                    var position = TimeSpan.FromSeconds(audioSamplePlayer.GetPositionInSeconds());
                    var length = TimeSpan.FromSeconds(audioSamplePlayer.GetLengthInSeconds());

                    newUiStatus = $"Paused:{position.Minutes}:{position:ss}/{length.Minutes}:{length:ss}";
                }
                else if (currentPlaybackState == PlaybackState.Stopped)
                {
                    newUiStatus = $"Stopped";
                }

                if (currentUiStatus != newUiStatus)
                {
                    var currentPosition = Console.GetCursorPosition();

                    Console.CursorVisible = false;
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                    Console.Write(new string(' ', Console.BufferWidth));
                    Console.SetCursorPosition(0, Console.CursorTop - 1);

                    Console.WriteLine($"Current State: {newUiStatus}");

                    Console.SetCursorPosition(currentPosition.Left, currentPosition.Top);
                    Console.CursorVisible = true;

                    currentUiStatus = newUiStatus;
                }
            }
        }
    }
}
