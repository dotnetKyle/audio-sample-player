using System;

namespace AudioSamplePlayer.Events
{
    public delegate void StoppedEvent(Stopped e);

    public record Stopped 
    {
        public Stopped(StoppedBy stoppedBy)
        {
            StoppedBy = stoppedBy;
        }

        public StoppedBy StoppedBy { get; set; }
    }
    public enum StoppedBy
    {
        User,
        ExternalPlayer,
        PlayerDisposed
    }
}
