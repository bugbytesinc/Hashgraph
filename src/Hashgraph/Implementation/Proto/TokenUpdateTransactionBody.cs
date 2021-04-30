using Grpc.Core;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Threading;

namespace Proto
{
    public sealed partial class TokenUpdateTransactionBody : INetworkTransaction
    {
        string INetworkTransaction.TransactionExceptionMessage => "Unable to update Token, status: {0}";

        SchedulableTransactionBody INetworkTransaction.CreateSchedulableTransactionBody()
        {
            return new SchedulableTransactionBody { TokenUpdate = this };
        }

        TransactionBody INetworkTransaction.CreateTransactionBody()
        {
            return new TransactionBody { TokenUpdate = this };
        }

        Func<Transaction, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<TransactionResponse>> INetworkTransaction.InstantiateNetworkRequestMethod(Channel channel)
        {
            return new TokenService.TokenServiceClient(channel).updateTokenAsync;
        }

        internal TokenUpdateTransactionBody(Hashgraph.UpdateTokenParams updateParameters) : this()
        {
            if (updateParameters is null)
            {
                throw new ArgumentNullException(nameof(updateParameters), "Token Update Parameters argument is missing. Please check that it is not null.");
            }
            if (updateParameters.Token.IsNullOrNone())
            {
                throw new ArgumentNullException(nameof(updateParameters.Token), "The Token is missing.  Please check that it is not null or empty.");
            }
            if (updateParameters.Treasury is null &&
                updateParameters.Administrator is null &&
                updateParameters.GrantKycEndorsement is null &&
                updateParameters.SuspendEndorsement is null &&
                updateParameters.ConfiscateEndorsement is null &&
                updateParameters.SupplyEndorsement is null &&
                string.IsNullOrWhiteSpace(updateParameters.Symbol) &&
                string.IsNullOrWhiteSpace(updateParameters.Name) &&
                !updateParameters.Expiration.HasValue &&
                !updateParameters.RenewPeriod.HasValue &&
                updateParameters.RenewAccount is null &&
                updateParameters.Memo is null)
            {
                throw new ArgumentException("The Topic Updates contain no update properties, it is blank.", nameof(updateParameters));
            }
            Token = new TokenID(updateParameters.Token);
            if (!string.IsNullOrWhiteSpace(updateParameters.Symbol))
            {
                if (updateParameters.Symbol.Trim().Length != updateParameters.Symbol.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(updateParameters.Symbol), "The new token symbol cannot contain leading or trailing white space.");
                }
                if (updateParameters.Symbol.Length > 32)
                {
                    throw new ArgumentOutOfRangeException(nameof(updateParameters.Symbol), "The new token symbol cannot exceed 32 characters in length.");
                }
                if (!updateParameters.Symbol.Equals(updateParameters.Symbol.ToUpperInvariant()))
                {
                    throw new ArgumentOutOfRangeException(nameof(updateParameters.Symbol), "The new token symbol must contain upper case characters.");
                }
                Symbol = updateParameters.Symbol;
            }
            if (!string.IsNullOrWhiteSpace(updateParameters.Name))
            {
                if (updateParameters.Name.Trim().Length != updateParameters.Name.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(updateParameters.Name), "The new token name cannot contain leading or trailing white space.");
                }
                Name = updateParameters.Name;
            }
            if (updateParameters.Expiration.HasValue)
            {
                if (updateParameters.Expiration.Value < DateTime.UtcNow)
                {
                    throw new ArgumentOutOfRangeException(nameof(updateParameters.Expiration), "The new expiration can not be set to the past.");
                }
                Expiry = new Timestamp(updateParameters.Expiration.Value);
            }
            if (updateParameters.RenewPeriod.HasValue)
            {
                if (updateParameters.RenewPeriod.Value.TotalSeconds < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(updateParameters.RenewPeriod), "The renew period must be non negative.");
                }
                AutoRenewPeriod = new Duration(updateParameters.RenewPeriod.Value);
            }
            if (updateParameters.Memo is not null)
            {
                if (updateParameters.Memo.Trim().Length != updateParameters.Memo.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(updateParameters.Memo), "The new token memo cannot contain leading or trailing white space.");
                }
                Memo = updateParameters.Memo;
            }
            if (!(updateParameters.Treasury is null))
            {
                Treasury = new AccountID(updateParameters.Treasury);
            }
            if (!(updateParameters.Administrator is null))
            {
                AdminKey = new Key(updateParameters.Administrator);
            }
            if (!(updateParameters.GrantKycEndorsement is null))
            {
                KycKey = new Key(updateParameters.GrantKycEndorsement);
            }
            if (!(updateParameters.SuspendEndorsement is null))
            {
                FreezeKey = new Key(updateParameters.SuspendEndorsement);
            }
            if (!(updateParameters.ConfiscateEndorsement is null))
            {
                WipeKey = new Key(updateParameters.ConfiscateEndorsement);
            }
            if (!(updateParameters.SupplyEndorsement is null))
            {
                SupplyKey = new Key(updateParameters.SupplyEndorsement);
            }
            if (!(updateParameters.RenewAccount is null))
            {
                AutoRenewAccount = new AccountID(updateParameters.RenewAccount);
            }
        }
    }
}
