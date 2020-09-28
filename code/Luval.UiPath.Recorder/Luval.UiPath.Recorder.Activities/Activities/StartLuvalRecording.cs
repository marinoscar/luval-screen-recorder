using System;
using System.Activities;
using System.Threading;
using System.Threading.Tasks;
using Luval.UiPath.Recorder.Activities.Properties;
using UiPath.Shared.Activities;
using UiPath.Shared.Activities.Localization;

namespace Luval.UiPath.Recorder.Activities
{
    [LocalizedDisplayName(nameof(Resources.StartLuvalRecording_DisplayName))]
    [LocalizedDescription(nameof(Resources.StartLuvalRecording_Description))]
    public class StartLuvalRecording : ContinuableAsyncCodeActivity
    {
        #region Properties

        /// <summary>
        /// If set, continue executing the remaining activities even if the current activity has failed.
        /// </summary>
        [LocalizedCategory(nameof(Resources.Common_Category))]
        [LocalizedDisplayName(nameof(Resources.ContinueOnError_DisplayName))]
        [LocalizedDescription(nameof(Resources.ContinueOnError_Description))]
        public override InArgument<bool> ContinueOnError { get; set; }

        [LocalizedDisplayName(nameof(Resources.StartLuvalRecording_VideoFolderLocation_DisplayName))]
        [LocalizedDescription(nameof(Resources.StartLuvalRecording_VideoFolderLocation_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> VideoFolderLocation { get; set; }

        [LocalizedDisplayName(nameof(Resources.StartLuvalRecording_RecordingDurationInMinutes_DisplayName))]
        [LocalizedDescription(nameof(Resources.StartLuvalRecording_RecordingDurationInMinutes_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> RecordingDurationInMinutes { get; set; }

        [LocalizedDisplayName(nameof(Resources.StartLuvalRecording_MaxRecordingDurationInMinutes_DisplayName))]
        [LocalizedDescription(nameof(Resources.StartLuvalRecording_MaxRecordingDurationInMinutes_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> MaxRecordingDurationInMinutes { get; set; }

        #endregion


        #region Constructors

        public StartLuvalRecording()
        {
        }

        #endregion


        #region Protected Methods

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (VideoFolderLocation == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(VideoFolderLocation)));

            base.CacheMetadata(metadata);
        }

        protected override async Task<Action<AsyncCodeActivityContext>> ExecuteAsync(AsyncCodeActivityContext context, CancellationToken cancellationToken)
        {
            // Inputs
            var videofolderlocation = VideoFolderLocation.Get(context);
            var recordingdurationinminutes = RecordingDurationInMinutes.Get(context);
            var maxrecordingdurationinminutes = MaxRecordingDurationInMinutes.Get(context);
    
            ///////////////////////////
            // Add execution logic HERE
            ///////////////////////////

            // Outputs
            return (ctx) => {
            };
        }

        #endregion
    }
}

