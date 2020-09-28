using System;
using System.Activities;
using System.Threading;
using System.Threading.Tasks;
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

        [LocalizedDisplayName(nameof(Resources.StopLuvalRecording_FileName_DisplayName))]
        [LocalizedDescription(nameof(Resources.StopLuvalRecording_FileName_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<string> FileName { get; set; }

        [LocalizedDisplayName(nameof(Resources.StopLuvalRecording_ErrorMessage_DisplayName))]
        [LocalizedDescription(nameof(Resources.StopLuvalRecording_ErrorMessage_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<string> ErrorMessage { get; set; }

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
            // Inputs
    
            ///////////////////////////
            // Add execution logic HERE
            ///////////////////////////

            // Outputs
            return (ctx) => {
                FileName.Set(ctx, null);
                ErrorMessage.Set(ctx, null);
            };
        }

        #endregion
    }
}

