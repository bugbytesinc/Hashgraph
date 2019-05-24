using Google.Protobuf;
using System;

namespace Hashgraph
{
    /// <summary>
    /// A <see cref="Client"/> instance’s configuration.
    /// </summary>
    /// <remarks>
    /// This interface exposes the current configuration context for a 
    /// <see cref="Client"/> instance.  When accessed thru a 
    /// <see cref="Client.Configure(Action{IContext})"/>, 
    /// <see cref="Client.Clone(Action{IContext})"/> or one of the 
    /// network request methods, calling code can interrogate the 
    /// object for configuration details and update as necessary.  
    /// Typically, the bare minimum that must be configured in a 
    /// context in order to access the network are values for 
    /// the Node <see cref="IContext.Gateway"/> and 
    /// <see cref="IContext.Payer"/> Account.  The other default 
    /// values are typically suitable for most interactions with 
    /// the network.
    /// </remarks>
    public interface IContext
    {
        /// <summary>
        /// Network Address and Node Account address for gaining 
        /// access to the Hedera Network.
        /// </summary>
        Gateway? Gateway { get; set; }
        /// <summary>
        /// The account that pays for transactions submitted to
        /// the network.
        /// </summary>
        Account? Payer { get; set; }
        /// <summary>
        /// The maximum fee the payer is willing to pay for a transaction.
        /// </summary>
        long FeeLimit { get; set; }
        /// <summary>
        /// The maximum amount of time the client is willing to wait for 
        /// consensus for a transaction, after which the submitted 
        /// transaction is invalid.
        /// </summary>
        TimeSpan TransactionDuration { get; set; }
        /// <summary>
        /// The default memo associated with transactions set to the network.
        /// </summary>
        string? Memo { get; set; }
        /// <summary>
        /// Flag indicating that network requests should generate a signed 
        /// network record for the transaction.
        /// </summary>
        bool GenerateRecord { get; set; }
        /// <summary>
        /// The number of times the client software should retry sending a 
        /// transaction to the network after the initial request failed 
        /// with the server returning BUSY or invalid due to client/server 
        /// time drift, or waiting for a “fast” receipt to return consensus.
        /// </summary>
        int RetryCount { get; set; }
        /// <summary>
        /// The timespan to wait between retry attempts.  Each retry attempt 
        /// adds the same additional amount of delay between retries.  The 
        /// second attempt will wait twice the delay, the third attempt 
        /// will wait three times the delay and so on. 
        /// </summary>
        TimeSpan RetryDelay { get; set; }
        /// <summary>
        /// Override the automatic generation of a transaction ID and use 
        /// the given Transaction ID.  Can be useful for submitting the 
        /// same transaction to multiple client/gateways.
        /// </summary>
        TxId? Transaction { get; set; }
        /// <summary>
        /// A method that is called when a transaction ID is created.  
        /// Can be useful for capturing transaction IDs when sending 
        /// duplicate transactions to multiple client/gateways or for 
        /// other tracking purposes.
        /// </summary>
        Action<TxId>? OnTransactionCreated { get; set; }
        /// <summary>
        /// Called by the library just before the serialized protobuf 
        /// is sent to the Hedera Network.  This is the only exposure 
        /// the library provides to the underlying protobuf implementation 
        /// (although they are publicly available in the library).  
        /// This method can be useful for logging and tracking purposes. 
        /// It could also be used in advanced scenarios to modify the 
        /// protobuf message just before sending.
        /// </summary>
        Action<IMessage>? OnSendingRequest { get; set; }
        /// <summary>
        /// Called by the library just after receiving a response from the 
        /// server and before processing that response.  The information 
        /// includes a retry number, the response from the first send 
        /// being 0.  Subsequent numbers indicate responses from resends 
        /// of the same transaction, typically in response to a BUSY response 
        /// from the server or in the cases where there is some time drift 
        /// between the server and the client system.  This information 
        /// can be useful for logging and troubleshooting efforts. 
        /// </summary>
        Action<int, IMessage>? OnResponseReceived { get; set; }
        /// <summary>
        /// Clears a property on the context.  This is the only way to clear 
        /// a property value on the context so that a parent value can be 
        /// used instead.  Setting a value to <code>null</code> does not 
        /// clear the value, it sets it to <code>null</code> for the 
        /// current context and child contexts. 
        /// </summary>
        /// <param name="name">
        /// The name of the property to reset, must be one of the public 
        /// properties of the <code>IContext</code>.  We suggest using 
        /// the <code>nameof()</code> operator to ensure type safety.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// when an invalid <code>name</code> is provided.
        /// </exception>
        void Reset(string name);
    }
}
