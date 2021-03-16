using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace luval.recorder
{
    /// <summary>
    /// Provides an abstraction to handle common console switches
    /// </summary>
    public class ConsoleSwitches
    {
        private List<string> _args;

        /// <summary>
        /// Creates an instance of the class
        /// </summary>
        /// <param name="args">Collection of arguments</param>
        public ConsoleSwitches(IEnumerable<string> args)
        {
            _args = new List<string>(args);
        }

        /// <summary>
        /// Gets the value for the switch in the argument collection
        /// </summary>
        /// <param name="name">The name of the switch</param>
        /// <returns>The switch value if present, otherwise null</returns>
        public string this[string name]
        {
            get
            {
                var idx = _args.IndexOf(name);
                if (idx == (_args.Count - 1)) return null;
                return _args[idx + 1];
            }
        }

        /// <summary>
        /// Indicates if the switch exists in the argument collection
        /// </summary>
        /// <param name="name">The name of the switch</param>
        /// <returns>True if the switch name is on the colleciton, otherwise false</returns>
        public bool ContainsSwitch(string name)
        {
            return _args.Contains(name);
        }

        public RecordingInfo ToRecordingInfo()
        {
            var res = new RecordingInfo();
            if (ContainsSwitch("/session")) res.SessionName = this["/session"];
            if (ContainsSwitch("/outputFile")) res.FileName = this["/outputFile"];
            if (ContainsSwitch("/isRolling")) res.IsRollingFile = !(!string.IsNullOrEmpty(this["/isRolling"]) && this["/isRolling"].ToLowerInvariant() == "false");
            if (ContainsSwitch("/duration")) res.RollingDurationInMinutes = Convert.ToInt32(this["/duration"]);
            if (ContainsSwitch("/interval")) res.IntervalTimeInMs = Convert.ToInt32(this["/interval"]);
            if (ContainsSwitch("/maxRecording")) res.MaxRecordingMinutes = Convert.ToInt32(this["/maxRecording"]);
            if (ContainsSwitch("/useShareFile")) res.UseShareFile = (!string.IsNullOrWhiteSpace(this["/useShareFile"]) && (new[] { "yes", "true", "1", "y", "t"}).Contains(this["/useShareFile"].ToLowerInvariant()));
            if (ContainsSwitch("/windowMode") && (new[] {0, 5, 6 }).Contains(Convert.ToInt32(this["/windowMode"])))
            {
                res.WindowMode = Convert.ToInt32(this["/windowMode"]);
            }
            res.LogInfoMessages = ContainsSwitch("/logInfo");
            return res;
        }
    }
}
