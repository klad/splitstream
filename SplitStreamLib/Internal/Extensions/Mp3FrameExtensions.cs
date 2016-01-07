using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplitStreamLib.Internal
{
    internal static class Mp3FrameExtensions
    {
        public static Mp3FrameInfo ToFrameInfo(this Mp3Frame frame, double averageVolume)
        {
            return new Mp3FrameInfo(frame, averageVolume);
        }
    }
}
