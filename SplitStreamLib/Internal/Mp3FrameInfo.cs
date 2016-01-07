using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplitStreamLib.Internal
{
    internal class Mp3FrameInfo
    {
        private double _averageVolume;

        public Mp3FrameInfo(Mp3Frame frame, double averageVolume)
        {
            this.Frame = frame;
            _averageVolume = averageVolume;
        }

        public Mp3Frame Frame { get; }

        public double AverageVolume
        {
            get { return _averageVolume; }
        }
    }
}
