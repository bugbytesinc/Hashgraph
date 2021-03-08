namespace Proto
{
    public sealed partial class TokenInfo
    {
        internal Hashgraph.TokenInfo ToTokenInfo()
        {
            return new Hashgraph.TokenInfo
            {
                Token = TokenId.AsAddress(),
                Symbol = Symbol,
                Name = Name,
                Treasury = Treasury.AsAddress(),
                Circulation = TotalSupply,
                Decimals = Decimals,
                Administrator = AdminKey?.ToEndorsement(),
                GrantKycEndorsement = KycKey?.ToEndorsement(),
                SuspendEndorsement = FreezeKey?.ToEndorsement(),
                ConfiscateEndorsement = WipeKey?.ToEndorsement(),
                SupplyEndorsement = SupplyKey?.ToEndorsement(),
                TradableStatus = (Hashgraph.TokenTradableStatus)DefaultFreezeStatus,
                KycStatus = (Hashgraph.TokenKycStatus)DefaultKycStatus,
                Expiration = Expiry.ToDateTime(),
                RenewPeriod = AutoRenewPeriod?.ToTimeSpan(),
                RenewAccount = AutoRenewAccount?.AsAddress(),
                Deleted = Deleted,
                Memo = Memo
            };
        }
    }
}
