﻿using System;
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

        /// <summary>
        /// Creates a new instance of the class with a serialized string
        /// </summary>
        /// <param name="value">String to deserialize</param>
        /// <returns>A new instance of <see cref="ProcessShare"/></returns>
        public static ProcessShareData FromString(string value)
        {
            var res = new ProcessShareData();
            var lines = value.Split(Environment.NewLine.ToCharArray());
            foreach (var item in lines)
            {
                if (string.IsNullOrWhiteSpace(item) || !item.Contains(";")) continue;
                var content = item.Split(";".ToCharArray());
                if (content.Length <= 1) continue;
                res[content[0]] = content[1];
            }
            return res;
        }
    }
}
