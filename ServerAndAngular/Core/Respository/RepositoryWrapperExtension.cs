using System;

namespace SoupDiscover.Core.Respository
{
    public static class RepositoryWrapperExtension
    {
        public static RepositoryManager GetRepositoryWrapper(this Repository repository, IServiceProvider provider)
        {
            return RepositoryManager.CreateManagerFrom(repository, provider);
        }
    }

}
