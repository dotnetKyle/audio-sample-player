using System;
using NAudio.Wave;
using AudioSamplePlayer.Events;
using System.Threading.Tasks;

namespace AudioSamplePlayer.Services
{
    public class AudioSamplePlayerService : IDisposable
    {
        AudioFileReader _audioFileReader;
        DirectSoundOut _output;
        string _filePath;
        float _currentVolume;

        public AudioSamplePlayerService(string filepath, float volume = 1.0f)
        {
            _currentVolume = volume;
            _filePath = filepath;

            PlaybackStopType = PlaybackStoppedTypes.EndOfFile;

            // sends updates to the UI when there are any
            Task.Run(backgroundAudioTimeTask);
        }

        public PlaybackState PlaybackState 
        { 
            get 
            {
                if (_output == null)
                    return PlaybackState.Stopped;
                else
                    return _output.PlaybackState;
            } 
        }
        public PlaybackStoppedTypes PlaybackStopType { get; set; }
        public float CurrentVolumeLevel => _currentVolume;

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

        public void Play(PlaybackStartedBy playbackStartedBy = PlaybackStartedBy.User)
        {
            if (PlaybackState == PlaybackState.Stopped || PlaybackState == PlaybackState.Paused)
            {
                initializeIfNeeded();

                _output.Play();
            }

            _audioFileReader.Volume = CurrentVolumeLevel;

            Started?.Invoke(new Started(playbackStartedBy));
        }

        public void Stop(StoppedBy playbackStoppedBy = StoppedBy.User)
        {
            if (_output != null)
            {
                _output.Stop();
                Stopped?.Invoke(new Stopped(playbackStoppedBy));
            }
        }

        public void Pause(PausedBy playbackPausedBy = PausedBy.User)
        {
            if (_output != null)
            {
                _output.Pause();

                Paused?.Invoke(new Paused(playbackPausedBy));
            }
        }

        //public double GetLengthInSeconds()
        //{
        //    if (_audioFileReader != null)
        //    {
        //        return _audioFileReader.TotalTime.TotalSeconds;
        //    }
        //    else
        //    {
        //        return 0;
        //    }
        //}
        //public double GetPositionInSeconds()
        //{
        //    return _audioFileReader != null ? _audioFileReader.CurrentTime.TotalSeconds : 0;
        //}

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
                _currentVolume = value;
                _audioFileReader.Volume = value;
            }
        }

        private void _output_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            Dispose();

            if (Stopped != null && PlaybackState == PlaybackState.Playing || PlaybackState == PlaybackState.Paused)
                Stopped.Invoke(new Stopped(StoppedBy.PlayerDisposed));
        }

        bool playerClosing = false;
        double lastAudioPosition = -1;
        double lastAudioLength = -1;
        void backgroundAudioTimeTask()
        {
            // iterate until dispose is called
            while(playerClosing == false)
            {
                if(PositionChanged != null && _audioFileReader != null)
                {
                    var currentPosition = Math.Truncate(_audioFileReader.CurrentTime.TotalSeconds);
                    var currentLength = Math.Truncate(_audioFileReader.TotalTime.TotalSeconds);

                    if(currentPosition != lastAudioPosition || currentLength != lastAudioLength)
                    {
                        lastAudioPosition = currentPosition;
                        lastAudioLength = currentLength;

                        var tsPosition = TimeSpan.FromSeconds(currentPosition);
                        var tsLength = TimeSpan.FromSeconds(currentLength);

                        PositionChanged.Invoke(new AudioPositionChanged(tsPosition, tsLength));
                    }
                }

                // sleep one frame
                System.Threading.Thread.Sleep(33);
            }
        }

        #region Disposable Implementation

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if(disposing)
            {
                // free managed resources here
                playerClosing = true;

                // sleep one frame
                System.Threading.Thread.Sleep(33);
            }

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

        }
        #endregion

        public event StartedEvent Started; 
        public event StoppedEvent Stopped;
        public event PausedEvent Paused;
        public event AudioPositionChangedEvent PositionChanged;
        
        ~AudioSamplePlayerService()
        {
            Dispose(false);
        }
    }

}
