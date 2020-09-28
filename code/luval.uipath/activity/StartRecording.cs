using CoreWf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace luval.uipath.recorder
{
    /// <summary>
    /// Provides the ability to record the screen in a rolling formart
    /// </summary>
    [Description(" Provides the ability to record the screen in a rolling formart")]
    public class StartRecording : BaseCodeActivity
    {

        [Category("Input")]
        [Description("File to store the recording")]
        [RequiredArgument]
        public InArgument<string> FilePath { get; set; }
        [Category("Input")]
        [Description("Max duration of the recording")]
        [RequiredArgument]
        public InArgument<int> RecordingDuration { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            DoExecute(() =>
            {
                StartRecordingProcess(context);
                return true;
            }, context);
        }

        private void StartRecordingProcess(CodeActivityContext context)
        {
            var session = Utils.GetSessionName();
            var startInfo = new ProcessStartInfo()
            {
                Arguments = string.Format("/outputFile {0} /duration {1} /windowMode {2} /session {3}", FilePath.Get(context), RecordingDuration.Get(context), 0, session),
                FileName = string.Format(@"{0}\recorder.bin\luval.recorder.exe", Environment.CurrentDirectory)
            };
            Trace.TraceInformation("Starting process recording on session {0}", session);
            Process.Start(startInfo);
        }
    }
}
