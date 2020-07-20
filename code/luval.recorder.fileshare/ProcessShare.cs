using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace luval.recorder.fileshare
{
    public class ProcessShare
    {

        private readonly FileInfo _fileInfo;


        /// <summary>
        /// Creates a new instance of the class
        /// </summary>
        /// <param name="sessionName">The name of the session to share data across processes</param>
        public ProcessShare(string sessionName) : this(sessionName, TimeSpan.FromSeconds(60), 3, 1000)
        {
        }

        /// <summary>
        /// Creates a new instance of the class
        /// </summary>
        /// <param name="sessionName">The name of the session to share data across processes</param>
        /// <param name="timeout">How long it would wait for the operations to be completed</param>
        public ProcessShare(string sessionName, TimeSpan timeout) : this(sessionName, timeout, 5, 3000)
        {
        }

        /// <summary>
        /// Creates a new instance of the class
        /// </summary>
        /// <param name="sessionName">The name of the session to share data across processes</param>
        /// <param name="timeout">How long it would wait for the operations to be completed</param>
        /// <param name="retryCount">How many times would it retry if an error is found, like a file being block during an operation</param>
        public ProcessShare(string sessionName, TimeSpan timeout, int retryCount) : this(sessionName, timeout, retryCount, 3000)
        {
        }

        /// <summary>
        /// Creates a new instance of the class
        /// </summary>
        /// <param name="sessionName">The name of the session to share data across processes</param>
        /// <param name="timeout">How long it would wait for the operations to be completed</param>
        /// <param name="retryCount">How many times would it retry if an error is found, like a file being block during an operation</param>
        /// <param name="waitBetweenRetryInMs">How much time to wait in ms for the next retry</param>
        public ProcessShare(string sessionName, TimeSpan timeout, int retryCount, int waitBetweenRetryInMs)
        {
            SessionName = sessionName;
            Timeout = timeout;
            RetryCount = retryCount;
            WaitBetweenRetryInMs = waitBetweenRetryInMs;
            var dirInfo = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Luval-Recording"));
            if (!dirInfo.Exists) dirInfo.Create();
            _fileInfo = new FileInfo(Path.Combine(dirInfo.FullName, string.Format("{0}-PSI.bin", sessionName)));
        }

        public string SessionName { get; private set; }
        public TimeSpan Timeout { get; private set; }
        public int RetryCount { get; private set; }
        public int WaitBetweenRetryInMs { get; private set; }


        /// <summary>
        /// Writes a message to the shared file
        /// </summary>
        /// <param name="message">The message to write</param>
        /// <returns>True if succesful</returns>
        public bool WriteMessage(string message)
        {
            return (bool)ExecuteWithTimeout(() =>
            {
                return DoWriteMessageWithRetry(message);
            });
        }

        /// <summary>
        /// Wait for the file to contain text in it to continue the execution
        /// </summary>
        /// <param name="expectedText">The expected text</param>
        /// <param name="timeout">The max time to wait before sending an exception</param>
        /// <exception cref="TimeoutException">After the operation is not finding the text</exception>
        public void WaitForText(string expectedText, TimeSpan timeout)
        {
            WaitForText(expectedText, timeout, 1000);
        }

        /// <summary>
        /// Wait for the file to contain text in it to continue the execution
        /// </summary>
        /// <param name="expectedText">The expected text</param>
        /// <param name="timeout">The max time to wait before sending an exception</param>
        /// <param name="waitCycleInMs">The wait between every check</param>
        /// <exception cref="TimeoutException">After the operation is not finding the text</exception>
        public void WaitForText(string expectedText, TimeSpan timeout, int waitCycleInMs)
        {
            var startUtc = DateTime.UtcNow;
            while (true)
            {
                var text = DoReadMessage();
                if (!string.IsNullOrWhiteSpace(text) && text.Trim().ToLowerInvariant().Equals(expectedText.Trim().ToLowerInvariant()))
                {
                    if(File.Exists(_fileInfo.FullName))
                        _fileInfo.Delete();
                    return;
                }
                if (timeout < DateTime.UtcNow.Subtract(startUtc)) throw new TimeoutException(string.Format("Unable to complete the find {0} in file {1} after waiting {2}.", expectedText, _fileInfo.FullName, timeout));
                Thread.Sleep(waitCycleInMs);
            }
        }


        private string DoReadMessageWithRetry()
        {
            var text = default(string);
            RetryBlock(() =>
            {
                text = DoReadMessage();
                return !string.IsNullOrEmpty(text);
            });
            return text;
        }

        private string DoReadMessage()
        {
            var result = default(string);
            if (!File.Exists(_fileInfo.FullName)) return result;
            try
            {
                using (var stream = new StreamReader(_fileInfo.FullName))
                {
                    result = stream.ReadToEnd();
                    Trace.TraceInformation("Message recieved: {0}", result);
                    stream.Close();
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(string.Format("Unable to read file: {0} with exception: {1}", _fileInfo.FullName, ex.Message));
            }
            return result;
        }

        private object ExecuteWithTimeout(Func<object> doFunc)
        {
            object result = null;
            var task = Task.Run(() =>
            {
                result = doFunc();
            });
            if (!task.Wait(Timeout)) throw new TimeoutException(string.Format("Unable to complete the operation on time after waiting {0}", Timeout));
            return result;
        }

        private bool DoWriteMessageWithRetry(string message)
        {
            return RetryBlock(() =>
            {
                return DoWriteMessage(message);
            });
        }

        private bool DoWriteMessage(string message)
        {
            try
            {
                if (_fileInfo.Exists) _fileInfo.Delete();
                using (var stream = new StreamWriter(_fileInfo.FullName, false))
                {
                    stream.WriteLine(message);
                    stream.Close();
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failed to write to file {0}: {1}", _fileInfo.FullName, ex.Message);
                return false;
            }
            return true;
        }

        private bool RetryBlock(Func<bool> doFunc)
        {
            for (int i = 0; i < RetryCount; i++)
            {
                if (doFunc()) return true;
            }
            return false;
        }

    }
}
