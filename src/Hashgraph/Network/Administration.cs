using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph;

public partial class Client
{
    /// <summary>
    /// Suspends the network at the specified consensus time.  
    /// This does not result in any network changes or upgrades 
    /// and requires manual intervention to restart the network.
    /// </summary>
    /// <param name="consensusTime">
    /// The time of consensus that nodes will stop services, this
    /// date must be in the future relative to the submission of
    /// this transaciton.
    /// </param>
    /// <param name="configure">
    /// Optional callback method providing an opportunity to modify 
    /// the execution configuration for just this method call. 
    /// It is executed prior to submitting the request to the network.
    /// </param>
    /// <returns>
    /// A Transaction Receipt indicating success.
    /// </returns>
    /// <remarks>
    /// This operation must be submitted by a privileged account
    /// having access rights to perform this operation.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
    /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
    /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
    /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
    /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
    public async Task<TransactionReceipt> SuspendNetworkAsync(ConsensusTimeStamp consensusTime, Action<IContext>? configure = null)
    {
        return new TransactionReceipt(await ExecuteTransactionAsync(new FreezeTransactionBody(consensusTime, FreezeType.FreezeOnly), configure, false).ConfigureAwait(false));
    }
    /// <summary>
    /// Prepares the network for an upgrade as configured by 
    /// the specified upgrade file.  The file hash must match 
    /// the hash of the identified upgrade file stored at the 
    /// specified location.  This operation does not 
    /// immediately affect network operations.
    /// </summary>
    /// <param name="fileAddress">
    /// Address of the upgrade file (previously uploaded).
    /// </param>
    /// <param name="fileHash">
    /// Hash of the file upgrade file's contents.
    /// </param>
    /// <param name="configure">
    /// Optional callback method providing an opportunity to modify 
    /// the execution configuration for just this method call. 
    /// It is executed prior to submitting the request to the network.
    /// </param>
    /// <returns>
    /// A Transaction Receipt indicating success.
    /// </returns>
    /// <remarks>
    /// This operation must be submitted by a privileged account
    /// having access rights to perform this operation.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
    /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
    /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
    /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
    /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
    public async Task<TransactionReceipt> PrepareNetworkUpgrade(Address fileAddress, ReadOnlyMemory<byte> fileHash, Action<IContext>? configure = null)
    {
        return new TransactionReceipt(await ExecuteTransactionAsync(new FreezeTransactionBody(fileAddress, fileHash), configure, false).ConfigureAwait(false));
    }
    /// <summary>
    /// Executes a previously "prepared" upgrade file at the
    /// specified consensus time across the entire network.
    /// This act will suspend network services for the duration
    /// of the upgrade.
    /// </summary>
    /// <param name="consensusTime">
    /// The time of consensus that nodes will stop services, this
    /// date must be in the future relative to the submission of
    /// this transaction.
    /// </param>
    /// <param name="configure">
    /// Optional callback method providing an opportunity to modify 
    /// the execution configuration for just this method call. 
    /// It is executed prior to submitting the request to the network.
    /// </param>
    /// <returns>
    /// A Transaction Receipt indicating success.
    /// </returns>
    /// <remarks>
    /// This operation must be submitted by a privileged account
    /// having access rights to perform this operation.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
    /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
    /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
    /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
    /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
    public async Task<TransactionReceipt> ScheduleNetworkUpgradeAsync(ConsensusTimeStamp consensusTime, Action<IContext>? configure = null)
    {
        return new TransactionReceipt(await ExecuteTransactionAsync(new FreezeTransactionBody(consensusTime, FreezeType.FreezeUpgrade), configure, false).ConfigureAwait(false));
    }
    /// <summary>
    /// Schedules an immediate upgrade of auxiliary services and 
    /// containers providing telemetry and metrics.  Does not 
    /// impact ongoing network operations.
    /// </summary>
    /// <param name="configure">
    /// Optional callback method providing an opportunity to modify 
    /// the execution configuration for just this method call. 
    /// It is executed prior to submitting the request to the network.
    /// </param>
    /// <returns>
    /// A Transaction Receipt indicating success.
    /// </returns>
    /// <remarks>
    /// This operation must be submitted by a privileged account
    /// having access rights to perform this operation.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
    /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
    /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
    /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
    /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
    public async Task<TransactionReceipt> ScheduleTelemetryUpgradeAsync(Action<IContext>? configure = null)
    {
        return new TransactionReceipt(await ExecuteTransactionAsync(new FreezeTransactionBody(FreezeType.TelemetryUpgrade), configure, false).ConfigureAwait(false));
    }
    /// <summary>
    /// Aborts any scheduled network upgrade operation.
    /// </summary>
    /// <param name="configure">
    /// Optional callback method providing an opportunity to modify 
    /// the execution configuration for just this method call. 
    /// It is executed prior to submitting the request to the network.
    /// </param>
    /// <returns>
    /// A Transaction Receipt indicating success.
    /// </returns>
    /// <remarks>
    /// This operation must be submitted by a privileged account
    /// having access rights to perform this operation.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
    /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
    /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
    /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
    /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
    public async Task<TransactionReceipt> AbortNetworkUpgrade(Action<IContext>? configure = null)
    {
        return new TransactionReceipt(await ExecuteTransactionAsync(new FreezeTransactionBody(FreezeType.FreezeAbort), configure, false).ConfigureAwait(false));
    }
}