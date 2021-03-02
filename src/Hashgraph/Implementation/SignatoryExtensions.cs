using System.Collections.Generic;
using System.Linq;

namespace Hashgraph.Implementation
{
    internal static class SignatoryExtensions
    {
        internal static ISignatory? Consolidate(this Signatory?[] list)
        {
            if (list is not null)
            {
                var signatories = list.Where(s => s is not null).ToArray();
                return signatories.Length switch
                {
                    0 => null,
                    1 => signatories[0],
                    _ => new Signatory(signatories!)
                };
            }
            return null;
        }
    }
}
