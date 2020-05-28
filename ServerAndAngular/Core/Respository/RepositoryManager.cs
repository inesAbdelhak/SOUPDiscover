﻿using log4net.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoupDiscover.ORM;
using System;

namespace SoupDiscover.Core.Respository
{
    public abstract class RepositoryManager
    {
        public abstract void CopyTo(string path);

        public static RepositoryManager CreateManagerFrom(Repository repository, IServiceProvider provider)
        {
            switch (repository)
            {
                case GitRepository git:
                    return new GitRepositoryManager(provider.GetService<ILogger<GitRepositoryManager>>(), git.Url, git.Branch, git.SshKeyId, git.SshKey?.key, $"sshgitkey{git.SshKeyId}");
                default:
                    throw new ApplicationException($"The repository type {repository.GetType()} is not supported!");
            }
        }
    }
}