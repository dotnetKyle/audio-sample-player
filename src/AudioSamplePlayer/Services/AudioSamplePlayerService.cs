using System;
using NAudio;
using NAudio.Wave;

namespace AudioSamplePlayer.Services
{
    public class AudioSamplePlayerService : IDisposable
    {
        AudioFileReader _audioFileReader;
        DirectSoundOut _output;
        string _filePath;
        float _currentVolume;

        public AudioSamplePlayerService(string filepath, float volume)
        {
            _currentVolume = volume;
            _filePath = filepath;

            PlaybackStopType = PlaybackStoppedTypes.EndOfFile;
        }

        public PlaybackStoppedTypes PlaybackStopType { get; set; }

        public enum PlaybackStoppedTypes
        {
            PlaybackStoppedByUser,
            EndOfFile
        }

        private void initializeIfNeeded()
        {
            if (_audioFileReader == null)
                initializeStream();
            if (_output == null)
                initializeOutput();
        }
        private void initializeStream()
        {
            if(_audioFileReader != null)
            {
                _audioFileReader.Dispose();
                _audioFileReader = null;
            }

            _audioFileReader = new AudioFileReader(_filePath) { Volume = _currentVolume };
        }
        private void initializeOutput()
        {
            if (_output != null)
            {
                _output.Dispose();
                _output = null;
            }

            _output = new DirectSoundOut(200);
            _output.PlaybackStopped += _output_PlaybackStopped;
            _output.Init(new WaveChannel32(_audioFileReader) { PadWithZeroes = false });
        }

        public void Play(PlaybackState playbackState, double currentVolumeLevel)
        {
            if (playbackState == PlaybackState.Stopped || playbackState == PlaybackState.Paused)
            {
                initializeIfNeeded();

                _output.Play();
            }

            _audioFileReader.Volume = (float)currentVolumeLevel;

            if (PlaybackResumed != null)
            {
                PlaybackResumed();
            }
        }

        public void Stop()
        {
            if (_output != null)
            {
                _output.Stop();
            }
        }

        public void Pause()
        {
            if (_output != null)
            {
                _output.Pause();

                if (PlaybackPaused != null)
                {
                    PlaybackPaused();
                }
            }
        }

        public void TogglePlayPause(double currentVolumeLevel)
        {
            if (_output != null)
            {
                if (_output.PlaybackState == PlaybackState.Playing)
                {
                    Pause();
                }
                else
                {
                    Play(_output.PlaybackState, currentVolumeLevel);
                }
            }
            else
            {
                Play(PlaybackState.Stopped, currentVolumeLevel);
            }
        }

        public double GetLengthInSeconds()
        {
            if (_audioFileReader != null)
            {
                return _audioFileReader.TotalTime.TotalSeconds;
            }
            else
            {
                return 0;
            }
        }

        public double GetPositionInSeconds()
        {
            return _audioFileReader != null ? _audioFileReader.CurrentTime.TotalSeconds : 0;
        }

        public float GetVolume()
        {
            if (_audioFileReader != null)
            {
                return _audioFileReader.Volume;
            }
            return 1;
        }

        public void SetPosition(double value)
        {
            if (_audioFileReader != null)
            {
                _audioFileReader.CurrentTime = TimeSpan.FromSeconds(value);
            }
        }

        public void SetVolume(float value)
        {
            if (_output != null)
            {
                _audioFileReader.Volume = value;
            }
        }

        private void _output_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            Dispose();
            if (PlaybackStopped != null)
            {
                PlaybackStopped();
            }
        }

        #region Disposable Implementation

        private bool disposing;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            // free all unmanaged resources 
            if (_output != null)
            {
                if (_output.PlaybackState == PlaybackState.Playing)
                {
                    _output.Stop();
                }
                _output.Dispose();
                _output = null;
            }

            if (_audioFileReader != null)
            {
                _audioFileReader.Dispose();
                _audioFileReader = null;
            }

            if(disposing)
            {
                // free managed resources here
            }
        }
        #endregion

        public event Action PlaybackResumed; 
        public event Action PlaybackStopped;
        public event Action PlaybackPaused;

        ~AudioSamplePlayerService()
        {
            Dispose(false);
        }
    }
}
