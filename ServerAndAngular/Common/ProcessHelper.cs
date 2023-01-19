﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace SoupDiscover.Common
{
    public static class ProcessHelper
    {
        /// <summary>
        /// Create and start a process
        /// </summary>
        /// <param name="filename">the filename to execute</param>
        /// <param name="arguments">arguments to execute the filename</param>
        /// <param name="workingDirectory">The working directory where start the process</param>
        /// <param name="logger">The logger to log StandardOutput and StandardError</param>
        /// <returns>The exit code and the all messages sent in StandardError stream</returns>
        public static (int ExitCode, string ErrorMessage) ExecuteAndLog(string filename, string arguments, string workingDirectory = null, ILogger logger = null, CancellationToken token = default)
        {
            if (logger == null)
            {
                logger = NullLogger.Instance;
            }
            var logs = new StringBuilder();
            AutoResetEvent exitEvent = null;
            using var process = new Process();
            if (token.CanBeCanceled)
            {
                exitEvent = new AutoResetEvent(false);
                process.EnableRaisingEvents = true;
                process.Exited += (o, e) => exitEvent.Set();
            }
            process.StartInfo.FileName = filename;
            process.StartInfo.Arguments = arguments;
            if (workingDirectory != null)
            {
                process.StartInfo.WorkingDirectory = workingDirectory;
            }
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.OutputDataReceived += (o, e) => logger.LogInformation(e.Data);
            process.ErrorDataReceived += (o, e) => { logs.AppendLine(e.Data); logger.LogWarning(e.Data); };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            if (token.CanBeCanceled)
            {
                int indexWait = WaitHandle.WaitAny(new WaitHandle[] { token.WaitHandle, exitEvent });

                if (indexWait == 0) // The user cancel the processing
                {
                    process.Kill(true);
                }
                token.ThrowIfCancellationRequested();
            }
            else
            {
                process.WaitForExit();
            }
            string messageError = logs.ToString();
            if (string.IsNullOrEmpty(messageError))
            {
                messageError = $"Error on executing file {filename}";
            }
            return (ExitCode: process.ExitCode, ErrorMessage: messageError ?? string.Empty);
        }
    }
}
