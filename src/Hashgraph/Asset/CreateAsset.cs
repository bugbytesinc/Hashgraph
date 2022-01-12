using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph;

public partial class Client
{
    /// <summary>
    /// Creates a new asset (non fungible token) with the given create parameters.
    /// </summary>
    /// <param name="createParameters">
    /// Details regarding the asset to instantiate.
    /// </param>
    /// <param name="configure">
    /// Optional callback method providing an opportunity to modify 
    /// the execution configuration for just this method call. 
    /// It is executed prior to submitting the request to the network.
    /// </param>
    /// <returns>
    /// A transaction receipt with a description of the newly created Asset.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
    /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
    /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
    /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
    /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
    public async Task<CreateTokenReceipt> CreateTokenAsync(CreateAssetParams createParameters, Action<IContext>? configure = null)
    {
        return new CreateTokenReceipt(await ExecuteTransactionAsync(new TokenCreateTransactionBody(createParameters), configure, false, createParameters.Signatory).ConfigureAwait(false));
    }
    /// <summary>
    /// Creates a new Asset instance with the given create parameters.
    /// </summary>
    /// <param name="createParameters">
    /// Details regarding the Asset to instantiate.
    /// </param>
    /// <param name="configure">
    /// Optional callback method providing an opportunity to modify 
    /// the execution configuration for just this method call. 
    /// It is executed prior to submitting the request to the network.
    /// </param>
    /// <returns>
    /// A transaction record with a description of the newly created Asset.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
    /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
    /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
    /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
    /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
    public async Task<CreateTokenRecord> CreateTokenWithRecordAsync(CreateAssetParams createParameters, Action<IContext>? configure = null)
    {
        return new CreateTokenRecord(await ExecuteTransactionAsync(new TokenCreateTransactionBody(createParameters), configure, true, createParameters.Signatory).ConfigureAwait(false));
    }
}