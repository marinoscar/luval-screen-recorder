using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace luval.recorder
{
    /// <summary>
    /// Provides an implementation of the named pipes server
    /// </summary>
    public class NamedPipesServer
    {

        private string _pipeName;
        private string _stopWord;

        /// <summary>
        /// Creates a new instance of the named piped server
        /// </summary>
        /// <param name="pipeName">The name of the pipe</param>
        /// <param name="stopWord">The message expected to close the connection</param>
        public NamedPipesServer(string pipeName, string stopWord)
        {
            if (string.IsNullOrWhiteSpace(pipeName)) throw new ArgumentNullException("pipeName cannot be null or empty");
            if (string.IsNullOrWhiteSpace(stopWord)) throw new ArgumentNullException("stopWord cannot be null or empty");

            _pipeName = pipeName;
            _stopWord = stopWord.ToLowerInvariant();
        }

        /// <summary>
        /// Starts the server and will stop only until the stop word is recieved or the timeout is completed
        /// </summary>
        /// <param name="timeoutInMinutes">The number of muinutes until the timeout is met</param>
        public void StartServer(int timeoutInMinutes)
        {
            var startTs = DateTime.UtcNow;
            using (var server = new NamedPipeServerStream(_pipeName))
            {
                server.WaitForConnection();
                Thread.Sleep(2000); //waits for server to start
                using (var stream = new StreamReader(server))
                {
                    while (true)
                    {
                        var line = stream.ReadLine();
                        Trace.TraceInformation("Signal recieved {0}", line);
                        if (!string.IsNullOrWhiteSpace(line) && line.ToLowerInvariant() == _stopWord)
                            return;
                        if (DateTime.UtcNow.Subtract(startTs).TotalMinutes > timeoutInMinutes)
                            throw new TimeoutException(string.Format("No message was recieved after {0} minutes", timeoutInMinutes));
                    }
                }
            }
        }


        /// <summary>
        /// Sends a message back to the pipe
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <param name="timeoutInMs">The timeout time in milliseconds</param>
        /// <param name="connectionRetries">Number of retries in case of a connection failure</param>
        /// <param name="retryWaitInMs">Number of milliseconds to wait before a retry take place</param>
        /// <returns>True of the message was sent, otherwise false</returns>
        public bool SendMessageToPipe(string message, int timeoutInMs, int connectionRetries, int retryWaitInMs)
        {
            var success = false;
            using (var client = new NamedPipeClientStream(_pipeName))
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
    }
}
