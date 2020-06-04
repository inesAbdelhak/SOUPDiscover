using log4net.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoupDiscover.Controllers.Dto;
using SoupDiscover.Dto;
using SoupDiscover.ORM;
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
                    return new GitRepositoryManager(provider.GetService<ILogger<GitRepositoryManager>>(), repository.url, repository.branch, repository.sshKeyName, repository.sshKey?.Key, $"sshgitkey{repository.sshKeyName}");
                default:
                    throw new ApplicationException($"The repository type {repository.GetType()} is not supported!");
            }
        }
    }
}