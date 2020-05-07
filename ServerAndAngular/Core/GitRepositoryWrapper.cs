using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SoupDiscover.Core
{
    /// <summary>
    /// Wrapper to Git to clone files to a directory
    /// </summary>
    internal class GitRepositoryWrapper : RepositoryWrapper
    {
        public GitRepositoryWrapper(string urlRepository, string branch, string sshKeyId, string sshKey, string sshKeyFilename)
        {
            _urlRepository = urlRepository;
            _branch = branch;
            _sshKeyId = sshKeyId;
            _sshKey = sshKey;
            _sshKeyFilename = sshKeyFilename;
        }

        private const string _gitCommand = "git";
        private readonly string _urlRepository;
        private readonly string _branch;
        private readonly string _sshKeyId;
        private readonly string _sshKey;
        private readonly string _sshKeyFilename;

        /// <summary>
        /// Check if Git is installed
        /// </summary>
        /// <returns></returns>
        private bool CheckGitInstalled()
        {
            return true;
        }

        private static string CreateSShDirectory()
        {
            var sshDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (sshDir == null)
            {
                throw new ApplicationException("Unable to find the UserProfile directory!");
            }
            sshDir = Path.Combine(sshDir, ".ssh");
            if (!Directory.Exists(sshDir))
            {
                Directory.CreateDirectory(sshDir);
            }
            return sshDir;
        }

        /// <summary>
        /// Create the ssh key to %Home%/.ssh
        /// </summary>
        private string CreateSshKeyFile()
        {
            var sshDir = CreateSShDirectory();
            var sshFilename = Path.Combine(sshDir, _sshKeyFilename);
            if(!File.Exists(sshFilename))
            {
                File.WriteAllText(sshFilename, _sshKey);
            }
            return sshFilename;
        }

        private bool AddSshKey()
        {
            // https://medium.com/@xiaolishen/use-multiple-ssh-keys-for-different-github-accounts-on-the-same-computer-7d7103ca8693
            var sshConfigFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".ssh", "config");
            var config = new ConfigFile(sshConfigFile);
            
            config.Add($"Host {HostName}-{_sshKeyId}", "StrictHostKeyChecking no");
            config.Add($"Host {HostName}-{_sshKeyId}", $"HostName {HostName}");
            config.Add($"Host {HostName}-{_sshKeyId}", $"User git");
            config.Add($"Host {HostName}-{_sshKeyId}", $"IdentityFile {_sshKeyFilename}");
            return config.Save();
        }

        /// <summary>
        /// Return the hostname of the url
        /// </summary>
        protected string HostName
        {
            get
            {
                return "coucou";
            }
        }


        public override void CopyTo(string path)
        {
            // Create the ssh key to clone the repository
            CreateSshKeyFile();
            AddSshKey();
            if(!CheckGitInstalled())
            {
                throw new ApplicationException($"Unable to find command {_gitCommand}");
            }
            Process.Start(_gitCommand, $"clone -b {_branch} --depth 1 {_urlRepository}").WaitForExit();
        }
    }
}
