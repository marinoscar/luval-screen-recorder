using System;
using System.Collections.Generic;
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

            Console.CancelKeyPress += (sender, eArgs) => {
                _quitEvent.Set();
                eArgs.Cancel = true;
            };

            recorder.Start(info);

            _quitEvent.WaitOne();

        }

        private static void Recorder_Stopped(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
