using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace luval.uipath.recorder
{
    public static class Utils
    {
        public static string ProcessName { get { return "luval.recorder.exe";  } }

        public static Process GetProcess()
        {
            var processes = Process.GetProcesses().Where(i => i.ProcessName.ToLowerInvariant().Equals(ProcessName.ToLowerInvariant()));
            if (!processes.Any()) return null;
            foreach(var p in processes)
            {
                var owner = GetProcessOwner(p.Id);
                if (Environment.UserName.Contains(owner)) return p;
            }
            return processes.First();
        }

        public static string GetProcessOwner(int processId)
        {
            string query = "Select * From Win32_Process Where ProcessID = " + processId;
            var searcher = new ManagementObjectSearcher(query);
            var processList = searcher.Get();

            foreach (ManagementObject obj in processList)
            {
                string[] argList = new string[] { string.Empty, string.Empty };
                int returnVal = Convert.ToInt32(obj.InvokeMethod("GetOwner", argList));
                if (returnVal == 0)
                {
                    // return DOMAIN\user
                    return argList[1] + "\\" + argList[0];
                }
            }

            return "NO OWNER";
        }

        public static string GetSessionName()
        {
            return string.Format("LUVAL-RECORDING-{0}-{1}", Environment.MachineName, Environment.UserName);
        }
    }
}
