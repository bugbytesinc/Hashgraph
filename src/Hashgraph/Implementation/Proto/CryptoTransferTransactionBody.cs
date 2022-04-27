using Grpc.Core;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Proto;

public sealed partial class CryptoTransferTransactionBody : INetworkTransaction
{
    SchedulableTransactionBody INetworkTransaction.CreateSchedulableTransactionBody()
    {
        return new SchedulableTransactionBody { CryptoTransfer = this };
    }

    TransactionBody INetworkTransaction.CreateTransactionBody()
    {
        return new TransactionBody { CryptoTransfer = this };
    }

    Func<Transaction, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<TransactionResponse>> INetworkTransaction.InstantiateNetworkRequestMethod(Channel channel)
    {
        return new CryptoService.CryptoServiceClient(channel).cryptoTransferAsync;
    }

    void INetworkTransaction.CheckReceipt(NetworkResult result)
    {
        if (result.Receipt.Status != ResponseCodeEnum.Success)
        {
            throw new TransactionException(string.Format("Unable to execute transfers, status: {0}", result.Receipt.Status), result);
        }
    }

    internal CryptoTransferTransactionBody(Address fromAddress, Address toAddress, long amount) : this()
    {
        if (fromAddress is null)
        {
            throw new ArgumentNullException(nameof(fromAddress), "Account to transfer from is missing. Please check that it is not null.");
        }
        if (toAddress is null)
        {
            throw new ArgumentNullException(nameof(toAddress), "Account to transfer to is missing. Please check that it is not null.");
        }
        if (amount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "The amount to transfer must be non-negative.");
        }
        var xferList = new TransferList();
        xferList.AccountAmounts.Add(new AccountAmount(fromAddress, -amount));
        xferList.AccountAmounts.Add(new AccountAmount(toAddress, amount));
        Transfers = xferList;
    }
    internal CryptoTransferTransactionBody(Address token, Address fromAddress, Address toAddress, long amount) : this()
    {
        TokenTransfers.Add(new TokenTransferList(token, fromAddress, toAddress, amount));
    }
    internal CryptoTransferTransactionBody(Asset asset, Address fromAddress, Address toAddress) : this()
    {
        TokenTransfers.Add(new TokenTransferList(asset, fromAddress, toAddress));
    }
    internal CryptoTransferTransactionBody(TransferParams transfers) : this()
    {
        if (transfers == null)
        {
            throw new ArgumentNullException(nameof(transfers), "The transfer parametes cannot not be null.");
        }
        var missingTransfers = true;
        if (transfers.CryptoTransfers is not null)
        {
            long sum = 0;
            var netRequests = new Dictionary<Address, long>();
            foreach (var transfer in transfers.CryptoTransfers)
            {
                if (transfer.Value == 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(transfers.CryptoTransfers), $"The amount to transfer crypto to/from {transfer.Key} must be a value, negative for transfers out, and positive for transfers in. A value of zero is not allowed.");
                }
                if (netRequests.TryGetValue(transfer.Key, out long value))
                {
                    netRequests[transfer.Key] = value + transfer.Value;
                }
                else
                {
                    netRequests[transfer.Key] = transfer.Value;
                }
                sum += transfer.Value;
            }
            if (netRequests.Count == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(transfers.CryptoTransfers), "The dictionary of crypto transfers can not be empty.");
            }
            if (sum != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(transfers.CryptoTransfers), "The sum of crypto sends and receives does not balance.");
            }
            var xferList = new TransferList();
            foreach (var transfer in netRequests)
            {
                if (transfer.Value != 0)
                {
                    xferList.AccountAmounts.Add(new AccountAmount(transfer.Key, transfer.Value));
                }
            }
            missingTransfers = xferList.AccountAmounts.Count == 0;
            Transfers = xferList;
        }
        if (transfers.TokenTransfers is not null)
        {
            foreach (var tokenGroup in transfers.TokenTransfers.GroupBy(txfer => txfer.Token))
            {
                if (tokenGroup.Key.IsNullOrNone())
                {
                    throw new ArgumentException("Token", "The list of token transfers cannot contain a null or empty Token value.");
                }
                long sum = 0;
                var netRequests = new Dictionary<Address, long>();
                foreach (var xfer in tokenGroup)
                {
                    if (xfer.Address.IsNullOrNone())
                    {
                        throw new ArgumentException(nameof(xfer.Address), "The list of token transfers cannot contain a null or empty account value.");
                    }
                    if (xfer.Amount == 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(xfer.Amount), $"The amount to transfer tokens to/from {xfer.Address} must be a value, negative for transfers out, and positive for transfers in. A value of zero is not allowed.");
                    }
                    if (netRequests.TryGetValue(xfer.Address, out long value))
                    {
                        netRequests[xfer.Address] = value + xfer.Amount;
                    }
                    else
                    {
                        netRequests[xfer.Address] = xfer.Amount;
                    }
                    sum += xfer.Amount;
                }
                if (sum != 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(transfers.TokenTransfers), $"The sum of token sends and receives for {tokenGroup.Key.ShardNum}.{tokenGroup.Key.RealmNum}.{tokenGroup.Key.AccountNum} does not balance.");
                }
                var xferList = new TokenTransferList
                {
                    Token = new TokenID(tokenGroup.Key)
                };
                foreach (var netTransfer in netRequests)
                {
                    if (netTransfer.Value != 0)
                    {
                        xferList.Transfers.Add(new AccountAmount(netTransfer.Key, netTransfer.Value));
                    }
                }
                if (xferList.Transfers.Count > 0)
                {
                    TokenTransfers.Add(xferList);
                }
            }
        }
        if (transfers.AssetTransfers is not null)
        {
            foreach (var tokenGroup in transfers.AssetTransfers.GroupBy(txfer => (Address)txfer.Asset))
            {
                if (tokenGroup.Key.IsNullOrNone())
                {
                    throw new ArgumentException("Asset", "The list of asset token transfers cannot contain a null or empty Asset Token address.");
                }
                var netRequests = new Dictionary<long, AssetTransfer>();
                foreach (var xfer in tokenGroup)
                {
                    if (xfer.Asset.SerialNum <= 0)
                    {
                        throw new ArgumentException(nameof(xfer.Asset), "The list of asset transfers cannot contain an asset without a serial number.");
                    }
                    if (xfer.From.IsNullOrNone())
                    {
                        throw new ArgumentException(nameof(xfer.From), "The list of asset transfers cannot contain a null or empty from account value.");
                    }
                    if (xfer.To.IsNullOrNone())
                    {
                        throw new ArgumentException(nameof(xfer.From), "The list of asset transfers cannot contain a null or empty to account value.");
                    }
                    if (netRequests.ContainsKey(xfer.Asset.SerialNum))
                    {
                        throw new ArgumentException(nameof(xfer.Asset), "The list of asset transfers cannot contain the same asset in multiple transfers at once.");
                    }
                    else
                    {
                        netRequests[xfer.Asset.SerialNum] = xfer;
                    }
                }
                if (netRequests.Count > 0)
                {
                    var xferList = new TokenTransferList
                    {
                        Token = new TokenID(tokenGroup.Key)
                    };
                    foreach (var netTransfer in netRequests)
                    {
                        xferList.NftTransfers.Add(new NftTransfer(netTransfer.Value));
                    }
                    TokenTransfers.Add(xferList);
                }
            }
        }
        missingTransfers &= TokenTransfers.Count == 0;
        if (missingTransfers)
        {
            throw new ArgumentException(nameof(transfers), "Both crypto, token and asset transfer lists are null or empty.  At least one must include net transfers.");
        }
    }
}