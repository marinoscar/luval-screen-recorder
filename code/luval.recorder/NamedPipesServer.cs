using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.IO;
using System.Threading;

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
        public void Start(int timeoutInMinutes)
        {
            var startTs = DateTime.UtcNow;
            using (var server = new NamedPipeServerStream(_pipeName))
            {
                //server.WaitForConnection();
                Thread.Sleep(2000); //waits for server to start
                using (var stream = new StreamReader(server))
                {
                    while (true)
                    {
                        var line = stream.ReadLine();
                        if(!string.IsNullOrWhiteSpace(line) && line.ToLowerInvariant() == _stopWord)
                        if (DateTime.UtcNow.Subtract(startTs).TotalMinutes > timeoutInMinutes)
                            throw new TimeoutException(string.Format("No message was recieved after {0} minutes", timeoutInMinutes));
                    }
                }
            }
        }
    }
}
