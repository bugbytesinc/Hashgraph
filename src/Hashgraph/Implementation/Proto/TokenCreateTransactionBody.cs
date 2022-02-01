#pragma warning disable CS8604
using Grpc.Core;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Linq;
using System.Threading;

namespace Proto;

public sealed partial class TokenCreateTransactionBody : INetworkTransaction
{
    SchedulableTransactionBody INetworkTransaction.CreateSchedulableTransactionBody()
    {
        return new SchedulableTransactionBody { TokenCreation = this };
    }

    TransactionBody INetworkTransaction.CreateTransactionBody()
    {
        return new TransactionBody { TokenCreation = this };
    }

    Func<Transaction, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<TransactionResponse>> INetworkTransaction.InstantiateNetworkRequestMethod(Channel channel)
    {
        return new TokenService.TokenServiceClient(channel).createTokenAsync;
    }

    void INetworkTransaction.CheckReceipt(NetworkResult result)
    {
        if (result.Receipt.Status != ResponseCodeEnum.Success)
        {
            throw new TransactionException(string.Format("Unable to create Token, status: {0}", result.Receipt.Status), result);
        }
    }

    public TokenCreateTransactionBody(CreateTokenParams createParameters) : this()
    {
        if (createParameters is null)
        {
            throw new ArgumentNullException(nameof(createParameters), "The create parameters are missing. Please check that the argument is not null.");
        }
        if (string.IsNullOrWhiteSpace(createParameters.Name))
        {
            throw new ArgumentOutOfRangeException(nameof(createParameters.Name), "The name cannot be null or empty.");
        }
        if (string.IsNullOrWhiteSpace(createParameters.Symbol))
        {
            throw new ArgumentOutOfRangeException(nameof(createParameters.Symbol), "The token symbol must be specified.");
        }
        if (createParameters.Symbol.Trim().Length != createParameters.Symbol.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(createParameters.Symbol), "The token symbol cannot contain leading or trailing white space.");
        }
        if (createParameters.Symbol.Length > 32)
        {
            throw new ArgumentOutOfRangeException(nameof(createParameters.Symbol), "The token symbol cannot exceed 32 characters in length.");
        }
        if (!createParameters.Symbol.Equals(createParameters.Symbol.ToUpperInvariant()))
        {
            throw new ArgumentOutOfRangeException(nameof(createParameters.Symbol), "The token symbol must contain upper case characters.");
        }
        if (createParameters.Circulation < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(createParameters.Circulation), "The initial circulation of tokens must be greater than zero.");
        }
        if (createParameters.Decimals < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(createParameters.Decimals), "The divisibility of tokens cannot be negative.");
        }
        if (createParameters.Treasury is null || createParameters.Treasury == Hashgraph.Address.None)
        {
            throw new ArgumentOutOfRangeException(nameof(createParameters.Treasury), "The treasury must be specified.");
        }
        if (createParameters.Expiration < DateTime.UtcNow)
        {
            throw new ArgumentOutOfRangeException(nameof(createParameters.Expiration), "The expiration time must be in the future.");
        }
        if (createParameters.RenewAccount.IsNullOrNone() == createParameters.RenewPeriod.HasValue)
        {
            throw new ArgumentOutOfRangeException(nameof(createParameters.RenewPeriod), "Both the renew account and period must be specified, or not at all.");
        }
        if (!string.IsNullOrEmpty(createParameters.Memo))
        {
            if (createParameters.Memo.Trim().Length != createParameters.Memo.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(createParameters.Memo), "The token memo cannot contain leading or trailing white space.");
            }
        }
        Name = createParameters.Name;
        Symbol = createParameters.Symbol;
        InitialSupply = createParameters.Circulation;
        Decimals = createParameters.Decimals;
        Treasury = new AccountID(createParameters.Treasury);
        TokenType = TokenType.FungibleCommon;
        if (createParameters.Ceiling > 0 && createParameters.Ceiling < long.MaxValue)
        {
            MaxSupply = createParameters.Ceiling;
            SupplyType = TokenSupplyType.Finite;
        }
        else
        {
            SupplyType = TokenSupplyType.Infinite;
        }
        if (!createParameters.Administrator.IsNullOrNone())
        {
            AdminKey = new Key(createParameters.Administrator);
        }
        if (!createParameters.GrantKycEndorsement.IsNullOrNone())
        {
            KycKey = new Key(createParameters.GrantKycEndorsement);
        }
        if (!createParameters.SuspendEndorsement.IsNullOrNone())
        {
            FreezeKey = new Key(createParameters.SuspendEndorsement);
        }
        if (!createParameters.PauseEndorsement.IsNullOrNone())
        {
            PauseKey = new Key(createParameters.PauseEndorsement);
        }
        if (!createParameters.ConfiscateEndorsement.IsNullOrNone())
        {
            WipeKey = new Key(createParameters.ConfiscateEndorsement);
        }
        if (!createParameters.SupplyEndorsement.IsNullOrNone())
        {
            SupplyKey = new Key(createParameters.SupplyEndorsement);
        }
        if (!createParameters.RoyaltiesEndorsement.IsNullOrNone())
        {
            FeeScheduleKey = new Key(createParameters.RoyaltiesEndorsement);
        }
        if (createParameters.Royalties is not null)
        {
            CustomFees.AddRange(createParameters.Royalties.Select(royalty => new CustomFee(royalty)));
        }
        FreezeDefault = createParameters.InitializeSuspended;
        Expiry = new Timestamp(createParameters.Expiration);
        if (!createParameters.RenewAccount.IsNullOrNone())
        {
            AutoRenewAccount = new AccountID(createParameters.RenewAccount);
        }
        if (createParameters.RenewPeriod.HasValue)
        {
            AutoRenewPeriod = new Duration(createParameters.RenewPeriod.Value);
        }
        Memo = createParameters.Memo ?? string.Empty;
    }
    public TokenCreateTransactionBody(CreateAssetParams createParameters) : this()
    {
        if (createParameters is null)
        {
            throw new ArgumentNullException(nameof(createParameters), "The create parameters are missing. Please check that the argument is not null.");
        }
        if (string.IsNullOrWhiteSpace(createParameters.Name))
        {
            throw new ArgumentOutOfRangeException(nameof(createParameters.Name), "The name cannot be null or empty.");
        }
        if (string.IsNullOrWhiteSpace(createParameters.Symbol))
        {
            throw new ArgumentOutOfRangeException(nameof(createParameters.Symbol), "The token symbol must be specified.");
        }
        if (createParameters.Symbol.Trim().Length != createParameters.Symbol.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(createParameters.Symbol), "The token symbol cannot contain leading or trailing white space.");
        }
        if (createParameters.Symbol.Length > 32)
        {
            throw new ArgumentOutOfRangeException(nameof(createParameters.Symbol), "The token symbol cannot exceed 32 characters in length.");
        }
        if (!createParameters.Symbol.Equals(createParameters.Symbol.ToUpperInvariant()))
        {
            throw new ArgumentOutOfRangeException(nameof(createParameters.Symbol), "The token symbol must contain upper case characters.");
        }
        if (createParameters.Treasury is null || createParameters.Treasury == Hashgraph.Address.None)
        {
            throw new ArgumentOutOfRangeException(nameof(createParameters.Treasury), "The treasury must be specified.");
        }
        if (createParameters.Expiration < DateTime.UtcNow)
        {
            throw new ArgumentOutOfRangeException(nameof(createParameters.Expiration), "The expiration time must be in the future.");
        }
        if (createParameters.RenewAccount.IsNullOrNone() == createParameters.RenewPeriod.HasValue)
        {
            throw new ArgumentOutOfRangeException(nameof(createParameters.RenewPeriod), "Both the renew account and period must be specified, or not at all.");
        }
        if (!string.IsNullOrEmpty(createParameters.Memo))
        {
            if (createParameters.Memo.Trim().Length != createParameters.Memo.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(createParameters.Memo), "The token memo cannot contain leading or trailing white space.");
            }
        }
        Name = createParameters.Name;
        Symbol = createParameters.Symbol;
        Treasury = new AccountID(createParameters.Treasury);
        TokenType = TokenType.NonFungibleUnique;
        if (createParameters.Ceiling > 0 && createParameters.Ceiling < long.MaxValue)
        {
            MaxSupply = createParameters.Ceiling;
            SupplyType = TokenSupplyType.Finite;
        }
        else
        {
            SupplyType = TokenSupplyType.Infinite;
        }
        if (!createParameters.Administrator.IsNullOrNone())
        {
            AdminKey = new Key(createParameters.Administrator);
        }
        if (!createParameters.GrantKycEndorsement.IsNullOrNone())
        {
            KycKey = new Key(createParameters.GrantKycEndorsement);
        }
        if (!createParameters.SuspendEndorsement.IsNullOrNone())
        {
            FreezeKey = new Key(createParameters.SuspendEndorsement);
        }
        if (!createParameters.PauseEndorsement.IsNullOrNone())
        {
            PauseKey = new Key(createParameters.PauseEndorsement);
        }
        if (!createParameters.PauseEndorsement.IsNullOrNone())
        {
            PauseKey = new Key(createParameters.PauseEndorsement);
        }
        if (!createParameters.ConfiscateEndorsement.IsNullOrNone())
        {
            WipeKey = new Key(createParameters.ConfiscateEndorsement);
        }
        if (!createParameters.SupplyEndorsement.IsNullOrNone())
        {
            SupplyKey = new Key(createParameters.SupplyEndorsement);
        }
        if (!createParameters.RoyaltiesEndorsement.IsNullOrNone())
        {
            FeeScheduleKey = new Key(createParameters.RoyaltiesEndorsement);
        }
        if (createParameters.Royalties is not null)
        {
            CustomFees.AddRange(createParameters.Royalties.Select(royalty => new CustomFee(royalty)));
        }
        FreezeDefault = createParameters.InitializeSuspended;
        Expiry = new Timestamp(createParameters.Expiration);
        if (!createParameters.RenewAccount.IsNullOrNone())
        {
            AutoRenewAccount = new AccountID(createParameters.RenewAccount);
        }
        if (createParameters.RenewPeriod.HasValue)
        {
            AutoRenewPeriod = new Duration(createParameters.RenewPeriod.Value);
        }
        Memo = createParameters.Memo ?? string.Empty;
    }
}