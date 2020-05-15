namespace SoupDiscover.Controllers.Dto
{
    public class RepositoryDto
    {
        public RepositoryType repositoryType { get; set; }

        /// <summary>
        /// The name of the repository (to display)
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// The name of sh key to clone the repository
        /// </summary>
        public string url { get; set; }

        /// <summary>
        /// The name of sh key to clone the repository
        /// </summary>
        public string sshKeyName { get; set; }

        /// <summary>
        /// The name of the branch to process
        /// </summary>
        public string branch { get; set; }

    }
}
