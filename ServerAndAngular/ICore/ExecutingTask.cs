using SoupDiscover.ICore;
using System.Threading;
using System.Threading.Tasks;

namespace SoupDiscover.ICore
{
    public class ExecutingTask
    {
        public Task Task;
        public CancellationTokenSource CancellationTokenSource;
        public IJob Job;
    }
}
