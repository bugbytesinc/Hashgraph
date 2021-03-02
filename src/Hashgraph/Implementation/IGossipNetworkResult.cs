namespace Hashgraph.Implementation
{
    /// <summary>
    /// Internal interface implemented by objects that 
    /// can sign transactions.  Not intended for public use.
    /// </summary>
    internal interface IGossipNetworkResult<TResult> where TResult : class
    {
        Proto.TransactionID TransactionID { get; set; }
        TResult Result { get; set; }
    }
}
