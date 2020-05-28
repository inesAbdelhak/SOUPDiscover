using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

namespace SoupDiscover.Common
{
    public static class ProcessHelper
    {
        /// <summary>
        /// Create and start a process
        /// </summary>
        /// <param name="logger">Th logger to log StandardOutput and StandardError</param>
        /// <param name="filename"></param>
        /// <param name="arguments"></param>
        /// <param name="workingDirectory"></param>
        /// <returns></returns>
        public static (int ExitCode, string ErrorMessage) ExecuteAndLog(ILogger logger, string filename, string arguments, string workingDirectory = null)
        {
            var logs = new StringBuilder();
            using var process = new Process();
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
            process.OutputDataReceived += (o, e) => logger.LogDebug(e.Data);
            process.ErrorDataReceived += (o, e) => { logs.AppendLine(e.Data); logger.LogWarning(e.Data); };
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            return (ExitCode: process.ExitCode, ErrorMessage: logs.ToString());
        }
    }
}
