using SoupDiscover.ORM;
using System;

namespace SoupDiscover.Core
{
    public static class RepositoryWrapperExtension
    {
        public static RepositoryWrapper GetRepositoryWrapper(this Repository repository, IServiceProvider provider)
        {
            return RepositoryWrapper.CreateWrapperFrom(repository, provider);
        }
    }

}
