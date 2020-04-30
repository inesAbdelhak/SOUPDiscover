namespace SoupDiscover.ORM
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
        public Credential SshKey { get; set; }

        public Credential Token { get; set; }
    }
}
