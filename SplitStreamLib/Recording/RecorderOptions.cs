using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplitStreamLib.Recording
{
    public class RecorderOptions
    {
        public TimeSpan TriggerDuration { get; set; } = TimeSpan.FromMilliseconds(750);
        public TimeSpan EndTriggerDuration { get; set; } = TimeSpan.FromMilliseconds(750);
        public double TriggerVolume { get; set; } = 0.10d;

        public double EndTriggerVolume { get; set; } = 0.10d;

        public TimeSpan MinClipSeparation { get; set; } = TimeSpan.FromMilliseconds(1000);

        public string OutputDirectory { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;

        public bool Debug { get; set; } = false;
    }
}
