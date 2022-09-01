using Grpc.Core;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Threading;

namespace Proto;

public sealed partial class CryptoApproveAllowanceTransactionBody : INetworkTransaction
{
    SchedulableTransactionBody INetworkTransaction.CreateSchedulableTransactionBody()
    {
        return new SchedulableTransactionBody { CryptoApproveAllowance = this };
    }

    TransactionBody INetworkTransaction.CreateTransactionBody()
    {
        return new TransactionBody { CryptoApproveAllowance = this };
    }

    Func<Transaction, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<TransactionResponse>> INetworkTransaction.InstantiateNetworkRequestMethod(Channel channel)
    {
        return new CryptoService.CryptoServiceClient(channel).approveAllowancesAsync;
    }

    void INetworkTransaction.CheckReceipt(NetworkResult result)
    {
        if (result.Receipt.Status != ResponseCodeEnum.Success)
        {
            throw new TransactionException(string.Format("Unable to create allowances, status: {0}", result.Receipt.Status), result);
        }
    }

    internal CryptoApproveAllowanceTransactionBody(AllowanceParams allowances) : this()
    {
        if (allowances == null)
        {
            throw new ArgumentNullException(nameof(allowances), "The allowance parametes cannot not be null.");
        }
        if (allowances.CryptoAllowances is not null)
        {
            foreach (var allowance in allowances.CryptoAllowances)
            {
                CryptoAllowances.Add(new CryptoAllowance(allowance));
            }
        }
        if (allowances.TokenAllowances is not null)
        {
            foreach (var allowance in allowances.TokenAllowances)
            {
                TokenAllowances.Add(new TokenAllowance(allowance));
            }
        }
        if (allowances.AssetAllowances is not null)
        {
            foreach (var allowance in allowances.AssetAllowances)
            {
                NftAllowances.Add(new NftAllowance(allowance));
            }
        }
        if (CryptoAllowances.Count == 0 && TokenAllowances.Count == 0 && NftAllowances.Count == 0)
        {
            throw new ArgumentException(nameof(allowances), "Both crypto, token and asset allowance lists are null or empty.  At least one must include a net amount.");
        }
    }
}