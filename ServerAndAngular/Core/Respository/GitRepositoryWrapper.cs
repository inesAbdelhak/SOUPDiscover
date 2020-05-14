using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using SoupDiscover.Common;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SoupDiscover.Core.Respository
{
    /// <summary>
    /// Wrapper to Git to clone files to a directory
    /// </summary>
    internal class GitRepositoryWrapper : RepositoryWrapper
    {
        private const string _gitSSHUrlStringRegex = @"^git@(?<hostname>[^:]+):(?<organization>[^\/]+)\/(?<repository>[^\/]+)\.git$";
        private static Regex _gitSSHUrlRegex;
        private const string _gitCommand = "git";
        private readonly ILogger<GitRepositoryWrapper> _logger;
        private readonly string _urlRepository;
        private readonly string _branch;
        private readonly string _sshKeyId;
        private readonly string _sshKey;
        private readonly string _sshKeyFilename;
        private string _hostname;
        private string _organization;
        private string _repository;

        static GitRepositoryWrapper()
        {
            _gitSSHUrlRegex = new Regex(_gitSSHUrlStringRegex);
        }

        public GitRepositoryWrapper(ILogger<GitRepositoryWrapper> logger, string urlRepository, string branch, string sshKeyId, string sshKey, string sshKeyFilename)
        {
            _logger = logger;
            _urlRepository = urlRepository;
            _branch = branch;
            _sshKeyId = sshKeyId;
            _sshKey = sshKey;
            _sshKeyFilename = sshKeyFilename;
        }

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
            if (!File.Exists(sshFilename))
            {
                File.WriteAllText(sshFilename, _sshKey.Replace("\r\n", "\n"));
            }
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                // Update permission to the ssh key
            }
            return sshFilename;
        }

        private bool AddSshKey()
        {
            // https://medium.com/@xiaolishen/use-multiple-ssh-keys-for-different-github-accounts-on-the-same-computer-7d7103ca8693
            var sshConfigFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh", "config");
            var config = new SshConfigFile(sshConfigFile);

            config.Add($"Host {SubstituteHostname}", "StrictHostKeyChecking no");
            config.Add($"Host {SubstituteHostname}", $"HostName {HostName}");
            config.Add($"Host {SubstituteHostname}", $"User git");
            config.Add($"Host {SubstituteHostname}", $"IdentityFile ~/.ssh/{_sshKeyFilename}");
            return config.Save();
        }

        /// <summary>
        /// Return the hostname of the url
        /// </summary>
        protected string HostName
        {
            get
            {
                ParseUrl();
                return _hostname;
            }
        }

        /// <summary>
        /// The organization parsed in gir url
        /// </summary>
        protected string Organization
        {
            get
            {
                ParseUrl();
                return _organization;
            }
        }

        protected string Repository
        {
            get
            {
                ParseUrl();
                return _repository;
            }
        }

        protected string SubstituteHostname
        {
            get
            {
                ParseUrl();
                return $"{_hostname}-{_sshKeyId}";
            }
        }

        /// <summary>
        /// Parse the url to extract hostename organization name and repository name
        /// </summary>
        private void ParseUrl()
        {
            if (_hostname != null)
            {
                // Is already parsed
                return;
            }
            var match = _gitSSHUrlRegex.Match(_urlRepository);
            if (!match.Success)
            {
                throw new ApplicationException($"Unable to parse git url \"{_urlRepository}\"!");
            }
            _hostname = match.Groups["hostname"].Value;
            _organization = match.Groups["organization"].Value;
            _repository = match.Groups["repository"].Value;
        }

        /// <summary>
        /// Clone the repository to the given path
        /// </summary>
        /// <param name="path">The path where clone the repository</param>
        public override void CopyTo(string path)
        {
            // Create the ssh key to clone the repository
            CreateSshKeyFile();
            AddSshKey();
            if (!CheckGitInstalled())
            {
                throw new ApplicationException($"Unable to find command {_gitCommand}");
            }
            CloneRepository(path);
        }

        private void CloneRepository(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var repositoryToClone = $@"git@{SubstituteHostname}:{Organization}/{Repository}.git";
            ProcessHelper.ExecuteAndLog(_logger, _gitCommand, $"clone -b {_branch} --depth 1 {repositoryToClone} \"{path}\"", path);
        }
    }
}
