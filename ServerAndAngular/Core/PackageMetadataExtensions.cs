using SoupDiscover.ORM;

namespace SoupDiscover.Core
{
    public static class PackageMetadataExtensions
    {
        /// <summary>
        /// Converts the type of the license.
        /// </summary>
        /// <param name="licenseType">Type of the license.</param>
        public static LicenseType ConvertLicenseType(this NuGet.Packaging.LicenseType? licenseType)
        {
            return licenseType switch
            {
                NuGet.Packaging.LicenseType.File => LicenseType.File,
                NuGet.Packaging.LicenseType.Expression => LicenseType.Expression,
                _ => LicenseType.None
            };
        }
    }
}
