#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

using System;

namespace Hashgraph.Extensions;

/// <summary>
/// Information regarding a node from the signed
/// address book.
/// </summary>
public sealed record NodeInfo
{
    /// <summary>
    /// Identifier of the node (non-sequential)
    /// </summary>
    public long Id { get; internal init; }
    /// <summary>
    /// The RSA public key of the node. Used to sign stream files 
    /// (e.g., record stream files). Precisely, this field is a string 
    /// of hexadecimal characters which, translated to binary, are the 
    /// public key's DER encoding.  
    /// </summary>
    public string RsaPublicKey { get; internal init; }
    /// <summary>
    /// The crypto account associated with this node.
    /// </summary>
    public Address Address { get; internal init; }
    /// <summary>
    /// Hash of the nodes TLS certificate. This field is a string of 
    /// hexadecimal characters which, translated to binary, are the SHA-384 hash of 
    /// the UTF-8 NFKD encoding of the node's TLS cert in PEM format. Its value can be 
    /// used to verify the node's certificate it presents during TLS negotiations.
    /// </summary>
    public ReadOnlyMemory<byte> CertificateHash { get; internal init; }
    /// <summary>
    /// List of public ip addresses and ports exposed by this node.
    /// </summary>
    public Endpoint[] Endpoints { get; internal init; }
    /// <summary>
    /// A Description of the node.
    /// </summary>
    public string Description { get; internal set; }
    /// <summary>
    /// The amount of tinybars staked to the node.
    /// </summary>
    public long Stake { get; internal set; }
}