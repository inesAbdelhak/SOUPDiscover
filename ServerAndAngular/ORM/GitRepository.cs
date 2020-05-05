namespace SoupDiscover.ORM
{
    public class GitRepository : Repository
    {
        /// <summary>
        /// The branch to check
        /// </summary>
        public string Branch { get; set; }

        /// <summary>
        /// ssh url to the repository
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The key to use, to clone repository if needed
        /// </summary>
        public virtual Credential SshKey { get; set; }

        public string SshKeyId { get; set; }

        public virtual Credential Token { get; set; }

        public string TokenId { get; set; }
    }
}
