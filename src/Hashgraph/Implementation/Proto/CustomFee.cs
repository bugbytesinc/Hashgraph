using Hashgraph;
using System;

namespace Proto
{
    public sealed partial class CustomFee
    {
        internal CustomFee(ICommission commission) : this()
        {
            FeeCollectorAccountId = new AccountID(commission.Account);
            switch (commission.CommissionType)
            {
                case CommissionType.Fixed:
                    var fixedCommission = commission as FixedCommission;
                    if (fixedCommission is null)
                    {
                        throw new ArgumentException("Commission had type of Fixed but was not a Fixed Commission", nameof(commission));
                    }
                    FixedFee = new FixedFee
                    {
                        Amount = fixedCommission.Amount,
                        DenominatingTokenId = fixedCommission.Token != Address.None ? new TokenID(fixedCommission.Token) : null
                    };
                    break;
                case CommissionType.Value:
                    var valueCommission = commission as ValueCommission;
                    if (valueCommission is null)
                    {
                        throw new ArgumentException("Commission had type of Value (Royalty) but was not a Value Commission", nameof(commission));
                    }
                    RoyaltyFee = new RoyaltyFee
                    {
                        ExchangeValueFraction = new Fraction
                        {
                            Numerator = valueCommission.Numerator,
                            Denominator = valueCommission.Denominator
                        }
                    };
                    if (valueCommission.FallbackAmount > 0 || !valueCommission.FallbackToken.IsNullOrNone())
                    {
                        RoyaltyFee.FallbackFee = new FixedFee
                        {
                            Amount = valueCommission.FallbackAmount,
                            DenominatingTokenId = valueCommission.FallbackToken != Address.None ? new TokenID(valueCommission.FallbackToken) : null
                        };
                    }

                    break;
                case CommissionType.Fractional:
                    var fractionalCommission = commission as FractionalCommission;
                    if (fractionalCommission is null)
                    {
                        throw new ArgumentException("Commission had type of Fractional but was not a Fractional Commission", nameof(commission));
                    }
                    FractionalFee = new FractionalFee
                    {
                        MinimumAmount = fractionalCommission.Minimum,
                        MaximumAmount = fractionalCommission.Maximum,
                        FractionalAmount = new Fraction
                        {
                            Numerator = fractionalCommission.Numerator,
                            Denominator = fractionalCommission.Denominator
                        }
                    };
                    break;
                default:
                    throw new ArgumentException("Unrecognized Commission Type", nameof(commission));
            }
        }

        internal ICommission ToCommission()
        {
            return feeCase_ switch
            {
                FeeOneofCase.RoyaltyFee => new ValueCommission(this),
                FeeOneofCase.FractionalFee => new FractionalCommission(this),
                FeeOneofCase.FixedFee => new FixedCommission(this),
                // Should not get here?, if its invalid info, what do we do?
                _ => new FixedCommission(Address.None, Address.None, 0),
            };
        }
    }
}
