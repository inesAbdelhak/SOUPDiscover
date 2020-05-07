using SoupDiscover.ORM;

namespace SoupDiscover.Core
{
    public static class RepositoryWrapperExtension
    {
        public static RepositoryWrapper GetRepositoryWrapper(this Repository repository)
        {
            return RepositoryWrapper.CreateWrapperFrom(repository);
        }
    }

}
