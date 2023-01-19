﻿using SoupDiscover.Common;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using SoupDiscover.Core.Repository;

namespace SoupDiscover.ORM
{
    /// <summary>
    /// A token used to authenticate from an api
    /// </summary>
    public class Credential : EntityObject
    {
        /// <summary>
        /// The name given to the credential
        /// </summary>
        [Key]
        public string Name { get; set; }

        /// <summary>
        /// The token
        /// </summary>
        public string Key { get; set; }

        public string Login { get; set; }

        public string Password { get; set; }

        public string Token { get; set; }

        public CredentialType CredentialType { get; set; }

        /// <summary>
        /// Update the ssh config file to define the key to used to clone the repository
        /// </summary>
        private bool AddSshKey(string hostname)
        {
            // https://medium.com/@xiaolishen/use-multiple-ssh-keys-for-different-github-accounts-on-the-same-computer-7d7103ca8693
            var sshConfigFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh", "config");
            var config = new SshConfigFile(sshConfigFile);
            var substituteHostname = SubstituteHostname(hostname);

            // define a substitute hostname to define an ssh private key for each repository. Even if there are several repositories on a same hotname.
            config.Add($"Host {substituteHostname}", "StrictHostKeyChecking no");
            config.Add($"Host {substituteHostname}", $"HostName {hostname}"); // The real hostname
            config.Add($"Host {substituteHostname}", $"User git"); // always git user, for git repositories
            config.Add($"Host {substituteHostname}", $"IdentityFile ~/.ssh/{SSHKeyFilename}");
            return config.Save();
        }

        /// <summary>
        /// The hostname to use to clone the repository.
        /// It is not the same that given by user.
        /// This permit to define a ssh private key for each repository of a same real hostname.
        /// </summary>
        private string SubstituteHostname(string hostname)
        {
            return $"{hostname}-{Name}";
        }

        private string SSHKeyFilename => $"sshgitkey{Name}";

        /// <summary>
        /// Create the ssh key file to %Home%/.ssh
        /// and update permission to the file.
        /// </summary>
        private string CreateSshKeyFile()
        {
            var sshDir = CreateSShDirectory();
            var sshFilename = Path.Combine(sshDir, SSHKeyFilename);
            if (!File.Exists(sshFilename))
            {
                File.WriteAllText(sshFilename, Key.Replace("\r\n", "\n"));
            }
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                // Update permission to the ssh key (Just for Linux)
                ProcessHelper.ExecuteAndLog("chmod", $"600 {sshFilename}");
            }
            return sshFilename;
        }

        private static string CreateSShDirectory()
        {
            var sshDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            SoupDiscoverException.ThrowIfNull(sshDir, "Unable to find the UserProfile directory!");            
            sshDir = Path.Combine(sshDir, ".ssh");
            if (!Directory.Exists(sshDir))
            {
                Directory.CreateDirectory(sshDir);
            }
            return sshDir;
        }

        /// <summary>
        /// Create the ssh key to the machine
        /// and return the substitute hostname that corresponding with the ssh private key file.
        /// </summary>
        /// <param name="hostname">The real hostname of the repository</param>
        public string PrepareAndGetSubstituteHostname(string hostname)
        {
            CreateSshKeyFile();
            AddSshKey(hostname);
            return SubstituteHostname(hostname);
        }

        /// <summary>
        /// The Repository name parsed in the url, without ".git"
        /// Ex: Ex : git@github.com:NonoDS/SOUPDiscover.git -> SOUPDiscover
        /// </summary>
        protected string Repository
        {
            get; private set;
        }
    }
}
