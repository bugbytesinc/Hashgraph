#pragma warning disable CS8618

using System;

namespace Hashgraph
{
    /// <summary>
    /// Token Creation Parameters.
    /// </summary>
    /// <remarks>
    /// The Name and Symbol properties must be unique within the network.
    /// If there are other tokens defined with the same name or symbol, respectively 
    /// <code>TOKEN_SYMBOL_ALREADY_IN_USE</code> and <code>TOKEN_NAME_ALREADY_IN_USE</code>
    /// errors are returned.
    /// 
    /// The specified Treasury Account is receiving the initial supply of tokens as-well 
    /// as the tokens from Token Mint operations when executed.  The balance of the treasury 
    /// account is decreased when the Token Burn operation is executed.
    /// 
    /// The supply that is going to be put in circulation is going to be <code>S*(10^D)</code>,
    /// where <code>S</code> is initial supply and <code>D</code> is Decimals. The maximum supply 
    /// a token can have is <code>S* (10^D) < 2^63</code>.
    /// 
    /// The token can be created as immutable if the <code>Administrator</code> endorsement is omitted
    /// or set to <code>None</code>.  In this case, the name, symbol, treasury, management keys, Expiration
    /// and renew properties cannot be updated. If a token is created as immutable, any account is able to 
    /// extend the expiration time by paying the fee.
    /// </remarks>
    public sealed class CreateTokenParams
    {
        /// <summary>
        /// A string containing only upper case ASCII alpha characters identifying this token.
        /// </summary>
        public string Symbol { get; set; }
        /// <summary>
        /// The initial number of whole tokens to placed into the token treasury
        /// account upon creation of the token.  The Treasury will be given units 
        /// in the smallest denomination <code>(Circulation * 10 ^ Decimals)</code>.
        /// </summary>
        public ulong Circulation { get; set; }
        /// <summary>
        /// The number of decimal places token may be subdivided.
        /// </summary>
        public uint Decimals { get; set; }
        /// <summary>
        /// The treasury account receiving the Initial Circulation balance of tokens.
        /// </summary>
        public Address Treasury { get; set; }
        /// <summary>
        /// Administrator key for signing transactions modifying this token's properties.
        /// </summary>
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
        /// The default frozen setting for current and newly created accounts.  A value 
        /// of <code>true</code> will default crypto account status of <code>Frozen</code> 
        /// with relationship to this token.  A value of <code>false</code> will default 
        /// to an tradable/unfrozen relationship.
        /// </summary>
        public bool InitializeSuspended { get; set; }
        /// <summary>
        /// Additional private key, keys or signing callback method 
        /// required to create to this token.  Typically matches the
        /// Administrator, KycEndorsement, FreezeEndorsement and other
        /// listed endorsements associated with this token.
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
