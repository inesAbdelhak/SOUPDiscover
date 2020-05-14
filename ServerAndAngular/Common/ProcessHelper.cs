using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace SoupDiscover.Common
{
    public static class ProcessHelper
    {
        public static int ExecuteAndLog(ILogger logger, string filename, string arguments, string workingDirectory = null)
        {
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
            process.ErrorDataReceived += (o, e) => logger.LogWarning(e.Data);
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            return process.ExitCode;
        }
    }
}
