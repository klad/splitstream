using System;
using System.Collections.Generic;


namespace SplitStreamLib.Internal
{
    internal class FrameInfoBuffer
    {
        private Queue<Mp3FrameInfo> _buffer;
        private readonly double _duration;
        private double _durationInFrames;

        public FrameInfoBuffer(TimeSpan duration)
        {
            _duration = duration.TotalMilliseconds;
        }

        public void  Enqueue(Mp3FrameInfo frameInfo)
        {
            if(_buffer == null)
            {
                // initialize the frame information
                _buffer = new Queue<Mp3FrameInfo>();

                var frame = frameInfo.Frame;
                var samplesPerMillisecond = (double)frame.SampleRate / (double)1000;
                var numSamples = _duration * samplesPerMillisecond;
                _durationInFrames = Math.Ceiling(numSamples / frame.SampleCount);
            }

            // if we're already full then drop the first frame that was added.
            if (IsFull)
                Dequeue();

            _buffer.Enqueue(frameInfo);
        }

        public Mp3FrameInfo Dequeue()
        {
            if (_buffer == null)
                throw new InvalidOperationException();

            return _buffer.Dequeue();
        }

        public void Clear()
        {
            _buffer.Clear();
        }

        public IEnumerable<Mp3FrameInfo> GetFrames()
        {
            return _buffer.ToArray();
        }

        public bool IsFull
        {
            get
            {
                if (_buffer == null)
                    return false;

                return _buffer.Count >= _durationInFrames;
            }
        }
    }
}
