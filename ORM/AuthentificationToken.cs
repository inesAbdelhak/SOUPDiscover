using System.ComponentModel.DataAnnotations;

namespace testAngulardotnet.ORM
{
    /// <summary>
    /// A token used to anthenticate from an api
    /// </summary>
    public class AuthentificationToken
    {
        /// <summary>
        /// The name given to the token
        /// </summary>
        [Key]
        public string NameID { get; set; }

        /// <summary>
        /// The token
        /// </summary>
        public string Token { get; set; }
    }
}
