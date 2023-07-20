using Hashgraph.Implementation;
using System;
using System.Linq;
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
    [Fact(DisplayName = "Transaction ID: Ticks Creator Does not Collide Multi Threaded In Linq")]
    public async Task TicsCreatorDoesNotCollideMultiThreadInLinq()
    {
        var tasks = Enumerable.Range(1, 30000).Select(_ => Task.Run(() => Epoch.UniqueClockNanos()));
        var nano = await Task.WhenAll(tasks);
        for(int i = 0; i < nano.Length; i ++)
        {
            for(int j = i+1; j < nano.Length; j++)
            {
                Assert.NotEqual(nano[i], nano[j]);
            }
        }
    }
    [Fact(DisplayName = "Transaction ID: Client Creator Does not Collide Multi Threaded In Linq")]
    public async Task ClientCreatorDoesNotCollideMultiThreadInLinq()
    {
        await using Client client = new(cfg => {
            cfg.Payer = new Address(0, 0, 3);
        });
        var tasks = Enumerable.Range(1, 20000).Select(_ => Task.Run(() => client.CreateNewTxId()));
        var txids = await Task.WhenAll(tasks);
        for (int i = 0; i < txids.Length; i++)
        {
            for (int j = i + 1; j < txids.Length; j++)
            {
                Assert.NotEqual(txids[i], txids[j]);
            }
        }
    }
}