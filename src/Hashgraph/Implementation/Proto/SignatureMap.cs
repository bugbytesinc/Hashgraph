using Hashgraph;

namespace Proto;

public sealed partial class SignatureMap
{
    internal int MaxSignaturePrefixLength
    {
        get
        {
            var length = 0;
            foreach (var sig in SigPair)
            {
                if (length < sig.PubKeyPrefix.Length)
                {
                    length = sig.PubKeyPrefix.Length;
                }
            }
            return length;
        }
    }

    internal void AddSignaturesToInvoice(IInvoice invoice)
    {
        foreach (var sig in SigPair)
        {
            sig.AddSignatureToInvoice(invoice);
        }
    }
}