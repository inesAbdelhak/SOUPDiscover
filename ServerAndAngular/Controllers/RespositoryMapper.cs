using SoupDiscover.Controllers.Dto;
using SoupDiscover.Core.Respository;
using SoupDiscover.ORM;

namespace SoupDiscover.Controllers
{
    /// <summary>
    /// Mapping between <see cref="RepositoryDto"/> and <see cref="Repository"/>
    /// </summary>
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
                    repositoryDto.sshKey = git.SshKey.ToDto();
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
                        SshKey = repositoryDto.sshKey.ToModel(),
                    };
                    break;
            }
            return repository;
        }
    }

    public static class CredentialMapper
    {
        public static Credential ToModel(this CredentialDto dto)
        {
            if (dto == null)
            {
                return null;
            }
            return new Credential()
            {
                name = dto.name,
                key = dto.key,
            };
        }

        public static CredentialDto ToDto(this Credential model)
        {
            if(model == null)
            {
                return null;
            }
            return new CredentialDto()
            {
                name = model.name,
                key = model.key,
            };
        }
    }
}
