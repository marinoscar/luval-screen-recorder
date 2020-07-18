using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace luval.uipath.recorder
{
    public class EndRecording : CodeActivity
    {
        [Category("Output")]
        [Description("Message that contains the result of the execution of the activity")]
        public OutArgument<string> ResultMessage { get; set; }
        [Category("Output")]
        [Description("Indicates if there was a success")]
        public OutArgument<bool> Success { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            throw new NotImplementedException();
        }
    }
}
