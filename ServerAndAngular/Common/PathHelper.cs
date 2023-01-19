using System;
using System.IO;
using System.Threading;

namespace SoupDiscover.Common
{
    public static class PathHelper
    {
        /// <summary>
        /// Remove recursively a directory and try several times
        /// </summary>
        /// <param name="directory">The directory to delete</param>
        /// <param name="retryCount">The retry count</param>
        public static void DeleteDirectory(string directory, int retryCount = 10)
        {
            var random = new Random();
            var nbRetry = retryCount;
            if (!Directory.Exists(directory))
            {
                return;
            }

            var isDeleted = false;
            while (!isDeleted && nbRetry > 0)
            {
                try
                {
                    Directory.Delete(directory, true);
                    isDeleted = true;
                }
                catch (Exception e) when (e is IOException or UnauthorizedAccessException)
                {
                    foreach (var f in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
                    {
                        File.SetAttributes(f, FileAttributes.Normal); // Set all file in normal mode
                    }
                    nbRetry--;
                    // Try to set all file in normal mode
                    Thread.Sleep(random.Next(900) + 100);
                }
            }
        }
    }
}
