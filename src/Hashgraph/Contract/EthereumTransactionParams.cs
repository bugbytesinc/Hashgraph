#pragma warning disable CS8618 // Non-nullable field is uninitialized.

using System;

namespace Hashgraph;

/// <summary>
/// Represents a transaction submitted to the hedera network thru the
/// native Ethereum gateway feature.
/// </summary>
public class EthereumTransactionParams
{
    /// <summary>
    /// The complete raw Ethereum transaction (RLP encoded type 0, 1, and 2),
    /// with the exception of the call data if <code>ExtraCallData</code>
    /// has been populated.
    /// </summary>
    /// <remarks>
    /// If it necessary to invoke the <code>ExtraCallData</code> feature,
    /// the callData for the ethereum transaction should be set to an empty
    /// string in this property. However Note: for validation of signatures,
    /// a node will reconstruct the proper ethereumData payload with the
    /// call data before attempting to validate signatures, so there may
    /// be extra work in generating the complete ethereum transaction to
    /// sign with private keys before breaking apart into components small
    /// enough to load onto a Hedera Gossip Node thru the HAPI.
    /// </remarks>
    public ReadOnlyMemory<byte> Transaction { get; set; }
    /// <summary>
    /// For large transactions where the call data cannot fit within the size
    /// of an hedera transaction, this address points to a file containing the 
    /// callData of the ethereumData. The hedera node will re-write the 
    /// ethereumData inserting the contents into the existing empty callData 
    /// element with the contents in the referenced file at time of execution. 
    /// The reconstructed etherumData will then be checked against signatures
    /// for validation.
    /// </summary>
    public Address ExtraCallData { get; set; }
    /// <summary>
    /// The maximum amount of gas, in tinybars, that the payer of the hedera 
    /// ethereum transaction is willing to pay to execute the transaction.
    /// </summary>
    /// <remarks>
    /// Ordinarily the account with the ECDSA alias corresponding to the public 
    /// key that is extracted from the ethereum_data signature is responsible for 
    /// fees that result from the execution of the transaction.  If that amount of 
    /// authorized fees is not sufficient then the (hapi) payer of the transaction 
    /// can be charged, up to but not exceeding this amount.  If the ethereum_data 
    /// transaction authorized an amount that was insufficient then the (hapi) payer 
    /// will only be charged the amount needed to make up the difference. If the gas 
    /// price in the ethereum transaction was set to zero then the (hapi) payer will 
    /// be assessed the entire gas & hedera fees.
    /// </remarks>
    public long AdditionalGasAllowance { get; set; }
    /// <summary>
    /// Additional private key, keys or signing callback method 
    /// required to invoke this transaction.  Typically not used,
    /// however there are some edge cases where it may send
    /// crypto to accounts that require a signature to receive
    /// funds.
    /// </summary>
    /// <remarks>
    /// Keys/callbacks added here will be combined with those already
    /// identified in the client object's context when signing this 
    /// transaction to change the state of this account.  They will 
    /// not be asked to sign transactions to retrieve the record
    /// if the "WithRecord" form of the method call is made.  The
    /// client will rely on the Signatory from the context to sign
    /// the transaction requesting the record.
    /// </remarks>
    public Signatory? Signatory { get; set; }
}