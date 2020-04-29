namespace testAngulardotnet.ORM
{
    public class GitRepository : Repository
    {
        /// <summary>
        /// The branch to check
        /// </summary>
        public string Branch { get; set; }

        /// <summary>
        /// The key to use, to clone repository if needed
        /// </summary>
        public Sshkey SshKey { get; set; }

        public AuthentificationToken Token { get; set; }
    }
}
