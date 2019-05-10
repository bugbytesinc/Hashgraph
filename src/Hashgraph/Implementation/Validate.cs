using Proto;
using System;

namespace Hashgraph.Implementation
{
    /// <summary>
    /// Internal helper class validating server responses.
    /// </summary>
    internal static class Validate
    {
        internal static void ValidatePreCheckResult(TransactionID transactionId, ResponseCodeEnum code)
        {
            if (code == ResponseCodeEnum.Ok)
            {
                return;
            }
            throw new PrecheckException($"Transaction Failed Pre-Check: {code}", Protobuf.FromTransactionId(transactionId), (ResponseCode)code);
        }

        internal static long AcountNumberArgument(long accountNum)
        {
            if (accountNum < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(accountNum), "Account Number cannot be negative.");
            }
            return accountNum;
        }

        internal static long ShardNumberArgument(long shardNum)
        {
            if (shardNum < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(shardNum), "Shard Number cannot be negative.");
            }
            return shardNum;
        }

        internal static long RealmNumberArgument(long realmNum)
        {
            if (realmNum < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(realmNum), "Realm Number cannot be negative.");
            }
            return realmNum;
        }
        internal static string UrlArgument(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentOutOfRangeException(nameof(url), "URL is required.");
            }
            return url;
        }
    }
}