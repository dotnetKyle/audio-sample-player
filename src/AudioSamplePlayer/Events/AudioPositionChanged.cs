using System;

namespace AudioSamplePlayer.Events
{
    public delegate void AudioPositionChangedEvent(AudioPositionChanged e);

    public record AudioPositionChanged
    {
        public AudioPositionChanged(TimeSpan position, TimeSpan length)
        {
            Position = position;
            Length = length;
        }

        public TimeSpan Position { get; private set; }
        public TimeSpan Length { get; private set; }
    }
}
