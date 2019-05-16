using System;
using System.Collections.Generic;
using System.Text;

namespace Hashgraph
{
    /// <summary>
    /// Account creation parameters.
    /// </summary>
    public class CreateAccountParams
    {
        /// <summary>
        /// The public Ed25519 key corresponding to the private key authorized 
        /// to sign transactions on behalf of this new account.  The key 
        /// length is expected to be 44 bytes long and start with the prefix 
        /// of 0x302a300506032b6570032100.
        /// </summary>
        public ReadOnlyMemory<byte> PublicKey { get; set; }
        /// <summary>
        /// The initial balance that will be transferred from the 
        /// <see cref="IContext.Payer"/> account to the new account 
        /// upon creation.
        /// </summary>
        public ulong InitialBalance { get; set; }
        /// <summary>
        /// Threshold in tinybars at which withdraws larger than
        /// this value will automatically trigger the creation of a record 
        /// for the transaction. This new account will be charged for the 
        /// record creation.
        /// </summary>
        public ulong SendThresholdCreateRecord { get; set; }
        /// <summary>
        /// Threshold in tinybars at which deposits larger than
        /// this value will automatically trigger the creation of a 
        /// record for the transaction.  This new account will be charged
        /// for the record creation.
        /// </summary>
        public ulong ReceiveThresholdCreateRecord { get; set; }
        /// <summary>
        /// When creating a new account: the newly created account must 
        /// sign any transaction transferring crypto into the newly 
        /// created account.
        /// </summary>
        public bool RequireReceiveSignature { get; set; }
        /// <summary>
        /// The auto-renew period for the newly created account, it will continue 
        /// to be renewed at the given interval for as long as the account contains 
        /// hbars sufficient to cover the renewal charge.
        /// </summary>
        public TimeSpan AutoRenewPeriod { get; set; }        
        /// <summary>
        /// Public Constructor, sets system defaults for various properties.
        /// </summary>
        public CreateAccountParams()
        {
            AutoRenewPeriod = TimeSpan.FromDays(31);
            SendThresholdCreateRecord = ulong.MaxValue;
            ReceiveThresholdCreateRecord = ulong.MaxValue;
            RequireReceiveSignature = false;
        }
    }
}
