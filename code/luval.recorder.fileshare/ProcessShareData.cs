using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace luval.recorder.fileshare
{
    public class ProcessShareData : Dictionary<string, string>
    {
        public override string ToString()
        {
            return Serialize();
        }

        private string Serialize()
        {
            var lines = new List<string>();
            foreach (var item in this)
            {
                lines.Add(string.Format("{0};{1}", item.Key.Replace(";", "&^!"), item.Value.Replace(";","&^!")));
            }
            return string.Join(Environment.NewLine, lines);
        }
    }
}
