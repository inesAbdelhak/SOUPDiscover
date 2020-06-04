using System;
using System.IO;
using System.Threading;

namespace SoupDiscover.Core
{
    public static class PathHelper
    {
        /// <summary>
        /// Remove recursively a directory and try several times
        /// </summary>
        /// <param name="directory">The directory to delete</param>
        public static void DeleteDirectory(string directory, int timesToTry = 10)
        {
            var randomizer = new Random();
            var nbRetry = timesToTry;
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
                catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
                {
                    if (nbRetry == 0)
                    {
                        throw e; // all try are used -> return the exception
                    }
                    foreach(var f in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
                    {
                        File.SetAttributes(f, FileAttributes.Normal); // Set all file in normal mode
                    }
                    nbRetry--;
                    // Try to set all file in normal mode
                    Thread.Sleep(randomizer.Next(900) + 100);
                }
            }
        }
    }
}
