using SplitStreamLib.Internal;
using System;
using System.Collections.Generic;

namespace SplitStreamLib.Recording.Internal
{
    internal class Trigger
    {
        private readonly double _volumeLevel;
        private readonly double _duration;
        private double _durationInFrames;
        private bool _isSet = false;

        private double _averageVolume = 0d;
        private double _totalVolume = 0d;

        private Queue<Mp3FrameInfo> _buffer;
        private Queue<double> _volumes;

        private bool _greaterThan;

        public Trigger(double volumeLevel, TimeSpan duration, bool greaterThan)
        {
            _volumeLevel = volumeLevel;
            _duration = duration.TotalMilliseconds;

            _volumes = new Queue<double>();

            _greaterThan = greaterThan;
        }

        public void AddFrameInfo(Mp3FrameInfo frameInfo)
        {
            if(_buffer == null)
            {
                _buffer = new Queue<Mp3FrameInfo>();

                var frame = frameInfo.Frame;
                var samplesPerMillisecond = (double)frame.SampleRate / (double)1000;
                var numSamples = _duration * samplesPerMillisecond;
                _durationInFrames = Math.Ceiling(numSamples / frame.SampleCount);
            }

            if(IsFull())
            {
                _buffer.Dequeue();
                var v = _volumes.Dequeue();
                _totalVolume -= v;
            }

            _buffer.Enqueue(frameInfo);

            var frameVolume = frameInfo.AverageVolume;
            _volumes.Enqueue(frameVolume);
            _totalVolume += frameVolume;

            _averageVolume = _totalVolume / _volumes.Count;

            if(_greaterThan)
            {
                if (_averageVolume >= _volumeLevel && IsFull())
                    _isSet = true;
            }
            else
            {
                if (_averageVolume <= _volumeLevel && IsFull())
                    _isSet = true;
            }
        }

        public Mp3FrameInfo[] GetFrames()
        {
            return _buffer.ToArray();
        }

        public bool IsSet()
        {
            if (_buffer == null)
                return false;

            return _isSet;
        }

        public void Clear()
        {
            if (_buffer == null)
                return;

            _totalVolume = 0d;
            _averageVolume = 0d;

            _buffer.Clear();
            _volumes.Clear();
            _isSet = false;
        }

        private bool IsFull()
        {
            if (_buffer == null)
                return false;

            return _buffer.Count >= _durationInFrames;
        }
    }

}
