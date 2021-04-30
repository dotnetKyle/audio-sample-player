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
            audioSamplePlayer.PositionChanged += AudioSamplePlayer_PositionChanged;
            audioSamplePlayer.Started += AudioSamplePlayer_Started;
            audioSamplePlayer.Stopped += AudioSamplePlayer_Stopped;
            audioSamplePlayer.Paused += AudioSamplePlayer_Paused;

            Console.WriteLine("Audio File Loaded..");

            audioSamplePlayer.Play();
                
            Console.WriteLine("Current State: Playing");
            Console.Write("Enter a command:");


            while (true)
            {
                var line = Console.ReadLine().ToLower();

                if (line == "pause")
                {
                    audioSamplePlayer.Pause();
                }
                if (line == "stop")
                {
                    audioSamplePlayer.Stop();
                }
                if (line == "play")
                {
                    audioSamplePlayer.Play();
                }

                if (line == "exit")
                    break;
            }

            audioSamplePlayer.Dispose();
        }

        static void AudioSamplePlayer_Paused(AudioSamplePlayer.Events.Paused e)
        {
            updateUiStatus("Paused");
        }
        static void AudioSamplePlayer_Stopped(AudioSamplePlayer.Events.Stopped e)
        {
            updateUiStatus("Stopped");
        }
        static void AudioSamplePlayer_Started(AudioSamplePlayer.Events.Started e)
        {
            updateUiStatus("Playing");
        }
        static void AudioSamplePlayer_PositionChanged(AudioSamplePlayer.Events.AudioPositionChanged e)
        {
            if(audioSamplePlayer.PlaybackState == PlaybackState.Stopped)
            {
                updateUiStatus("Stopped");
            }
            else if(audioSamplePlayer.PlaybackState == PlaybackState.Paused)
            {
                updateUiStatus($"Paused  - {e.Position.Minutes}:{e.Position:ss}/{e.Length.Minutes}:{e.Length:ss}");
            }
            else if(audioSamplePlayer.PlaybackState == PlaybackState.Playing)
            {
                updateUiStatus($"Playing - {e.Position.Minutes}:{e.Position:ss}/{e.Length.Minutes}:{e.Length:ss}");
            }
        }

        static void updateUiStatus(string status)
        {
            var currentPosition = Console.GetCursorPosition();

            Console.CursorVisible = false;
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.Write(new string(' ', Console.BufferWidth));
            Console.SetCursorPosition(0, Console.CursorTop - 1);

            Console.WriteLine(status);

            Console.SetCursorPosition(currentPosition.Left, currentPosition.Top);
            Console.CursorVisible = true;
        }
    }
}
