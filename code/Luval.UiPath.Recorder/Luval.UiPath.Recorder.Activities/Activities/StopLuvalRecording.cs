using System;
using System.Activities;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using luval.recorder.fileshare;
using Luval.UiPath.Recorder.Activities.Properties;
using UiPath.Shared.Activities;
using UiPath.Shared.Activities.Localization;

namespace Luval.UiPath.Recorder.Activities
{
    [LocalizedDisplayName(nameof(Resources.StopLuvalRecording_DisplayName))]
    [LocalizedDescription(nameof(Resources.StopLuvalRecording_Description))]
    public class StopLuvalRecording : ContinuableAsyncCodeActivity
    {
        #region Properties

        /// <summary>
        /// If set, continue executing the remaining activities even if the current activity has failed.
        /// </summary>
        [LocalizedCategory(nameof(Resources.Common_Category))]
        [LocalizedDisplayName(nameof(Resources.ContinueOnError_DisplayName))]
        [LocalizedDescription(nameof(Resources.ContinueOnError_Description))]
        public override InArgument<bool> ContinueOnError { get; set; }


        #endregion


        #region Constructors

        public StopLuvalRecording()
        {
        }

        #endregion


        #region Protected Methods

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {

            base.CacheMetadata(metadata);
        }

        protected override async Task<Action<AsyncCodeActivityContext>> ExecuteAsync(AsyncCodeActivityContext context, CancellationToken cancellationToken)
        {
            SendStopSignal();
            WaitForComplete();
            // Outputs
            return (ctx) => {

            };
        }

        private static void WaitForComplete()
        {
            var messageShare = new ProcessShare(Utils.GetSessionName() + "_BACK");
            Trace.TraceInformation("Waiting on session {0} for message", messageShare.SessionName);

            messageShare.WaitForText("complete", TimeSpan.FromMinutes(10));
        }

        private static void SendStopSignal()
        {
            var messageShare = new ProcessShare(Utils.GetSessionName());
            Trace.TraceInformation("Sending stop signal for recording on session {0}", messageShare.SessionName);
            messageShare.WriteMessage("stop");
            Trace.TraceInformation("Stop signal for session {0} completed", messageShare.SessionName);
        }

        #endregion
    }
}

