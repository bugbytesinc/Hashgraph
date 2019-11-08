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
        /// The address of the account that pays for transactions 
        /// submitted to the network, otherwise known as the 
        /// "Operator".
        /// </summary>
        Address? Payer { get; set; }
        /// <summary>
        /// The private key, keys or signing callback method that 
        /// are needed to sign the transaction.  At a minimum, this
        /// typically includes the Payer's signing key, but can also
        /// include other signatories such as those required when 
        /// creating claims, files and contracts.
        /// </summary>
        /// <remarks>
        /// Every key added into this context will sign every 
        /// transaction made by the owning client object.  This 
        /// includes the "second" transaction required to obtain
        /// a record from the network when the "WithRecord" form
        /// of method calls are invoked.  It is important to note
        /// that many of the API calls have additional opportunities
        /// to identify specific signatories for various transactions.
        /// It is not necessary to add all the keys on this context 
        /// for say, a key rotation on an account; especially if the
        /// "WithRecord" form is invoked.  It is recommended to place
        /// only the keys associated with the Payer account in this
        /// property.  Not doing so may result in extraneous unecessary
        /// signatures being sent with transactions.  If this happens
        /// the transaction cost goes up due to the larger transaction
        /// size, waisting crypto due to unecessary signature processing
        /// fees.
        /// </remarks>
        Signatory? Signatory { get; set; }
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
        /// If set to <code>true</code> the library client will automatically
        /// scan for <see cref="ResponseCode.InvalidTransactionStart"/> responses
        /// from the server and adjust the automatic generation of transaction
        /// IDs as appropriate to compensate for local clock drift from the
        /// network server's clock.  This can help reduce the wait time for
        /// acceptance of transactions if the local clock deviates from the
        /// network time by too wide a margin.  It is only recommended to 
        /// enable this setting if you are having performance problems related
        /// to clock drift an are unable to resolve at the environment level.
        /// The default for this value is <code>false</code>.
        /// </summary>
        bool AdjustForLocalClockDrift { get; set; }
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
