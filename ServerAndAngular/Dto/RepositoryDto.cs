using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using SoupDiscover.Dto;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SoupDiscover.Controllers.Dto
{
    public class RepositoryDto
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RepositoryType repositoryType { get; set; }

        /// <summary>
        /// The name of the repository (to display)
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string name { get; set; }
        
        /// <summary>
        /// The name of sh key to clone the repository
        /// </summary>
        [RegularExpression(@"^(git@[^:]+:[^\/]+\/[^\/]+\.git)|(https?:\/\/[^\/]+\/[^\/]+\/.*\.git)$")]
        public string url { get; set; }

        /// <summary>
        /// The name of the key to clone the repository
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string credentialId { get; set; }

        [JsonIgnore]
        public CredentialDto credential { get; set; }

        /// <summary>
        /// The name of the branch to process
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string branch { get; set; }
    }
}
