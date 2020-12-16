using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using SoupDiscover.Common;
using SoupDiscover.ORM;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace SoupDiscover.Core.Respository
{
    /// <summary>
    /// To clone and checkout Git repository to a directory
    /// </summary>
    internal class GitRepositoryManager : RepositoryManager
    {
        private static readonly string[] _gitUrlStringRegex = new string[]
        {
            @"^(?<protocol>git)@(?<hostname>[^:]+):(?<organization>[^\/]+)\/(?<repository>[^\/]+)\.git$",
            @"^(?<protocol>https?):\/\/(?<hostname>[^\/]+)\/(?<organization>[^\/]+)\/(?<repository>[^\/]+)\.git$"
        };
        private static Regex[] _gitUrlRegex;
        private const string _gitCommand = "git";
        private readonly ILogger<GitRepositoryManager> _logger;
        private readonly string _urlRepository;
        private readonly string _branch;
        private readonly Credential _credential;
        
        static GitRepositoryManager()
        {
            _gitUrlRegex = _gitUrlStringRegex.Select(x => new Regex(x)).ToArray();
        }

        public GitRepositoryManager(ILogger<GitRepositoryManager> logger, string urlRepository, string branch, Credential credential)
        {
            _logger = logger;
            _urlRepository = urlRepository;
            _branch = branch;
            _credential = credential;
            ParseRepositoryUrl();
        }

        /// <summary>
        /// Check if Git is installed
        /// </summary>
        private bool CheckGitInstalled()
        {
            return true;
        }

        /// <summary>
        /// Parse the url to extract hostname organization name and repository name
        /// </summary>
        private (string Protocol, string Hostname, string Organisation, string Repository) ParseRepositoryUrl()
        {
            var match = _gitUrlRegex.Select(x => x.Match(_urlRepository)).FirstOrDefault(match => match.Success);
            if(match == null)
            {
                throw new ApplicationException($"Unable to parse git url \"{_urlRepository}\"!");
            }
            return new(match.Groups["protocol"].Value, match.Groups["hostname"].Value, match.Groups["organization"].Value, match.Groups["repository"].Value);
        }

        /// <summary>
        /// Clone the repository to the given path
        /// </summary>
        /// <param name="path">The path where clone the repository</param>
        public override void CopyTo(string path, CancellationToken token = default)
        {
            // Create the ssh key to clone the repository
            CloneRepository(path, token);
        }

        /// <summary>
        /// Execute the clone command, and checkout the defined branch.
        /// Clone with depth 1. Don't clone all history.
        /// </summary>
        /// <param name="path">The directory where clone the repository</param>
        private void CloneRepository(string path, CancellationToken token = default)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string repositoryToClone = null;
            var giturl = ParseRepositoryUrl();
            if (giturl.Protocol == "git")
            {
                var substituteHostname = _credential.InstallSSHKey(giturl.Hostname);
                repositoryToClone = $@"git@{substituteHostname}:{giturl.Organisation}/{giturl.Repository}.git";
            }
            else
            {
                repositoryToClone = $@"{giturl.Protocol}://{_credential.Login}:{_credential.Password}@{giturl.Hostname}/{giturl.Organisation}/{giturl.Repository}.git";
            }
            if (!CheckGitInstalled())
            {
                throw new ApplicationException($"Unable to find command {_gitCommand}");
            }

            var result = ProcessHelper.ExecuteAndLog(_gitCommand, $"clone -b {_branch} --depth 1 {repositoryToClone} \"{path}\"", path, _logger, token);
            if(result.ExitCode != 0)
            {
                throw new ApplicationException($"Error on cloning the Git repository {_urlRepository} in path {path}. Error :\r\n {result.ErrorMessage}");
            }
        }
    }
}
