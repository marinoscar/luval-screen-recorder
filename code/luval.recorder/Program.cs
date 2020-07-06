using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace luval.recorder
{
    class Program
    {
        static ManualResetEvent _quitEvent = new ManualResetEvent(false);
        static void Main(string[] args)
        {
            var info = new RecordingInfo() { MaxDurationInMinutes = 1, SessionName = "Oscar", FramesPerSecond = 100 };
            var recorder = new Recorder();
            recorder.Stopped += Recorder_Stopped;

            Console.CancelKeyPress += (sender, eArgs) =>
            {
                recorder.Stop();
                eArgs.Cancel = true;
                
            };

            recorder.Start(new FileInfo("recording.mp4"), 1);

            //_quitEvent.WaitOne();

            Console.WriteLine("press any key to complete the recording");
            Console.ReadKey();

            recorder.Stop();

            Console.WriteLine("Recording completed. File saved in {0}", info.FileName);

        }

        private static void Recorder_Stopped(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
