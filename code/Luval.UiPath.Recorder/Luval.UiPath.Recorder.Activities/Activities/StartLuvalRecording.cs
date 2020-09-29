using System;
using System.Activities;
using System.Diagnostics;
using System.IO;
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
        public InArgument<double?> RecordingDurationInMinutes { get; set; }

        [LocalizedDisplayName(nameof(Resources.StartLuvalRecording_MaxRecordingDurationInMinutes_DisplayName))]
        [LocalizedDescription(nameof(Resources.StartLuvalRecording_MaxRecordingDurationInMinutes_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<double?> MaxRecordingDurationInMinutes { get; set; }

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

            var dirInfo = new DirectoryInfo(Utils.GetExecDirLocation());
            Trace.TraceInformation("Starting from {0}", Utils.GetExecDirLocation());

            // Inputs
            var videofolderlocation = VideoFolderLocation.Get(context);
            var recordingdurationinminutes = RecordingDurationInMinutes.Get(context);
            var maxrecordingdurationinminutes = MaxRecordingDurationInMinutes.Get(context);
            var sessionName = Utils.GetSessionName();
            var libraryFile = new FileInfo(dirInfo.FullName + @"\recorder.bin\luval.recorder.exe");

            if (!libraryFile.Exists) throw new InvalidOperationException(string.Format("Current directory {0} and unable to locate dependency {1}", dirInfo.FullName,  libraryFile.FullName));

            if (maxrecordingdurationinminutes == null) maxrecordingdurationinminutes = 120d;
            if (recordingdurationinminutes == null) recordingdurationinminutes = 3d;

            if (recordingdurationinminutes < 0.99 || recordingdurationinminutes > 20) throw new ArgumentOutOfRangeException("The RecordingDurationInMinutes should be greater or equal than 1 and less than 20");

            var videoFile = Utils.GetVideoFileName(videofolderlocation);

            Trace.TraceInformation("Starting Luval Recording with Session: {0} Duration: {1} Max Duration: {2}", sessionName, recordingdurationinminutes, maxrecordingdurationinminutes);

            var args = new ProcessStartInfo() {
                FileName = libraryFile.FullName,
                Arguments = string.Format("/session {0} /outputFile {1} /duration {2} /maxRecording {3} /useShareFile y /windowMode 6", sessionName, videoFile.FullName, recordingdurationinminutes, maxrecordingdurationinminutes)
            }; 

            try
            {
                Process.Start(args);
            }
            catch (Exception)
            {
                throw;
            }

            // Outputs
            return (ctx) => {
            };
        }

        #endregion
    }
}

