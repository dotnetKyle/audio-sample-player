using System;

namespace AudioSamplePlayer.Events
{
    public delegate void PausedEvent(Paused e);

    public record Paused
    {
        public Paused(PausedBy pausedBy)
        {
            PausedBy = pausedBy;
        }

        public PausedBy PausedBy { get; set; }
    }
    public enum PausedBy
    {
        User,
        ExternalPlayer
    }
}
