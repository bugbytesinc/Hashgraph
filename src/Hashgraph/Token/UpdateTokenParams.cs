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
        public TokenIdentifier Token { get; set; }
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
    }
}
