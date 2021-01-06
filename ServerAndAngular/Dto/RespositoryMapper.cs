using SoupDiscover.Core.Respository;
using SoupDiscover.ORM;

namespace SoupDiscover.Dto
{
    /// <summary>
    /// Mapping between <see cref="RepositoryDto"/> and <see cref="Repository"/>
    /// </summary>
    public static class RespositoryMapper
    {
        public static RepositoryDto ToDto(this Repository repositoryModel)
        {
            if (repositoryModel == null)
            {
                return null;
            }
            var repositoryDto = new RepositoryDto
            {
                name = repositoryModel.Name
            };
            switch (repositoryModel)
            {
                case GitRepository git:
                    repositoryDto.repositoryType = RepositoryType.Git;
                    repositoryDto.branch = git.Branch;
                    repositoryDto.credentialId = git.CredentialId;
                    repositoryDto.credential = git.Credential.ToDto();
                    repositoryDto.url = git.Url;
                    break;
                default:
                    return null;
            }
            return repositoryDto;
        }

        public static Repository ToModel(this RepositoryDto repositoryDto)
        {
            if (repositoryDto == null)
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
                        CredentialId = repositoryDto.credentialId,
                        Credential = repositoryDto.credential.ToModel(),
                    };
                    break;
            }
            return repository;
        }
    }
}
