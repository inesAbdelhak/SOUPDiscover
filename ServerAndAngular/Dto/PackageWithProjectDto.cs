using SoupDiscover.ORM;

namespace SoupDiscover.Dto
{
    public class PackageWithProjectDto
    {
        public PackageWithProjectDto(Package packageDto, string[] projectNames)
        {
            this.packageDto = packageDto;
            this.projectNames = projectNames;
        }

        public Package packageDto { get; }
        public string[] projectNames { get; }
    }
}
