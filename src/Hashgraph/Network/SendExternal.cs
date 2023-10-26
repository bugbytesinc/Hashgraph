using Google.Protobuf;
using Hashgraph.Implementation;
using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph;

public partial class Client
{
    /// <summary>
    /// Sends an arbitrary externally created Hedera Transaction to the network, 
    /// but does not wait for a receipt, only returning the <code>PRECHECK</code>
    /// code returned from the network (as <code>ResponseCode</code>).
    /// The transaction is submitted as a <code>SignedTransaction</code> object, 
    /// protobuf encoded, and may include signatures in the associated 
    /// <code>sigMap</code> field.  Any Signatories held in the client context 
    /// (or method call) will add signatures to this transaction prior to submitting.  
    /// It is not necessary to include a <code>Payer</code> in the context as the 
    /// transaction itself defines the payer, however a compatible 
    /// <code>Gateway</code> must be contained in context as it provides the necessary 
    /// routing to the Hedera Network’s node, which is not encoded in the signed 
    /// transaction structure.
    /// </summary>
    /// <remarks>
    /// Note: this method accepts protobuf encoded as a <code>SignedTransaction</code>,
    /// not a <code>Transaction</code> object as the transaction object contains 
    /// depricated protobuf fields not supported by this SDK.  The method will perform
    /// the necessary final wrapping of the transaction for final submission.
    /// </remarks>
    /// <param name="signedTransactionBytes">
    /// The serialized protobuf encoded bytes of a <code>SignedTransaction</code>
    /// object to be submitted to a Hedera Gossip Network Node. These bytes must be 
    /// manually created from calling code having a knowledge of how to construct a
    /// proper Hedera transaction.
    /// </param>
    /// <param name="configure">
    /// Optional callback method providing an opportunity to modify 
    /// the execution configuration for just this method call. 
    /// It is executed prior to submitting the request to the network.
    /// </param>
    /// <returns>
    /// The <code>ResponseCode</code> corresponding to the precheck value
    /// returned from the remote Hedera Gossip Node which can indicate
    /// success or failure.  This method does not wait for consensus and does 
    /// not return a receipt.  However, it will retry sending the transaction
    /// when it receives a <code>BUSY</code> response from the remote node.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
    /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
    /// <exception cref="PrecheckException">If transaction submission failed due to network communication errors.  
    /// The PrecheckException will not be thrown for PRECHECK error codes returned from the remote gRPC endpoint.  
    /// This behavior is different from most other API calls that throw a PrecheckException for any precheck value
    /// returned that is not <code>OK</code>.  The PrecheckException is thrown because there is no true response
    /// code to return and the method should divulge some information as to the nature of the network error.</exception>
    public Task<ResponseCode> SendExternalTransactionAsync(ReadOnlyMemory<byte> signedTransactionBytes, Action<IContext>? configure = null)
    {
        return SendExternalTransactionImplementationAsync(signedTransactionBytes, null, configure);
    }
    /// <summary>
    /// Sends an arbitrary externally created Hedera Transaction to the network, 
    /// but does not wait for a receipt, only returning the <code>PRECHECK</code>
    /// code returned from the network (as <code>ResponseCode</code>).
    /// The transaction is submitted as a <code>SignedTransaction</code> object, 
    /// protobuf encoded, and may include signatures in the associated 
    /// <code>sigMap</code> field.  Any Signatories held in the client context 
    /// (or method call) will add signatures to this transaction prior to submitting.  
    /// It is not necessary to include a <code>Payer</code> in the context as the 
    /// transaction itself defines the payer, however a compatible 
    /// <code>Gateway</code> must be contained in context as it provides the necessary 
    /// routing to the Hedera Network’s node, which is not encoded in the signed 
    /// transaction structure.
    /// </summary>
    /// <remarks>
    /// Note: this method accepts protobuf encoded as a <code>SignedTransaction</code>,
    /// not a <code>Transaction</code> object as the transaction object contains 
    /// depricated protobuf fields not supported by this SDK.  The method will perform
    /// the necessary final wrapping of the transaction for final submission.
    /// </remarks>
    /// <param name="signedTransactionBytes">
    /// The serialized protobuf encoded bytes of a <code>SignedTransaction</code>
    /// object to be submitted to a Gossip Network Transaction. These bytes must be 
    /// manually created from calling code having a knowledge of how to construct the 
    /// Hedera transaction.
    /// </param>
    /// <param name="signatory">
    /// The signatory containing any additional private keys or callbacks
    /// that should add signatures to the externally created transaction
    /// prior to submitting to the hedera network.
    /// </param>
    /// <param name="configure">
    /// Optional callback method providing an opportunity to modify 
    /// the execution configuration for just this method call. 
    /// It is executed prior to submitting the request to the network.
    /// </param>
    /// <returns>
    /// The <code>ResponseCode</code> corresponding to the precheck value
    /// returned from the remote Hedera Gossip Node which can indicate
    /// success or failure.  This method does not wait for consensus and does 
    /// not return a receipt.  However, it will retry sending the transaction
    /// when it receives a <code>BUSY</code> response from the remote node.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
    /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
    /// <exception cref="PrecheckException">If transaction submission failed due to network communication errors.  
    /// The PrecheckException will not be thrown for PRECHECK error codes returned from the remote gRPC endpoint.  
    /// This behavior is different from most other API calls that throw a PrecheckException for any precheck value
    /// returned that is not <code>OK</code>.  The PrecheckException is thrown because there is no true response
    /// code to return and the method should divulge some information as to the nature of the network error.</exception>
    public Task<ResponseCode> SendExternalTransactionAsync(ReadOnlyMemory<byte> signedTransactionBytes, Signatory signatory, Action<IContext>? configure = null)
    {
        return SendExternalTransactionImplementationAsync(signedTransactionBytes, signatory, configure);
    }
    /// <summary>
    /// Internal implementation of the send message call.
    /// </summary>
    private async Task<ResponseCode> SendExternalTransactionImplementationAsync(ReadOnlyMemory<byte> signedTransactionBytes, Signatory? extraSignatory, Action<IContext>? configure)
    {
        try
        {
            if (signedTransactionBytes.IsEmpty)
            {
                throw new ArgumentOutOfRangeException(nameof(signedTransactionBytes), "Missing Signed Transaction Bytes (was empty).");
            }
            var signedTransaction = SignedTransaction.Parser.ParseFrom(signedTransactionBytes.Span);
            if (signedTransaction.BodyBytes.IsEmpty)
            {
                throw new ArgumentOutOfRangeException(nameof(signedTransactionBytes), "The Signed transaction did not contain a transaction.");
            }
            var transactionBody = TransactionBody.Parser.ParseFrom(signedTransaction.BodyBytes);
            if (!transactionBody.TryGetNetworkTransaction(out INetworkTransaction networkTransaction))
            {
                throw new ArgumentOutOfRangeException(nameof(signedTransactionBytes), "Unrecognized Transaction Type, unable to determine which Hedera Network Service Type should process transaction.");
            }
            await using var context = CreateChildContext(configure);
            var gateway = context.Gateway;
            if (gateway is null)
            {
                throw new InvalidOperationException("The Network Gateway Node has not been configured. Please check that 'Gateway' is set in the Client context and is compatible with this external transaction.");
            }
            var nodeAddress = transactionBody.NodeAccountID.AsAddress();
            var gatewayAddress = (Address)gateway;
            if (nodeAddress != gatewayAddress)
            {
                throw new ArgumentException("The configured Gateway is not compatible with the Node Account ID of this transaction.", nameof(signedTransactionBytes));
            }
            var result = new NetworkResult();
            result.TransactionID = transactionBody.TransactionID;
            // Only go to the effort of adding signatures if the signatories
            // exist, if they are null we're just a pass-thru in this context.
            if (context.Signatory is not null || extraSignatory is not null)
            {
                var signatory = context.GatherSignatories(extraSignatory);
                if (signatory.GetSchedule() is not null)
                {
                    throw new ArgumentException("Scheduling the submission of an external transaction is not supported (one or more signatories in the context were created as pending signatories).  However, the external transaction itself can be a scheduled transaction.", nameof(signedTransactionBytes));
                }
                // Some of the complexity below is necessary to prevent accidental
                // truncation of signature prefixes and/or duplicate signatures.
                var signaturePrefixTrimLimit = signedTransaction.SigMap is null ?
                    context.SignaturePrefixTrimLimit :
                    Math.Max(context.SignaturePrefixTrimLimit, signedTransaction.SigMap.MaxSignaturePrefixLength);
                var invoice = new Invoice(signedTransaction.BodyBytes.Memory, signaturePrefixTrimLimit);
                signedTransaction.SigMap?.AddSignaturesToInvoice(invoice);
                await signatory.SignAsync(invoice).ConfigureAwait(false);
                signedTransaction.SigMap = invoice.GenerateSignedTransactionFromSignatures().SigMap;
            }
            var transaction = new Transaction
            {
                SignedTransactionBytes = signedTransaction.ToByteString()
            };
            var precheck = await context.ExecuteSignedRequestWithRetryImplementationAsync(transaction, networkTransaction.InstantiateNetworkRequestMethod, getResponseCode).ConfigureAwait(false);
            return (ResponseCode)precheck.NodeTransactionPrecheckCode;

            static ResponseCodeEnum getResponseCode(TransactionResponse response)
            {
                return response.NodeTransactionPrecheckCode;
            }
        }
        catch (InvalidProtocolBufferException ipbe)
        {
            throw new ArgumentException("Signed Transaction Bytes not recognized as valid Protobuf.", nameof(signedTransactionBytes), ipbe);
        }
    }
}
