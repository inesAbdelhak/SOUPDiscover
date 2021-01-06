using SoupDiscover.ORM;

namespace SoupDiscover.Dto
{
    public class PackageWithProjectDto
    {
        public PackageWithProjectDto(Package packageDto, PackageConsumerDto[] packageConsumers)
        {
            this.packageDto = packageDto;
            this.packageConsumers = packageConsumers;
        }

        public Package packageDto { get; }

        public PackageConsumerDto[] packageConsumers { get; }
    }
}
