using Hashgraph;

namespace Proto
{
    public sealed partial class CustomFee
    {
        internal CustomFee(FixedCommission commission) : this()
        {
            FeeCollectorAccountId = new AccountID(commission.Account);
            FixedFee = new FixedFee
            {
                Amount = commission.Amount,
                DenominatingTokenId = commission.Token != Address.None ? new TokenID(commission.Token) : null
            };
        }
        internal CustomFee(VariableCommission commission) : this()
        {
            FeeCollectorAccountId = new AccountID(commission.Account);
            FractionalFee = new FractionalFee
            {
                MinimumAmount = commission.Minimum,
                MaximumAmount = commission.Maximum,
                FractionalAmount = new Fraction
                {
                    Numerator = commission.Numerator,
                    Denominator = commission.Denominator
                }
            };
        }
    }
}
