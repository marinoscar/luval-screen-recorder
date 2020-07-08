﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

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

        /// <summary>
        /// Main entry point to the application
        /// </summary>
        /// <param name="args">Arguments</param>
        static void Main(string[] args)
        {
            Trace.Listeners.Add(new ConsoleTraceListener());

            Console.Title = String.Format("{0}: Starting", AppName);

            var arguments = new ConsoleSwitches(args);

            var handle = GetConsoleWindow();
            ShowWindow(handle, arguments.ToRecordingInfo().WindowMode);

            RunAction(() =>
            {
                StartRecording(arguments);

            }, true);
        }

        /// <summary>
        /// Executes an action on the application
        /// </summary>
        /// <param name="args"></param>
        static void StartRecording(ConsoleSwitches args)
        {
            Console.WriteLine("Recording Started");
            var recorder = new Recorder();
            var info = args.ToRecordingInfo();
            recorder.Start(info);

            Console.Title = String.Format("{0}: Recording", AppName);

            if (!info.UseNamedPipes)
                WaitForConsoleSignal();
            else
            {
                WriteLineInfo("Using pipe {0}", info.SessionName);
                WaitForNamedPipesSignal(info);
                WriteLineInfo("Stop signal recieved on pipe {0}", info.SessionName);
            }

            recorder.Stop();

            Console.Title = String.Format("{0}: Completed", AppName);

            var fileInfo = new FileInfo(info.FileName);
            var framesPerSecond = Math.Round((double)(1000 / info.IntervalTimeInMs), 0);
            WriteLineInfo("");
            WriteLineWarning("Completed in file: {0}", fileInfo.FullName);
            WriteLineWarning("File size........: {0} MB", Math.Round((double)(fileInfo.Length / (1024 * 1024)), 2));
            WriteLineWarning("Frames per second: {0}", framesPerSecond);
            WriteLineWarning("Video duration...: {0} min", Math.Round((framesPerSecond * recorder.Frames.Count)/60, 2));
            WriteLineInfo("");
        }

        /// <summary>
        /// Waits for a named piped signal to finish the recording
        /// </summary>
        private static void WaitForNamedPipesSignal(RecordingInfo info)
        {
            var pipeServer = new NamedPipesServer(info.SessionName, "stop");
            pipeServer.Start(info.MaxRecordingMinutes);
        }

        /// <summary>
        /// Waits for text to be entered in the console to finish the recording
        /// </summary>
        private static void WaitForConsoleSignal()
        {
            var isStopSignalRecieved = false;
            while (!isStopSignalRecieved)
            {
                Console.WriteLine("Enter the word STOP top finish the recording");
                isStopSignalRecieved = Console.ReadLine().ToLowerInvariant() == "stop";
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
            var current = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(format, arg);
            Console.ForegroundColor = current;
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
        }

        /// <summary>
        /// Writes a new line to the console
        /// </summary>
        /// <param name="color">The forground color of the message</param>
        /// <param name="format">The string to format</param>
        /// <param name="arg">The arguments to format the string</param>
        public static void WriteLineInfo(string format, params object[] arg)
        {
            Console.WriteLine(format, arg);
        }

        /// <summary>
        /// Writes a new line to the console
        /// </summary>
        /// <param name="color">The forground color of the message</param>
        /// <param name="message">The string to format</param>
        public static void WriteLineInfo(string message)
        {
            Console.WriteLine(message);
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
            using (var eventLog = new EventLog("Application"))
            {
                eventLog.Source = "Application";
                eventLog.WriteEntry(string.Format("{0}:{1}", AppName, errorMessage), EventLogEntryType.Error, 101, 1);
            }
        }
    }
}
