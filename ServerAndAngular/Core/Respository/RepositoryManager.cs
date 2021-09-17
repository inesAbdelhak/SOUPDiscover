using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoupDiscover.Common;
using SoupDiscover.Dto;
using System;
using System.Threading;

namespace SoupDiscover.Core.Respository
{
    public abstract class RepositoryManager
    {
        public abstract void CopyTo(string path, CancellationToken token = default);

        public static RepositoryManager CreateManagerFrom(RepositoryDto repository, IServiceProvider provider)
        {
            switch (repository.repositoryType)
            {
                case RepositoryType.Git:
                    return new GitRepositoryManager(provider.GetService<ILogger<GitRepositoryManager>>(), repository.url, repository.branch, CredentialMapper.ToModel(repository.credential));
                default:
                    throw new SoupDiscoverException($"The repository type {repository.GetType()} is not supported!");
            }
        }
    }
}