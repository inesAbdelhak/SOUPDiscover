using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoupDiscover.Common;
using SoupDiscover.Dto;

namespace SoupDiscover.Core.Repository
{
    public abstract class RepositoryManager
    {
        public abstract void CopyTo(string path, CancellationToken token = default);

        public static RepositoryManager CreateManagerFrom(RepositoryDto repository, IServiceProvider provider)
        {
            return repository.repositoryType switch
            {
                RepositoryType.Git => new GitRepositoryManager(provider.GetService<ILogger<GitRepositoryManager>>(),
                    repository.url, repository.branch, repository.credential.ToModel()),
                _ => throw new SoupDiscoverException($"The repository type {repository.GetType()} is not supported!")
            };
        }
    }
}