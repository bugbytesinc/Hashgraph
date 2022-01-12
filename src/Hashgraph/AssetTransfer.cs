namespace Hashgraph;

/// <summary>
/// Represents a token transfer (Token, Account, Amount)
/// </summary>
public sealed record AssetTransfer
{
    /// <summary>
    /// The Address and Serial Number of the asset to transfer.
    /// </summary>
    public Asset Asset { get; private init; }
    /// <summary>
    /// The Address having the asset to send.
    /// </summary>
    public AddressOrAlias From { get; private init; }
    /// <summary>
    /// The Address receiving the asset.
    /// </summary>
    public AddressOrAlias To { get; private init; }
    /// <summary>
    /// Internal Constructor representing the "None" version of an
    /// version.  This is a special construct indicating the version
    /// number is not known or is not specified.
    /// </summary>
    private AssetTransfer()
    {
        Asset = Asset.None;
        From = Address.None;
        To = Address.None;
    }
    /// <summary>
    /// Public Constructor, an <code>TokenTransfer</code> is immutable after creation.
    /// </summary>
    /// <param name="asset">
    /// The address and serial number of the asset to transfer.
    /// </param>
    /// <param name="fromAddress">
    /// The address of the crypto account having the token.
    /// </param>
    /// <param name="toAddress">
    /// The address of the crypto account sending the token.
    /// </param>
    public AssetTransfer(Asset asset, AddressOrAlias fromAddress, AddressOrAlias toAddress)
    {
        Asset = asset;
        From = fromAddress;
        To = toAddress;
    }
}