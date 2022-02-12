using System;
using System.Threading.Tasks;

namespace Hashgraph.Test.Fixtures;

public static class TestHelperExtensions
{
    public static async Task<TRecord> RetryKnownNetworkIssues<TRecord>(this Client client, Func<Client, Task<TRecord>> callback) where TRecord : TransactionRecord
    {
        try
        {
            while (true)
            {
                try
                {
                    return await callback(client).ConfigureAwait(false);
                }
                catch (PrecheckException pex) when (pex.Status == ResponseCode.TransactionExpired || pex.Status == ResponseCode.Busy)
                {
                    continue;
                }
            }
        }
        catch (TransactionException ex) when (ex.Message?.StartsWith("The Network Changed the price of Retrieving a Record while attempting to retrieve this record") == true)
        {
            var record = await client.GetTransactionRecordAsync(ex.TxId) as TRecord;
            if (record is not null)
            {
                return record;
            }
            else
            {
                throw;
            }
        }
    }

    public static string TruncateMemo(this string memo)
    {
        if (memo is not null && memo.Length > 100)
        {
            return memo.Substring(0, 100);
        }
        return memo;
    }
}