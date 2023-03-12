using Google.Protobuf;
using Hashgraph;
using System;

namespace Proto;

public sealed partial class AccountID
{
    internal AccountID(Address account) : this()
    {
        if (account is null)
        {
            throw new ArgumentNullException(nameof(account), "Account Address/Alias is missing. Please check that it is not null.");
        }
        if (account.TryGetAlias(out var alias))
        {
            ShardNum = alias.ShardNum;
            RealmNum = alias.RealmNum;
            Alias = new Key(alias.Endorsement).ToByteString();
        }
        else if (account.TryGetMoniker(out var moniker))
        {
            ShardNum = moniker.ShardNum;
            RealmNum = moniker.RealmNum;
            // BEGIN NETWORK DEFECT: Should we not be using EvmAddress instead?
            // See https://github.com/hashgraph/hedera-services/issues/4606
            // EXPECTED CODE: EvmAddress = ByteString.CopyFrom(moniker.Bytes.Span);
            // Code that works:
            Alias = ByteString.CopyFrom(moniker.Bytes.Span);
            // END DEFECT            
        }
        else if (account.AddressType == AddressType.ShardRealmNum)
        {
            ShardNum = account.ShardNum;
            RealmNum = account.RealmNum;
            AccountNum = account.AccountNum;
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(account), "Crypto Account Address does not appear to be a valid <shard>.<realm>.<num> or alias.");
        }
    }
}

internal static class AccountIDExtensions
{
    internal static Address AsAddress(this AccountID? accountId)
    {
        return accountId?.AccountCase switch
        {
            AccountID.AccountOneofCase.AccountNum => new Address(accountId.ShardNum, accountId.RealmNum, accountId.AccountNum),
            // HIP-583 Churn
            //AccountID.AccountOneofCase.EvmAddress => new Address(new Moniker(accountId.ShardNum, accountId.RealmNum, accountId.EvmAddress.Memory)),
            AccountID.AccountOneofCase.Alias => new Address(new Alias(accountId.ShardNum, accountId.RealmNum, Key.Parser.ParseFrom(accountId.Alias.Memory.Span).ToEndorsement())),
            _ => Address.None
        };
    }
}