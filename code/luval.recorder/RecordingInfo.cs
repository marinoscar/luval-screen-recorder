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
            FileName = string.Format("ScreenRecording-{0}.avi", SessionName);
            MaxDurationInMinutes = 10;
            FramesPerSecond = 50;
        }
        public string FileName { get; set; }
        public int MaxDurationInMinutes { get; set; }
        public int FramesPerSecond { get; set; }
        public string SessionName { get; set; } 
    }
}
