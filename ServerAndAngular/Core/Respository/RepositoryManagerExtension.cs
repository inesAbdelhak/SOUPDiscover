using SoupDiscover.Controllers.Dto;
using System;

namespace SoupDiscover.Core.Respository
{
    public static class RepositoryManagerExtension
    {
        public static RepositoryManager GetRepositoryManager(this RepositoryDto repository, IServiceProvider provider)
        {
            return RepositoryManager.CreateManagerFrom(repository, provider);
        }
    }

}
