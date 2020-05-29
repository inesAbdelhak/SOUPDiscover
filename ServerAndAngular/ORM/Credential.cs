using System.ComponentModel.DataAnnotations;

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
        public string name { get; set; }

        /// <summary>
        /// The token
        /// </summary>
        public string key { get; set; }
    }
}
