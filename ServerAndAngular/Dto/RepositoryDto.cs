using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SoupDiscover.Dto
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
        [RegularExpression(@"^(https?:\/\/(www\.)?|git@)[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}([-a-zA-Z0-9()!@:%_\+.~#?&\/\/=]*)$")]
        public string url { get; set; }

        public string GetUrlScheme()
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out var transformedUrl))
            {
                return transformedUrl.Scheme;
            }
            return null;
        }

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
