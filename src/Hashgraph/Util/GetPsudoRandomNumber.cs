using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph;

public partial class Client
{
    /// <summary>
    /// Generates an unbounded psudo random number, which can be retrieved via the
    /// transaction's record.
    /// </summary>
    /// <param name="configure">
    /// Optional callback method providing an opportunity to modify 
    /// the execution configuration for just this method call. 
    /// It is executed prior to submitting the request to the network.
    /// </param>
    /// <returns>
    /// A transaction receipt indicating the success of the operation.
    /// </returns>
    /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
    /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
    /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
    /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
    public async Task<TransactionReceipt> GetPsudoRandomNumberAsync(Action<IContext>? configure = null)
    {
        return new TransactionReceipt(await ExecuteTransactionAsync(new UtilPrngTransactionBody(), configure, false).ConfigureAwait(false));
    }
    /// <summary>
    /// Generates a bounded psudo random number, which can be retrieved via the
    /// transaction's record.
    /// </summary>
    /// <param name="maxValue">The maximum allowed value for
    /// the generated number.</param>
    /// <param name="configure">
    /// Optional callback method providing an opportunity to modify 
    /// the execution configuration for just this method call. 
    /// It is executed prior to submitting the request to the network.
    /// </param>
    /// <returns>
    /// A transaction receipt indicating the success of the operation.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
    /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
    /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
    /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
    /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
    public async Task<TransactionReceipt> GetPsudoRandomNumberAsync(int maxValue, Action<IContext>? configure = null)
    {
        return new TransactionReceipt(await ExecuteTransactionAsync(new UtilPrngTransactionBody(maxValue), configure, false).ConfigureAwait(false));
    }
    /// <summary>
    /// Generates an unbounded psudo random number, returning the 
    /// record containing the generated number.
    /// </summary>
    /// <param name="configure">
    /// Optional callback method providing an opportunity to modify 
    /// the execution configuration for just this method call. 
    /// It is executed prior to submitting the request to the network.
    /// </param>
    /// <returns>
    /// A transaction record containing the generated 384 bit psudo random number.
    /// </returns>
    /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
    /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
    /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
    /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
    public async Task<BytesPsudoRandomNumberRecord> GetPsudoRandomNumberWithRecordAsync(Action<IContext>? configure = null)
    {
        return new BytesPsudoRandomNumberRecord(await ExecuteTransactionAsync(new UtilPrngTransactionBody(), configure, true).ConfigureAwait(false));
    }
    /// <summary>
    /// Generates a bounded psudo random number, returning the
    /// record containing the generated number.
    /// </summary>
    /// <param name="maxValue">The maximum allowed value for
    /// the generated number.</param>
    /// <param name="configure">
    /// Optional callback method providing an opportunity to modify 
    /// the execution configuration for just this method call. 
    /// It is executed prior to submitting the request to the network.
    /// </param>
    /// <returns>
    /// A transaction record containing the generated psudo random integer number.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
    /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
    /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
    /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
    /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
    public async Task<RangedPsudoRandomNumberRecord> GetPsudoRandomNumberWithRecordAsync(int maxValue, Action<IContext>? configure = null)
    {
        return new RangedPsudoRandomNumberRecord(await ExecuteTransactionAsync(new UtilPrngTransactionBody(maxValue), configure, true).ConfigureAwait(false));
    }
}