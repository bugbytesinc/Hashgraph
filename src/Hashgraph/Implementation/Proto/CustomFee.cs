using Hashgraph;
using System;

namespace Proto
{
    public sealed partial class CustomFee
    {
        internal CustomFee(IRoyalty royalty) : this()
        {
            FeeCollectorAccountId = new AccountID(royalty.Account);
            switch (royalty.RoyaltyType)
            {
                case RoyaltyType.Fixed:
                    var fixedRoyalty = royalty as FixedRoyalty;
                    if (fixedRoyalty is null)
                    {
                        throw new ArgumentException("Royalty had type of Fixed but was not a Fixed Royalty", nameof(royalty));
                    }
                    FixedFee = new FixedFee
                    {
                        Amount = fixedRoyalty.Amount,
                        DenominatingTokenId = fixedRoyalty.Token != Address.None ? new TokenID(fixedRoyalty.Token) : null
                    };
                    break;
                case RoyaltyType.Asset:
                    var assetRoyalty = royalty as AssetRoyalty;
                    if (assetRoyalty is null)
                    {
                        throw new ArgumentException("Royalty had type of Value (Royalty) but was not a Value Royalty", nameof(royalty));
                    }
                    RoyaltyFee = new RoyaltyFee
                    {
                        ExchangeValueFraction = new Fraction
                        {
                            Numerator = assetRoyalty.Numerator,
                            Denominator = assetRoyalty.Denominator
                        }
                    };
                    if (assetRoyalty.FallbackAmount > 0 || !assetRoyalty.FallbackToken.IsNullOrNone())
                    {
                        RoyaltyFee.FallbackFee = new FixedFee
                        {
                            Amount = assetRoyalty.FallbackAmount,
                            DenominatingTokenId = assetRoyalty.FallbackToken != Address.None ? new TokenID(assetRoyalty.FallbackToken) : null
                        };
                    }

                    break;
                case RoyaltyType.Token:
                    var tokenRoyalty = royalty as TokenRoyalty;
                    if (tokenRoyalty is null)
                    {
                        throw new ArgumentException("Royalty had type of Fractional but was not a Fractional Royalty", nameof(royalty));
                    }
                    FractionalFee = new FractionalFee
                    {
                        MinimumAmount = tokenRoyalty.Minimum,
                        MaximumAmount = tokenRoyalty.Maximum,
                        FractionalAmount = new Fraction
                        {
                            Numerator = tokenRoyalty.Numerator,
                            Denominator = tokenRoyalty.Denominator
                        },
                        NetOfTransfers = tokenRoyalty.AssessAsSurcharge
                    };
                    break;
                default:
                    throw new ArgumentException("Unrecognized Royalty Type", nameof(royalty));
            }
        }

        internal IRoyalty ToRoyalty()
        {
            return feeCase_ switch
            {
                FeeOneofCase.RoyaltyFee => new AssetRoyalty(this),
                FeeOneofCase.FractionalFee => new TokenRoyalty(this),
                FeeOneofCase.FixedFee => new FixedRoyalty(this),
                // Should not get here?, if its invalid info, what do we do?
                _ => new FixedRoyalty(Address.None, Address.None, 0),
            };
        }
    }
}
