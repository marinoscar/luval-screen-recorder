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
            Trace.Listeners.Add(new ConsoleTraceListener());
            var pipeName = "Luval-Recorder-Session";
            Console.WriteLine("Enter the message to send to pipe {0}", pipeName);
            var message = Console.ReadLine();
            var result = SendMessageToPipe(pipeName, message, 30000, 5, 2000);
            if (!result)
            {
                Console.WriteLine("Failed to send message {0} on pipe {1}", message, pipeName);
                return;
            }

            Console.WriteLine("Waiting for completed signal");

            StartServer(pipeName, "complete", 10);

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

        private static void StartServer(string pipeName, string stopWord, int timeoutInMinutes)
        {
            if (string.IsNullOrWhiteSpace(stopWord)) throw new ArgumentException("stopWord cannot be null or empty");
            var startTs = DateTime.UtcNow;
            using (var server = new NamedPipeServerStream(pipeName))
            {
                server.WaitForConnection();
                Thread.Sleep(2000); //waits for server to start
                using (var stream = new StreamReader(server))
                {
                    while (true)
                    {
                        var line = stream.ReadLine();
                        Trace.TraceInformation("Signal recieved {0}", line);
                        if (!string.IsNullOrWhiteSpace(line) && line.ToLowerInvariant() == stopWord.ToLowerInvariant())
                            return;
                        if (DateTime.UtcNow.Subtract(startTs).TotalMinutes > timeoutInMinutes)
                            throw new TimeoutException(string.Format("No message was recieved after {0} minutes", timeoutInMinutes));
                    }
                }
            }
        }
    }
}
