using System.Threading.Tasks;

namespace Hashgraph.Implementation
{
    /// <summary>
    /// Internal interface implemented by objects that 
    /// can sign transactions.  Not intended for public use.
    /// </summary>
    internal interface ISignatory
    {
        Task SignAsync(IInvoice invoice);
        PendingParams? GetSchedule();
    }
}
