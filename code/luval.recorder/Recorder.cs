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
        private VideoFileWriter _writer;
        private Size _screenSize;
        private double _minutesElapsed;
        private int _maxDuration;
        private DateTime _startUtc;
        private uint _frameIndex;
        private RollingList<byte[]> _frames;
        private FileInfo _file;


        public event EventHandler Stopped;
        public Timer Timer { get; private set; }

        public Recorder()
        {
            Timer = new Timer();
            Timer.Elapsed += Timer_Tick;
            _screenSize = new Size(System.Windows.Forms.SystemInformation.VirtualScreen.Width,
                System.Windows.Forms.SystemInformation.VirtualScreen.Height);
        }

        protected virtual void OnStopped(EventArgs e)
        {
            Stopped?.Invoke(this, e);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            var rec = new Rectangle(0, 0, _screenSize.Width, _screenSize.Height);

            using (Bitmap bp = new Bitmap(rec.Width,
                                            rec.Height))
            {
                using (Graphics g = Graphics.FromImage(bp))
                {
                    g.CopyFromScreen(rec.X, rec.Y, 0, 0, bp.Size, CopyPixelOperation.SourceCopy);
                    using (var stream = new MemoryStream())
                    {
                        bp.Save(stream, ImageFormat.Jpeg);
                        _frames.Add(stream.ToArray());
                    }
                    _minutesElapsed = DateTime.UtcNow.Subtract(_startUtc).TotalMinutes;
                }
            }
        }

        public void Start(RecordingInfo info)
        {
            if (info.IntervalTimeInMs < 10 || info.IntervalTimeInMs > 1000) throw new ArgumentOutOfRangeException(string.Format("Interval has to be between 10 and 1000"));
            if (info.MaxDurationInMinutes <= 0 || info.MaxDurationInMinutes > 30) throw new ArgumentOutOfRangeException(string.Format("Duration has to be between 1 and 30"));

            _file = new FileInfo(info.FileName);
            _screenSize.Width = _screenSize.Width % 2 == 0 ? _screenSize.Width : _screenSize.Width - 1;
            _screenSize.Height = _screenSize.Height % 2 == 0 ? _screenSize.Height : _screenSize.Height - 1;
            Timer.Interval = info.IntervalTimeInMs;
            _maxDuration = info.MaxDurationInMinutes;
            var arraySize = ((1000 / Timer.Interval) * _maxDuration) * 60;
            _frames = new RollingList<byte[]>((int)arraySize);
            Timer.Start();
            _startUtc = DateTime.UtcNow;
        }

        private void CreateFile()
        {
            _frameIndex = 0;

            if (_file.Exists) _file.Delete();

            var frameRate = (int)(1000 / (Timer.Interval));
            _writer = new VideoFileWriter();
            _writer.Open(_file.FullName, _screenSize.Width, _screenSize.Height, frameRate, VideoCodec.MPEG4);

            foreach (var frame in _frames)
            {
                using (var stream = new MemoryStream(frame))
                {
                    var img = Image.FromStream(stream);
                    _writer.WriteVideoFrame(new Bitmap(img));
                }
                _frameIndex++;
            }

            _writer.Close();
        }

        public void Stop()
        {
            Timer.Enabled = false;
            Timer.Stop();
            Timer.Dispose();
            System.Threading.Thread.Sleep(1000);
            CreateFile();
            OnStopped(new EventArgs());
        }
    }
}
