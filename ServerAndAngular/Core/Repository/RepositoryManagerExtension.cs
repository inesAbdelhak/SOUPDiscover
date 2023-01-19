using System;
using SoupDiscover.Dto;

namespace SoupDiscover.Core.Repository
{
    public static class RepositoryManagerExtension
    {
        public static RepositoryManager GetRepositoryManager(this RepositoryDto repository, IServiceProvider provider)
        {
            return RepositoryManager.CreateManagerFrom(repository, provider);
        }
    }
}
