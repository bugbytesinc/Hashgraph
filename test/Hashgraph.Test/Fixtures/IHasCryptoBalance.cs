namespace Hashgraph.Test.Fixtures;

public interface IHasCryptoBalance
{
    public Task<ulong> GetCryptoBalanceAsync();
}
