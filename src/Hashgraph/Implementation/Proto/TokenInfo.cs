using Hashgraph.Implementation;
using System;

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
                //Name = Name,
                Treasury = Treasury.ToAddress(),
                Circulation = CurrentFloat,
                Decimals = Divisibility,
                Administrator = AdminKey?.ToEndorsement(),
                GrantKycEndorsement = KycKey?.ToEndorsement(),
                SuspendEndorsement = FreezeKey?.ToEndorsement(),
                ConfiscateEndorsement = WipeKey?.ToEndorsement(),
                SupplyEndorsement = SupplyKey?.ToEndorsement(),
                TradableStatus = (Hashgraph.TokenTradableStatus)DefaultFreezeStatus,
                KycStatus = (Hashgraph.TokenKycStatus)DefaultKycStatus,
                //Expiration = Epoch.ToDate((long)Expiry, 0),
                //RenewPeriod = TimeSpan.FromSeconds(AutoRenewPeriod),
                //RenewAccount = AutoRenewAccount?.ToAddress(),
                Deleted = IsDeleted
            };
        }
    }
}
