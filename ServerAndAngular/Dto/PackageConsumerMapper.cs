using SoupDiscover.ORM;

namespace SoupDiscover.Dto
{
    /// <summary>
    /// Mapping between <see cref="PackageConsumer" and <see cref="c"/>.
    /// </summary>
    public static class PackageConsumerMapper
    {
        /// <summary>
        /// Transform <see cref="PackageConsumer"/> to <see cref="PackageConsumerDto"/>  
        /// </summary>
        public static PackageConsumerDto ToDto(this PackageConsumer packageConsumer)
        {
            if (packageConsumer == null)
            {
                return null;
            }
            return new PackageConsumerDto()
            {
                name = packageConsumer.Name,
                projectId = packageConsumer.ProjectId,
            };
        }
    }
}
