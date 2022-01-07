using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph;

public partial class Client
{
    /// <summary>
    /// Calls a smart contract returning a receipt indicating success.  
    /// This can be used for contract calls that do not return data. 
    /// If the contract returns data, call the <see cref="CallContractWithRecordAsync"/> 
    /// call instead to retrieve the information returned from the function call.
    /// </summary>
    /// <param name="callParameters">
    /// An object identifying the function to call, any input parameters and the 
    /// amount of gas that may be used to execute the request.
    /// </param>
    /// <param name="configure">
    /// Optional callback method providing an opportunity to modify 
    /// the execution configuration for just this method call. 
    /// It is executed prior to submitting the request to the network.
    /// </param>
    /// <returns>
    /// A contract transaction receipt indicating success, it does not
    /// include any output parameters sent from the contract.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
    /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
    /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
    /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
    /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
    public async Task<TransactionReceipt> CallContractAsync(CallContractParams callParameters, Action<IContext>? configure = null)
    {
        return new TransactionReceipt(await ExecuteTransactionAsync(new ContractCallTransactionBody(callParameters), configure, false, callParameters.Signatory).ConfigureAwait(false));
    }
    /// <summary>
    /// Calls a smart contract returning if successful.  The CallContractReceipt 
    /// will include the details of the results from the call, including the 
    /// output parameters returned by the function call.
    /// </summary>
    /// <param name="callParameters">
    /// An object identifying the function to call, any input parameters and the 
    /// amount of gas that may be used to execute the request.
    /// </param>
    /// <param name="configure">
    /// Optional callback method providing an opportunity to modify 
    /// the execution configuration for just this method call. 
    /// It is executed prior to submitting the request to the network.
    /// </param>
    /// <returns>
    /// A contract transaction record indicating success, it also
    /// any output parameters sent from the contract function.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
    /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
    /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
    /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
    /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
    public async Task<CallContractRecord> CallContractWithRecordAsync(CallContractParams callParameters, Action<IContext>? configure = null)
    {
        return new CallContractRecord(await ExecuteTransactionAsync(new ContractCallTransactionBody(callParameters), configure, true, callParameters.Signatory).ConfigureAwait(false));
    }
}