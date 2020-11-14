namespace Proto
{
    public sealed partial class TokenInfo
    {
        internal Hashgraph.TokenInfo ToTokenInfo()
        {
            return new Hashgraph.TokenInfo
            {
                Token = TokenId.ToAddress(),
                Symbol = Symbol,
                Name = Name,
                Treasury = Treasury.ToAddress(),
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
                RenewAccount = AutoRenewAccount?.ToAddress(),
                Deleted = Deleted
            };
        }
    }
}
