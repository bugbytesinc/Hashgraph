using Google.Protobuf;
using Hashgraph.Implementation;
using System.Threading.Tasks;

namespace Proto
{
    public sealed partial class TransactionBody
    {
        internal async Task<Transaction> SignAsync(ISignatory signatory, int prefixTrimLimit)
        {
            var invoice = new Invoice(this);
            await signatory.SignAsync(invoice);
            return new Transaction
            {
                SignedTransactionBytes = invoice.GetSignedTransaction(prefixTrimLimit).ToByteString()
            };
        }
    }
}

