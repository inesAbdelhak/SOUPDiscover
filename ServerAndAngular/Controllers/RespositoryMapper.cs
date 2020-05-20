using SoupDiscover.Controllers.Dto;
using SoupDiscover.Core.Respository;
using SoupDiscover.ORM;

namespace SoupDiscover.Controllers
{
    public static class RespositoryMapper
    {
        public static RepositoryDto ToDto(this Repository repositoryModel)
        {
            if(repositoryModel == null)
            {
                return null;
            }
            var repositoryDto = new RepositoryDto();
            repositoryDto.name = repositoryModel.Name;
            switch (repositoryModel)
            {
                case GitRepository git:
                    repositoryDto.repositoryType = RepositoryType.Git;
                    repositoryDto.branch = git.Branch;
                    repositoryDto.sshKeyName = git.SshKeyId;
                    repositoryDto.url = git.Url;
                    break;
                default:
                    return null;
            }
            return repositoryDto;
        }

        public static Repository ToModel(this RepositoryDto repositoryDto)
        {
            if(repositoryDto == null)
            {
                return null;
            }
            Repository repository = null;
            // Create repository from repository dto
            switch (repositoryDto.repositoryType)
            {
                case RepositoryType.Git:
                    repository = new GitRepository()
                    {
                        Branch = repositoryDto.branch,
                        Url = repositoryDto.url,
                        Name = repositoryDto.name,
                        SshKeyId = repositoryDto.sshKeyName,
                    };
                    break;
            }
            return repository;
        }
    }
}
