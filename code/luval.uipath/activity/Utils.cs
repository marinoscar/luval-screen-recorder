using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace luval.uipath.recorder
{
    public static class Utils
    {

        public static string GetSessionName()
        {
            return string.Format("LUVAL-RECORDING-{0}-{1}", Environment.MachineName, Environment.UserName);
        }
    }
}
