﻿using System.ComponentModel.DataAnnotations;
using SoupDiscover.Core.Repository;

namespace SoupDiscover.ORM
{
    public class GitRepository : Repository
    {
        /// <summary>
        /// The branch to check
        /// </summary>
        [Required]
        public string Branch { get; set; }

        /// <summary>
        /// ssh url to the repository
        /// </summary>
        [Required]
        public string Url { get; set; }

        /// <summary>
        /// The key to use, to clone repository if needed
        /// </summary>
        public virtual Credential Credential { get; set; }

        public string CredentialId { get; set; }
    }
}
