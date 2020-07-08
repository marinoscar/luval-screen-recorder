using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace luval.recorder
{
    public class RecordingInfo
    {
        public RecordingInfo()
        {
            SessionName = Guid.NewGuid().ToString();
            FileName = string.Format("{0}\\ScreenRecording-{1}.mp4", Environment.CurrentDirectory,  SessionName);
            MaxDurationInMinutes = 3;
            IntervalTimeInMs = 250;
        }
        public string FileName { get; set; }
        public int MaxDurationInMinutes { get; set; }
        public int IntervalTimeInMs { get; set; }
        public string SessionName { get; set; } 
    }
}
