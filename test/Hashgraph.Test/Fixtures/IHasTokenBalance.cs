namespace Hashgraph.Test.Fixtures;

public interface IHasTokenBalance
{
    public Task<long?> GetTokenBalanceAsync(Address token);
    public Task<TokenHoldingData[]> GetTokenBalancesAsync();
}
