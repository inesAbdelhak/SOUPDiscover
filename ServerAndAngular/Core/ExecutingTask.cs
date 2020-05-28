using System.Threading;
using System.Threading.Tasks;

namespace SoupDiscover.Core
{
    public class ExecutingTask
    {
        public Task Task;
        public CancellationTokenSource CancellationTokenSource;
        public IJob Job;
    }
}
