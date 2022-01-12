using Hashgraph.Test.Fixtures;
using Xunit;

namespace Hashgraph.Tests;

public class ExchangeRateTests
{
    [Fact(DisplayName = "Exchange Rate: Equivalent Exchange Ratees are considered Equal")]
    public void EquivalentExchangeRateAreConsideredEqual()
    {
        var hBarEquivalent = Generator.Integer(0, 200);
        var usdCentEquivalent = Generator.Integer(0, 200);
        var expiration = Generator.TruncatedFutureDate(1, 500);
        var ExchangeRate1 = new ExchangeRate { HBarEquivalent = hBarEquivalent, USDCentEquivalent = usdCentEquivalent, Expiration = expiration };
        var ExchangeRate2 = new ExchangeRate { HBarEquivalent = hBarEquivalent, USDCentEquivalent = usdCentEquivalent, Expiration = expiration };
        Assert.Equal(ExchangeRate1, ExchangeRate2);
        Assert.True(ExchangeRate1 == ExchangeRate2);
        Assert.False(ExchangeRate1 != ExchangeRate2);
    }
    [Fact(DisplayName = "Exchange Rate: Disimilar Exchange Ratees are not considered Equal")]
    public void DisimilarExchangeRatesAreNotConsideredEqual()
    {
        var hBarEquivalent = Generator.Integer(0, 200);
        var usdCentEquivalent = Generator.Integer(0, 200);
        var expiration = Generator.TruncatedFutureDate(1, 500);
        var ExchangeRate1 = new ExchangeRate { HBarEquivalent = hBarEquivalent, USDCentEquivalent = usdCentEquivalent, Expiration = expiration };
        Assert.NotEqual(ExchangeRate1, new ExchangeRate { HBarEquivalent = hBarEquivalent + 1, USDCentEquivalent = usdCentEquivalent, Expiration = expiration });
        Assert.NotEqual(ExchangeRate1, new ExchangeRate { HBarEquivalent = hBarEquivalent, USDCentEquivalent = usdCentEquivalent + 1, Expiration = expiration });
        Assert.NotEqual(ExchangeRate1, new ExchangeRate { HBarEquivalent = hBarEquivalent, USDCentEquivalent = usdCentEquivalent, Expiration = expiration.AddMinutes(10) });
        Assert.False(ExchangeRate1 == new ExchangeRate { HBarEquivalent = hBarEquivalent, USDCentEquivalent = usdCentEquivalent, Expiration = expiration.AddMinutes(10) });
        Assert.True(ExchangeRate1 != new ExchangeRate { HBarEquivalent = hBarEquivalent, USDCentEquivalent = usdCentEquivalent, Expiration = expiration.AddMinutes(10) });
    }
}