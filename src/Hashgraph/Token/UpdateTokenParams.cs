#pragma warning disable CS8618 // Non-nullable field is uninitialized.
using System;

namespace Hashgraph
{
    /// <summary>
    /// Represents the properties on a token definition that can be changed.
    /// Any property set to <code>null</code> on this object when submitted to the 
    /// <see cref="Client.UpdateTokenAsync(UpdateTokenParams, Action{IContext})"/>
    /// method will be left unchanged by the system.  The transaction must be
    /// appropriately signed as described by the original
    /// <see cref="CreateTokenParams.Administrator"/> endorsement in order
    /// to make changes.  If there is no administrator endorsement specified,
    /// the token is imutable and cannot be changed.
    /// </summary>
    public sealed class UpdateTokenParams
    {
        /// <summary>
        /// The identifier (Address/Symbol) of the token to update.
        /// </summary>
        public Address Token { get; set; }
        /// <summary>
        /// The treasury account holding the reserve balance of tokens.
        /// </summary>
        public Address? Treasury { get; set; }
        /// <summary>
        /// Replace this Tokens's current administrative key signing rquirements 
        /// with new signing requirements.  
        /// </summary>
        /// <remarks>
        /// For this request to be accepted by the network, both the current private
        /// key(s) for this account and the new private key(s) must sign the transaction.  
        /// The existing key must sign for security and the new key must sign as a 
        /// safeguard to avoid accidentally changing the key to an invalid value.  
        /// </remarks>
        public Endorsement? Administrator { get; set; }
        /// <summary>
        /// Administrator key for signing transactions updating the grant or revoke 
        /// KYC status of an account.
        /// </summary>
        public Endorsement? GrantKycEndorsement { get; set; }
        /// <summary>
        /// Administrator key for signing transactions for freezing or unfreezing an 
        /// account's ability to transfer tokens.
        /// </summary>
        public Endorsement? SuspendEndorsement { get; set; }
        /// <summary>
        /// Administrator key for signing transaction that completely remove tokens
        /// from an crypto address.
        /// </summary>
        public Endorsement? ConfiscateEndorsement { get; set; }
        /// <summary>
        /// Administrator key for signing transactions for minting or unminting 
        /// tokens in the treasury account.
        /// </summary>
        public Endorsement? SupplyEndorsement { get; set; }
        /// <summary>
        /// If specified, replaces the current symbol for this token with
        /// the new Symbol.  The new symbol must not already be in use.
        /// </summary>
        public string Symbol { get; set; }
        /// <summary>
        /// If specified, replaces the current name of this token with
        /// the new name. The new name must not already be in use.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// If specified, changes to expiration new date, fees will be charged as appropriate.
        /// </summary>
        public DateTime? Expiration { get; set; }
        /// <summary>
        /// If specified, update the interval of the topic and auto-renewal period. If
        /// the associated renewal account does not have sufficient funds to 
        /// renew at the expiration time, it will be renewed for a period 
        /// of time the remaining funds can support.  If no funds remain, the
        /// topic instance will be deleted.
        /// </summary>
        public TimeSpan? RenewPeriod { get; set; }
        /// <summary>
        /// If specified updates the address of the account supporting the auto 
        /// renewal of the token at expiration time.  The topic lifetime will be
        /// extended by the RenewPeriod at expiration time if this account
        /// contains sufficient funds.  The private key associated with
        /// this account must sign the transaction if RenewAccount is
        /// specified.  Setting the value to <code>Address.None</code> clears the
        /// renewal account.
        /// </summary>
        public Address? RenewAccount { get; set; }
        /// <summary>
        /// Additional private key, keys or signing callback method 
        /// required to update this token.  Typically matches the
        /// Administrator endorsement associated with this token.
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
        /// <summary>
        /// The publicly visible memo to be associated with the token.
        /// </summary>
        public string? Memo { get; set; }
    }
}
