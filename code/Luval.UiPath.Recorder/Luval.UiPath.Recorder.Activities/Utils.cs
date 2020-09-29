using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Luval.UiPath.Recorder.Activities
{
    public class Utils
    {
        public static string GetSessionName()
        {
            return string.Format("LuvalRecording-{0}-{1}", CleanInput(Environment.MachineName), CleanInput(Environment.UserName));
        }

        public static FileInfo GetVideoFileName(string dir)
        {
            return new FileInfo(Path.Combine(dir, string.Format("{0}.mp4", GetSessionName())));
        }

        public static string GetExecDirLocation()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }

        private static string CleanInput(string value)
        {
            return value.Replace(@"\", "-").Replace(".", "-").Replace("~", "-").Replace(":", "-").Replace("|", "-").Replace("/", "-");
        }
    }
}
