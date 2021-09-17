namespace SoupDiscover.ORM
{
    public enum CredentialType
    {
        /// <summary>
        /// Login/password, to clone the repository
        /// </summary>
        Password,
        /// <summary>
        /// An SSH key to clone the repository
        /// </summary>
        SSH,
        /// <summary>
        /// A token to clone the repository
        /// </summary>
        Token,
    }
}
