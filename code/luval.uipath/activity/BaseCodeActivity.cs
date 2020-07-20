using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace luval.uipath.recorder
{
    public abstract class BaseCodeActivity : CodeActivity
    {
        [Category("Input")]
        [Description("Max duration in minutes that the operation can take to complete")]
        [RequiredArgument]
        public InArgument<int> TimeoutInMinutes { get; set; }

        [Category("Output")]
        [Description("Indicates if there was a success")]
        public OutArgument<bool> WasSuccess { get; set; }
        [Category("Output")]
        [Description("Indicates if operation timeout")]
        public OutArgument<bool> WasTimeout { get; set; }
        [Category("Output")]
        [Description("Message with the result of the execution")]
        public OutArgument<string> Message { get; set; }

        /// <summary>
        /// Encapsulates the execution of the activity in a timebound code block
        /// </summary>
        /// <param name="doFunc">The function that would be executed in a time block, true if it was succesful, false otherwise</param>
        /// <param name="context">The execution context</param>
        protected virtual void DoExecute(Func<bool> doFunc, CodeActivityContext context)
        {
            var timeoutVal = TimeoutInMinutes.Get(context);
            if (timeoutVal == 0) timeoutVal = 15;

            var task = Task.Run(() => { return doFunc(); });

            try
            {
                var waitResult = task.Wait(TimeSpan.FromMinutes(timeoutVal));
                WasTimeout.Set(context, waitResult);
                var executionResult = task.Result;
                WasSuccess.Set(context, waitResult && executionResult);
                if(waitResult) Message.Set(context, "Operation timed out");
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failed to complete operation with {0}", ex);
                WasSuccess.Set(context, false);
                Message.Set(context, ex.ToString());
            }
        }
    }
}
