using System;

namespace AudioSamplePlayer.Events
{
    public delegate void StartedEvent(Started e);

    public record Started 
    { 
        public Started(PlaybackStartedBy playbackStartedBy)
        {
            PlaybackStartedBy = playbackStartedBy;
        }

        public PlaybackStartedBy PlaybackStartedBy { get;  set; }
    }
    public enum PlaybackStartedBy
    {
        User,
        Autoplay
    }
}
