namespace SoupDiscover.Dto
{
    public class CredentialDto
    {
        /// <summary>
        /// The name given to the credential
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The token
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The login to use, to clone the repository
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// The password to use to clone the repository
        /// </summary>
        public string Password { get; set; }
    }
}
