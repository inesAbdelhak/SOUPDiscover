using System.ComponentModel.DataAnnotations;

namespace SoupDiscover.Controllers.Dto
{
    public class RepositoryDto
    {
        public RepositoryType repositoryType { get; set; }

        /// <summary>
        /// The name of the repository (to display)
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string name { get; set; }
        
        /// <summary>
        /// The name of sh key to clone the repository
        /// </summary>
        [RegularExpression(@"^git@[^:]+:[^\/]+\/[^\/]+\.git$")]
        public string url { get; set; }

        /// <summary>
        /// The name of ssh key to clone the repository
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string sshKeyName { get; set; }

        /// <summary>
        /// The name of the branch to process
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string branch { get; set; }

    }
}
