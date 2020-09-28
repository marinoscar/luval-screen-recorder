using CoreWf;
using luval.recorder.fileshare;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace luval.uipath.recorder
{

    [Description("Stops the screen recording a saves the file into the disk")]
    public class EndRecording : BaseCodeActivity
    {
        
        protected override void Execute(CodeActivityContext context)
        {
            DoExecute(() => {
                SendStopSignal();
                WaitForCompleteSignal(context);
                return true;
            }, context);
        }

        private bool SendStopSignal()
        {
            var session = Utils.GetSessionName();
            Trace.TraceInformation("Sending the stop signal on session {0}", session);
            var fileShareSend = new ProcessShare(Utils.GetSessionName());
            var res = fileShareSend.WriteMessage("stop");
            Trace.TraceInformation("Stop signal on session {0} posted", session);
            return res;
        }

        private void WaitForCompleteSignal(CodeActivityContext context)
        {
            var session = Utils.GetSessionName() + "_BACK";
            var fileShareRec = new ProcessShare(session);
            Trace.TraceInformation("Waiting for complete signal on session {0}", session);
            var st = DateTime.UtcNow;
            fileShareRec.WaitForText("complete", TimeSpan.FromMinutes(TimeoutInMinutes.Get(context)));
            Trace.TraceInformation("Finished wait on session {0} after {1}", session, DateTime.UtcNow.Subtract(st));
        }
    }
}
