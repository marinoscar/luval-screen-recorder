using Accord.Video.FFMPEG;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace luval.recorder
{
    public class Recorder
    {
        private Timer _timer;
        private VideoFileWriter _writer;
        private Size _screenSize;
        private double _minutesElapsed;
        private int _maxDuration;
        private DateTime _startUtc;

        public string SessionName { get; private set; }

        public event EventHandler Stopped;

        public Recorder()
        {
            _timer = new Timer();
            _timer.Elapsed += Timer_Tick;
            _screenSize = new Size(System.Windows.Forms.SystemInformation.VirtualScreen.Width,
                System.Windows.Forms.SystemInformation.VirtualScreen.Height);
        }

        protected virtual void OnStopped(EventArgs e)
        {
            Stopped?.Invoke(this, e);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            var rec = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            //var index = default(uint);
            using (Bitmap bp = new Bitmap(rec.Width,
                                            rec.Height))
            {
                using (Graphics g = Graphics.FromImage(bp))
                {
                    g.CopyFromScreen(rec.X, rec.Y, 0, 0, bp.Size, CopyPixelOperation.SourceCopy);
                    _writer.WriteVideoFrame(bp);
                    //index++;
                    _minutesElapsed = DateTime.UtcNow.Subtract(_startUtc).TotalMinutes;
                    if (_minutesElapsed >= _maxDuration)
                    {
                        _timer.Enabled = false;
                        Stop();
                    }
                }
            }
        }




        public void Start(RecordingInfo info)
        {
            var bitRate = ((_screenSize.Width * _screenSize.Height) * (int)info.FramesPerSecond);
            _writer = new VideoFileWriter()
            {
                FrameRate = info.FramesPerSecond,
                Height = _screenSize.Height,
                Width = _screenSize.Width,
                VideoCodec = VideoCodec.Mpeg4,

            };
            _writer.Open(info.FileName);
            //_writer.Open(info.FileName, _screenSize.Width, _screenSize.Height, 25, VideoCodec.MPEG4, 1000000);
            //_timer.Interval = Convert.ToInt32(1000 / info.FramesPerSecond);
            _timer.Interval = 30;
            _maxDuration = info.MaxDurationInMinutes;
            SessionName = info.SessionName;
            _timer.Start();
            _startUtc = DateTime.UtcNow;

        }

        public void Stop()
        {
            _timer.Enabled = false;
            _timer.Stop();
            _timer.Dispose();
            _writer.Close();
            OnStopped(new EventArgs());
        }
    }
}
