namespace Proto;

public sealed partial class ExchangeRate
{
    internal Hashgraph.ExchangeRate ToExchangeRate()
    {
        return new Hashgraph.ExchangeRate
        {
            HBarEquivalent = HbarEquiv,
            USDCentEquivalent = CentEquiv,
            Expiration = ExpirationTime.ToDateTime()
        };
    }
}