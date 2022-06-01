using Google.Protobuf.Collections;
using Hashgraph;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Proto;

public sealed partial class TokenTransferList
{
    internal TokenTransferList(Address token, Address fromAddress, Address toAddress, long amount) : this()
    {
        if (fromAddress is null)
        {
            throw new ArgumentNullException(nameof(toAddress), "Account to transfer from is missing. Please check that it is not null.");
        }
        if (toAddress is null)
        {
            throw new ArgumentNullException(nameof(toAddress), "Account to transfer to is missing. Please check that it is not null.");
        }
        if (amount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "The amount to transfer must be non-negative.");
        }
        Token = new TokenID(token);
        Transfers.Add(new AccountAmount(fromAddress, -amount, false));
        Transfers.Add(new AccountAmount(toAddress, amount, false));
    }
    internal TokenTransferList(Asset asset, Address fromAddress, Address toAddress) : this()
    {
        if (asset is null)
        {
            throw new ArgumentNullException(nameof(asset), "Asset to transfer is missing. Please check that it is not null.");
        }
        if (fromAddress is null)
        {
            throw new ArgumentNullException(nameof(fromAddress), "Account to transfer from is missing. Please check that it is not null.");
        }
        if (toAddress is null)
        {
            throw new ArgumentNullException(nameof(toAddress), "Account to transfer to is missing. Please check that it is not null.");
        }
        Token = new TokenID(asset);
        NftTransfers.Add(new NftTransfer
        {
            SenderAccountID = new AccountID(fromAddress),
            ReceiverAccountID = new AccountID(toAddress),
            SerialNumber = asset.SerialNum
        });
    }
}

internal static class TokenTransferExtensions
{
    private static ReadOnlyCollection<TokenTransfer> EMPTY_TOKEN_RESULT = new List<TokenTransfer>().AsReadOnly();
    private static ReadOnlyCollection<AssetTransfer> EMPTY_ASSET_RESULT = new List<AssetTransfer>().AsReadOnly();
    internal static (ReadOnlyCollection<TokenTransfer>, ReadOnlyCollection<AssetTransfer>) AsTokenAndAssetTransferLists(this RepeatedField<TokenTransferList> list)
    {
        if (list != null && list.Count > 0)
        {
            var tokenList = new List<TokenTransfer>(list.Count);
            var assetList = new List<AssetTransfer>(list.Count);
            foreach (var exchanges in list)
            {
                var token = exchanges.Token.AsAddress();
                foreach (var xfer in exchanges.Transfers)
                {
                    tokenList.Add(new TokenTransfer(token, xfer.AccountID.AsAddress(), xfer.Amount));
                }
                foreach (var xfer in exchanges.NftTransfers)
                {
                    assetList.Add(new AssetTransfer(new Asset(token, xfer.SerialNumber), xfer.SenderAccountID.AsAddress(), xfer.ReceiverAccountID.AsAddress()));
                }
            }
            return (tokenList.AsReadOnly(), assetList.AsReadOnly());
        }
        return (EMPTY_TOKEN_RESULT, EMPTY_ASSET_RESULT);
    }
}