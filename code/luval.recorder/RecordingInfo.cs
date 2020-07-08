using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace luval.recorder
{
    /// <summary>
    /// Information about the recording attributes
    /// </summary>
    public class RecordingInfo
    {
        public RecordingInfo()
        {
            SessionName = "Luval-Recorder-Session";
            FileName = string.Format("{0}\\{1}-{2}.mp4", Environment.CurrentDirectory,  SessionName, Guid.NewGuid());
            MaxDurationInMinutes = 3;
            IntervalTimeInMs = 250;
            MaxRecordingMinutes = 120;
            WindowMode = Program.SW_MINIMIZE;
        }
        /// <summary>
        /// Full file path for the recording
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// Max number of minutes for the screen recording
        /// </summary>
        public int MaxDurationInMinutes { get; set; }
        /// <summary>
        /// Number of milliseconds between each screen capture 
        /// </summary>
        public int IntervalTimeInMs { get; set; }
        /// <summary>
        /// Name of the recording session
        /// </summary>
        public string SessionName { get; set; } 
        /// <summary>
        /// Indicates if the session would open a named pipe to get messages about the recording
        /// </summary>
        public bool UseNamedPipes { get; set; }
        /// <summary>
        /// The recording session would be active to a max of minutes
        /// </summary>
        public int MaxRecordingMinutes { get; set; }
        /// <summary>
        /// Indicates the screen position, 0 = Hide, 5 = Show or 6 minimize
        /// </summary>
        public int WindowMode { get; set; }
    }
}
