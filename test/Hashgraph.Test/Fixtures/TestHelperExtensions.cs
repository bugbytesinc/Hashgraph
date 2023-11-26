using System;
using System.Threading.Tasks;

namespace Hashgraph.Test.Fixtures;

public static class TestHelperExtensions
{
    public static string TruncateMemo(this string memo)
    {
        if (memo is not null && memo.Length > 100)
        {
            return memo.Substring(0, 100);
        }
        return memo;
    }
}