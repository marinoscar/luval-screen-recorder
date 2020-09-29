using luval.recorder.fileshare;
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
            var sessionName = "LUVAL-RECORDING";
            Console.WriteLine("Enter the message to send to file session {0}", sessionName);
            var message = Console.ReadLine();
            var fileShareSend = new ProcessShare(sessionName);
            var result = fileShareSend.WriteMessage(message);

            if (!result)
            {
                Console.WriteLine("Failed to send message {0} on session file {1}", message, sessionName);
                return;
            }
            Console.WriteLine("Message succesful");

            Console.WriteLine("Waiting for completed signal");

            var fileShareRec = new ProcessShare(sessionName + "_BACK");

            fileShareRec.WaitForText("complete", TimeSpan.FromMinutes(2));

            Console.WriteLine("Process completed");

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
                        if (!success)
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

            Console.WriteLine("Creating server {0}", DateTime.Now);
            using (var server = new NamedPipeServerStream(pipeName))
            {
                Console.WriteLine("Created server at {0}", DateTime.Now);

                var line = string.Empty;
                while (true)
                {
                    Trace.TraceInformation("Waiting for signal");
                    Console.WriteLine("Waiting started at {0}", DateTime.Now);
                    var task = Task.Run(() =>
                    {
                        line = ReadStream(server);
                    });
                    Console.WriteLine("Task Status: {0} at {1}", task.Status, DateTime.Now);
                    if (!task.Wait(TimeSpan.FromMinutes(timeoutInMinutes)))
                        throw new TimeoutException(string.Format("Operation was not completed after {0} minutes", timeoutInMinutes));

                    Trace.TraceInformation("Signal recieved {0}", line);
                    Console.WriteLine("Signal recieved: {0} at {1}", line, DateTime.Now);
                    Console.WriteLine("Task Status: {0} at {1}", task.Status, DateTime.Now);

                    if (!string.IsNullOrWhiteSpace(line) && line.ToLowerInvariant() == stopWord.ToLowerInvariant())
                    {
                        Console.WriteLine("********** SUCCESS!!!!!! ********** ");
                        return;
                    }
                }
            }
        }

        private static string ReadStream(NamedPipeServerStream pipe)
        {
            string line;
            pipe.WaitForConnection();
            StreamReader sr = new StreamReader(pipe);
            line = sr.ReadToEnd();
            pipe.Disconnect();
            return line;
        }
    }
}
