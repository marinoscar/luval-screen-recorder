using System;
using System.Activities;
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
    public class StartRecording : CodeActivity
    {

        [Category("Input")]
        [Description("File to store the recording")]
        [RequiredArgument]
        public InArgument<string> FilePath { get; set; }
        [Category("Input")]
        [Description("Max duration of the recording")]
        [RequiredArgument]
        public InArgument<int> RecordingDuration { get; set; }

        [Category("Output")]
        [Description("Message that contains the result of the execution of the activity")]
        public OutArgument<string> ResultMessage { get; set; }
        [Category("Output")]
        [Description("Indicates if there was a success")]
        public OutArgument<bool> Success { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            var startInfo = new ProcessStartInfo()
            {
                Arguments = string.Format("/outputFile {0} /duration {1} /windowMode {2}", FilePath.Get(context), RecordingDuration.Get(context), 0),
                FileName = string.Format(@"{0}\recorder.bin\luval.recorder.exe", Environment.CurrentDirectory)
            };
            try
            {
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                Success.Set(context, false);
                ResultMessage.Set(context, ex.Message);
                return;
            }
            Success.Set(context, true);
            ResultMessage.Set(context, "Recording Started");
        }
    }
}
