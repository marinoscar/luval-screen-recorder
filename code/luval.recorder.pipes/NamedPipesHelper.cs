using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace luval.recorder.pipes
{
    /// <summary>
    /// Provides an abstraction to work with named pipes and make easier the communication between processes
    /// </summary>
    public class NamedPipesHelper
    {
        public NamedPipesHelper(string pipeName, TimeSpan timeout)
        {
            PipeName = pipeName;
            Timeout = timeout;
        }

        /// <summary>
        /// Gets name of the pipe
        /// </summary>
        public string PipeName { get; private set; }
        /// <summary>
        /// Gets the <see cref="TimeSpan"/> that would wait for the operations to complete
        /// </summary>
        public TimeSpan Timeout { get; private set; }


        /// <summary>
        /// Sends a message to the pipe
        /// </summary>
        /// <param name="message">Meessage to send</param>
        /// <returns>True if the message was succesful</returns>
        /// <exception cref="TimeoutException">In case the operation cannot be completed before the provided timeout in the class</exception>
        public bool SendMessage(string message)
        {
            return SendMessage(message, 1, 1);
        }

        /// <summary>
        /// Sends a message to the pipe
        /// </summary>
        /// <param name="message">Meessage to send</param>
        /// <param name="connectionRetries">If the server is not available it would retry</param>
        /// <param name="retryWaitInMs">The time in millisecond to wait after each try</param>
        /// <returns>True if the message was succesful</returns>
        /// <exception cref="TimeoutException">In case the operation cannot be completed before the provided timeout in the class</exception>
        public bool SendMessage(string message, int connectionRetries, int retryWaitInMs)
        {
            var success = false;
            var pipeTask = Task.Run(() =>
            {
                using (var client = new NamedPipeClientStream(PipeName))
                {
                    for (int i = 0; i < connectionRetries; i++)
                    {
                        try
                        {
                            using (var writer = new StreamWriter(client))
                            {
                                if (client.IsConnected)
                                {

                                    var writeTask = Task.Run(() =>
                                    {
                                        writer.WriteLine(message);
                                        writer.Flush();
                                    });
                                    if (!writeTask.Wait(Timeout))
                                        throw new TimeoutException(string.Format("Operation was not completed after {0}", Timeout));

                                    success = true;
                                    break;
                                }
                                if (!success)
                                    Thread.Sleep(retryWaitInMs);
                            }
                        }
                        catch (Exception ex)
                        {
                            Trace.TraceError("Server unavailable: {0}", ex);
                        }
                    }
                    client.Connect(Convert.ToInt32(Timeout.TotalMilliseconds));
                    if (!success)
                        Trace.TraceError("Failed after {0} retries", connectionRetries);
                }
            });

            if (!pipeTask.Wait(Timeout))
                throw new TimeoutException(string.Format("Operation was not completed after {0}", Timeout));

            return success;
        }

    /// <summary>
    /// Reads the stream of the pipe
    /// </summary>
    /// <param name="callback">A function that will process the stream from the pipe, if the result is true it would stop waiting for the stream if false would continue reading the stream</param>
    public void ReadPipe(Func<string, bool> callback)
    {
        using (var server = new NamedPipeServerStream(PipeName))
        {
            var line = string.Empty;
            while (true)
            {
                Trace.TraceInformation("Waiting for signal in {0}", PipeName);
                var readTask = Task.Run(() =>
                {
                    line = ReadStream(server);
                });
                if (!readTask.Wait(Timeout))
                    throw new TimeoutException(string.Format("Operation was not completed after {0}", Timeout));

                Trace.TraceInformation("Signal on pipe {0} recieved: {1}", PipeName, line);

                var result = callback(line);

                Trace.TraceInformation("The result from the callback was: {0}", result);

                if (result) return;
            }
        }
    }

    /// <summary>
    /// Opens the stream from the pipe reads it and then it disconnects
    /// </summary>
    /// <param name="pipeServer">The pipe server stream</param>
    /// <returns>The value of the stream</returns>
    private static string ReadStream(NamedPipeServerStream pipeServer)
    {
        string line;
        pipeServer.WaitForConnection();
        StreamReader stream = new StreamReader(pipeServer);
        line = stream.ReadToEnd();
        pipeServer.Disconnect();
        return line;
    }
}
}
