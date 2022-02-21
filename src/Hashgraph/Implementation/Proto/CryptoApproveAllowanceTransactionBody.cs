using Google.Protobuf;
using Grpc.Core;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
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
        return new SmartContractService.SmartContractServiceClient(channel).createContractAsync;
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
            foreach(var allowance in allowances.CryptoAllowances)
            {
                CryptoAllowances.Add(new CryptoAllowance(allowance, true));
            }
        }
        if (CryptoAllowances.Count == 0)
        {
            throw new ArgumentException(nameof(allowances), "Both crypto, token and asset allowance lists are null or empty.  At least one must include a net amount.");
        }
    }
}