using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using luval.recorder.fileshare;

namespace luval.recorder
{
    /// <summary>
    /// Application entry point
    /// </summary>
    class Program
    {

        #region constants

        public static string AppName = "Luval Recorder";

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public const int SW_HIDE = 0;
        public const int SW_SHOW = 5;
        public const int SW_MINIMIZE = 6;

        #endregion

        private static RecordingInfo _info;

        /// <summary>
        /// Main entry point to the application
        /// </summary>
        /// <param name="args">Arguments</param>
        static void Main(string[] args)
        {
            var arguments = new ConsoleSwitches(args);
            _info = arguments.ToRecordingInfo();

            Trace.Listeners.Add(new ConsoleTraceListener());
            if (_info.LogInfoMessages)
                Trace.Listeners.Add(new EventLogTraceListener());

            Console.Title = String.Format("{0}: Starting", AppName);

            var handle = GetConsoleWindow();
            ShowWindow(handle, _info.WindowMode);

            RunAction(() =>
            {
                StartRecording();

            }, false);
        }

        /// <summary>
        /// Executes an action on the application
        /// </summary>
        static void StartRecording()
        {
            WriteLine("Recording Started");
            var recorder = new Recorder();
            recorder.Start(_info);

            Console.Title = String.Format("{0}: Recording", AppName);

            if (!_info.UseShareFile)
                WaitForConsoleSignal();
            else
            {
                WriteLineInfo("Using pipe {0}", _info.SessionName);
                WaitForFileSignal();
                WriteLineInfo("Stop signal recieved on pipe {0}", _info.SessionName);
            }

            recorder.Stop();

            if (_info.UseShareFile)
                SendCompleteSignal();

            Console.Title = String.Format("{0}: Completed", AppName);

            var fileInfo = new FileInfo(_info.FileName);
            var framesPerSecond = Math.Round((double)(1000 / _info.IntervalTimeInMs), 0);
            WriteLineInfo("");
            WriteLineWarning("Completed in file: {0}", fileInfo.FullName);
            WriteLineWarning("File size........: {0} MB", Math.Round((double)(fileInfo.Length / (1024 * 1024)), 2));
            WriteLineWarning("Frames per second: {0}", framesPerSecond);
            WriteLineWarning("Video duration...: {0} min", Math.Round((framesPerSecond * recorder.Frames.Count) / 60, 2));
            WriteLineInfo("");
        }

        /// <summary>
        /// Waits for a named piped signal to finish the recording
        /// </summary>
        private static void WaitForFileSignal()
        {
            var fileShare = new ProcessShare(_info.SessionName, TimeSpan.FromMinutes(1));
            try
            {
                fileShare.WaitForText("stop", TimeSpan.FromMinutes(_info.MaxRecordingMinutes));
            }
            catch (TimeoutException ex)
            {
                Trace.TraceError("Unable to capture stop signal after waiting {0} mins. Exception: {1}", _info.MaxRecordingMinutes, ex);
            }
            
        }

        private static void SendCompleteSignal()
        {
            var fileShare = new ProcessShare(_info.SessionName + "_BACK", TimeSpan.FromMinutes(1));
            fileShare.WriteMessage("complete");
        }

        /// <summary>
        /// Waits for text to be entered in the console to finish the recording
        /// </summary>
        private static void WaitForConsoleSignal()
        {
            var task = Task.Run(() => {
                DoWaitForConsoleSignal();
            });
            if (!task.Wait(TimeSpan.FromMinutes(_info.MaxRecordingMinutes)))
                Trace.TraceWarning("Stoping recording after {0} minutes as it exceeded the max provided recording duration provided", _info.MaxRecordingMinutes);
        }

        private static void DoWaitForConsoleSignal()
        {
            var isStopSignalRecieved = false;
            while (!isStopSignalRecieved)
            {
                WriteLine("Enter the word STOP top finish the recording");
                isStopSignalRecieved = Console.ReadLine().ToLowerInvariant() == "stop";
            }
        }

        /// <summary>
        /// Validates that only a single instance of the process is available
        /// </summary>
        private static void CheckOnlyOneInstanceIsActive()
        {
            var sessionKey = string.Format("{0}-{1}", Environment.MachineName, Environment.UserName);
            bool createdMutex;
            using (var processMutex = new Mutex(false, sessionKey, out createdMutex))
            {

                if (!createdMutex)
                {
                    var msg = string.Format("There is already another recording in progress for machine {0} and user {1} and the process cannot start", Environment.MachineName, Environment.UserName);                    WriteLineError();
                    Trace.TraceError(msg);
                    WriteErrorToEventLog(msg);

                }
                else
                {
                    // this is the process to actually run..
                    // do application init stuff here
                }
            }
        }

        /// <summary>
        /// Runs the action and handles exceptions
        /// </summary>
        /// <param name="action">The action to execute</param>
        public static void RunAction(Action action, bool waitForKey = false)
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                Console.Title = String.Format("{0}: Failed", AppName);
                WriteLineError(exception.ToString());

            }
            finally
            {
                if (waitForKey)
                {
                    WriteLineInfo("Press any key to end");
                    Console.ReadKey();
                }
            }
        }

        #region Console Methods

        /// <summary>
        /// Writes an message to the console
        /// </summary>
        /// <param name="color">The forground color of the message</param>
        /// <param name="format">The string to format</param>
        /// <param name="arg">The arguments to format the string</param>
        public static void Write(ConsoleColor color, string format, params object[] arg)
        {
            var current = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(format, arg);
            Console.ForegroundColor = current;
        }

        /// <summary>
        /// Writes a new line to the console
        /// </summary>
        /// <param name="color">The forground color of the message</param>
        /// <param name="format">The string to format</param>
        /// <param name="arg">The arguments to format the string</param>
        public static void WriteLine(ConsoleColor color, string format, params object[] arg)
        {
            WriteLine(color, string.Format(format, arg));
        }

        /// <summary>
        /// Writes a new line to the console
        /// </summary>
        /// <param name="color">The forground color of the message</param>
        /// <param name="message">The string to format</param>
        public static void WriteLine(ConsoleColor color, string message)
        {
            var current = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = current;
            if (_info != null && _info.LogInfoMessages) Trace.TraceInformation(message);
        }

        /// <summary>
        /// Writes a new line to the console
        /// </summary>
        /// <param name="message">The string to format</param>
        public static void WriteLine(string message)
        {
            WriteLine(Console.ForegroundColor, message);
        }

        /// <summary>
        /// Writes a new line to the console
        /// </summary>
        /// <param name="format">The string to format</param>
        /// <param name="arg">The arguments to format the string</param>
        public static void WriteLineInfo(string format, params object[] arg)
        {
            WriteLine(string.Format(format, arg));
        }

        /// <summary>
        /// Writes a new line to the console
        /// </summary>
        /// <param name="color">The forground color of the message</param>
        /// <param name="message">The string to format</param>
        public static void WriteLineInfo(string message)
        {
            WriteLine(message);
        }

        /// <summary>
        /// Writes a new line to the console
        /// </summary>
        /// <param name="color">The forground color of the message</param>
        /// <param name="format">The string to format</param>
        /// <param name="arg">The arguments to format the string</param>
        public static void WriteLineWarning(string format, params object[] arg)
        {
            WriteLine(ConsoleColor.Yellow, format, arg);
        }

        /// <summary>
        /// Writes a new line to the console
        /// </summary>
        /// <param name="color">The forground color of the message</param>
        /// <param name="message">The string to format</param>
        public static void WriteLineWarning(string message)
        {
            WriteLine(ConsoleColor.Yellow, message);
        }

        /// <summary>
        /// Writes a new line to the console
        /// </summary>
        /// <param name="color">The forground color of the message</param>
        /// <param name="format">The string to format</param>
        /// <param name="arg">The arguments to format the string</param>
        public static void WriteLineError(string format, params object[] arg)
        {
            WriteLine(ConsoleColor.Red, format, arg);
        }

        /// <summary>
        /// Writes a new line to the console
        /// </summary>
        /// <param name="color">The forground color of the message</param>
        /// <param name="message">The string to format</param>
        public static void WriteLineError(string message)
        {
            WriteLine(ConsoleColor.Red, message);
            WriteErrorToEventLog(message);
        }

        #endregion

        public static void WriteErrorToEventLog(string errorMessage)
        {
            WriteLineError(errorMessage);
            if (!Trace.Listeners.Cast<TraceListener>().Any(i => i.GetType() == typeof(EventLogTraceListener)))
                Trace.TraceError(errorMessage);
            using (var eventLog = new EventLog("Application"))
            {
                eventLog.Source = "Application";
                eventLog.WriteEntry(string.Format("{0}:{1}", AppName, errorMessage), EventLogEntryType.Error, 101, 1);
            }
        }
    }
}
