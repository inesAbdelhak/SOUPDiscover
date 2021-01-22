namespace SoupDiscover.ORM
{
    public enum LicenseType
    {
        /// <summary>
        /// No one license find
        /// </summary>
        None,
        /// <summary>
        /// An expression of type of license.
        /// View https://spdx.org/licenses/ for all license expression.
        /// </summary>
        Expression,

        /// <summary>
        /// A file in the package
        /// </summary>
        File,

        /// <summary>
        /// An Url to the license details
        /// </summary>
        Url,
    }
}
