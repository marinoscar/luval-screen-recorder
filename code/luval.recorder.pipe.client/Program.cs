using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace luval.recorder.pipe.client
{
    class Program
    {
        static void Main(string[] args)
        {
            var pipeName = "";
            Console.WriteLine("Enter the message to send to pipe {0}", pipeName);
            var message = Console.ReadLine();
            var result = SendMessageToPipe(pipeName, message, 10000, 5, 2000);
        }

        private static bool SendMessageToPipe(string pipe, string message, int timeoutInMs, int connectionRetries, int retryWaitInMs)
        {
            var success = false;
            using (var client = new NamedPipeClientStream(pipe))
            {
                client.Connect(timeoutInMs);
                using (var writer = new StreamWriter(client))
                {
                    
                    for (int i = 0; i < connectionRetries; i++)
                    {
                        if (client.IsConnected)
                        {
                            writer.WriteLine(message);
                            try
                            {
                                writer.Flush();
                                success = true;
                                break;
                            }
                            catch (Exception ex)
                            {
                                Trace.TraceError("Server unavailable: {0}", ex);
                            }
                        }
                        if(!success)
                            Thread.Sleep(retryWaitInMs);
                    }
                    if (!success)
                        Trace.TraceError("Failed after {0} retries", connectionRetries);
                }
            }
            return success;
        }
    }
}
