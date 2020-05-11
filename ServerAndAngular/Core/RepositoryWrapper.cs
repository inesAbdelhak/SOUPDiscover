using SoupDiscover.ORM;
using System;

namespace SoupDiscover.Core
{
    public abstract class RepositoryWrapper
    {
        public abstract void CopyTo(string path);
        
        public static RepositoryWrapper CreateWrapperFrom(Repository repository)
        {
            switch(repository)
            {
                case GitRepository git:
                    return new GitRepositoryWrapper(git.Url, git.Branch, git.SshKeyId, git.SshKey?.key, $"sshgitkey{git.SshKeyId}");
                default:
                    throw new ApplicationException($"The repository type {repository.GetType()} is not supported");
            }
        }
    }
}