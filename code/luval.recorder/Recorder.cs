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
    public class Recorder : IDisposable
    {
        private Size _screenSize;
        private RecordingInfo _info;
        private VideoFileWriter _writer;

        public RollingList<byte[]> Frames { get; private set; }
        public Timer Timer { get; private set; }

        public Recorder()
        {
            Timer = new Timer();
            Timer.Elapsed += Timer_Tick;
            _screenSize = new Size(System.Windows.Forms.SystemInformation.VirtualScreen.Width,
                System.Windows.Forms.SystemInformation.VirtualScreen.Height);
        }


        private void Timer_Tick(object sender, EventArgs e)
        {

            if (_info.IsRollingFile)
                RecordRollingFile();
            else
                RecordToWriter();
        }


        private byte[] GetScreenBytes()
        {
            var res = new List<byte>();
            var rec = new Rectangle(0, 0, _screenSize.Width, _screenSize.Height);
            using (var bp = new Bitmap(rec.Width, rec.Height))
            {
                using (Graphics g = Graphics.FromImage(bp))
                {
                    g.CopyFromScreen(rec.X, rec.Y, 0, 0, bp.Size, CopyPixelOperation.SourceCopy);
                    using (var stream = new MemoryStream())
                    {
                        bp.Save(stream, ImageFormat.Jpeg);
                        res.AddRange(stream.ToArray());
                    }
                }
            }
            return res.ToArray();
        }

        private void RecordToWriter()
        {
            if (_writer == null)
                _writer = CreateVideoWriter();
            var img = Image.FromStream(new MemoryStream(GetScreenBytes()));
            _writer.WriteVideoFrame(new Bitmap(img));
        }

        private void RecordRollingFile()
        {
            Frames.Add(GetScreenBytes());
        }


        public void Start(RecordingInfo info)
        {
            if (info.IntervalTimeInMs < 10 || info.IntervalTimeInMs > 1000) throw new ArgumentOutOfRangeException(string.Format("Interval has to be between 10 and 1000"));
            if (info.RollingDurationInMinutes <= 0 || info.RollingDurationInMinutes > 30) throw new ArgumentOutOfRangeException(string.Format("Duration has to be between 1 and 30"));
            if (info.UseShareFile && string.IsNullOrWhiteSpace(info.SessionName)) throw new ArgumentOutOfRangeException(string.Format("Session name cannot be null or empty if named pipes are going to be used"));

            _info = info;

            //the recorder requires the screen size to not be an odd number
            _screenSize.Width = _screenSize.Width % 2 == 0 ? _screenSize.Width : _screenSize.Width - 1;
            _screenSize.Height = _screenSize.Height % 2 == 0 ? _screenSize.Height : _screenSize.Height - 1;

            Timer.Interval = info.IntervalTimeInMs;
            var arraySize = ((1000 / Timer.Interval) * info.RollingDurationInMinutes) * 60;
            Frames = new RollingList<byte[]>((int)arraySize);
            Timer.Start();
        }

        private void SaveRollingFile()
        {
            using (var writer = CreateVideoWriter())
            {
                foreach (var frame in Frames)
                {
                    using (var stream = new MemoryStream(frame))
                    {
                        var img = Image.FromStream(stream);
                        writer.WriteVideoFrame(new Bitmap(img));
                    }
                }
                writer.Close();
            }
        }

        private VideoFileWriter CreateVideoWriter()
        {
            var frameRate = (int)(1000 / (Timer.Interval));
            var writer = new VideoFileWriter();
            writer.Open(_info.FileName, _screenSize.Width, _screenSize.Height, frameRate, VideoCodec.MPEG4);
            return writer;
        }

        public void Stop()
        {
            Timer.Enabled = false;
            Timer.Stop();
            Timer.Dispose();
            System.Threading.Thread.Sleep(1000);
            if (_info.IsRollingFile)
                SaveRollingFile();
            else
                CloseWriter();
        }

        private void CloseWriter()
        {
            if (_writer == null) return;
            if(_writer.IsOpen)
                _writer.Close();
            _writer.Dispose();
            _writer = null;
        }

        public void Dispose()
        {
            CloseWriter();
            Frames = null;
            Timer.Dispose();
            Timer = null;
        }
    }
}
