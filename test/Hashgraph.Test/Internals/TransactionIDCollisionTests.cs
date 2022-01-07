using Hashgraph.Implementation;
using System.Threading.Tasks;
using Xunit;

namespace Hashgraph.Test.Internals;

public class TransactionIDCollisionTests
{
    [Fact(DisplayName = "Transaction ID: Ticks Creator Does not Collide Single Threaded")]
    public void TicsCreatorDoesNotCollide()
    {
        for (int i = 0; i < 100; i++)
        {
            var tic1 = Epoch.UniqueClockNanos();
            var tic2 = Epoch.UniqueClockNanos();
            Assert.NotEqual(tic1, tic2);
        }
    }
    [Fact(DisplayName = "Transaction ID: Ticks Creator Does not Collide Multi Threaded")]
    public async Task TicsCreatorDoesNotCollideMultiThread()
    {
        var tasks = new Task[20];
        for (int j = 0; j < tasks.Length; j++)
        {
            tasks[j] = Task.Run(() =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    var tic1 = Epoch.UniqueClockNanos();
                    var tic2 = Epoch.UniqueClockNanos();
                    Assert.NotEqual(tic1, tic2);
                }
            });
        }
        await Task.WhenAll(tasks);
    }
}