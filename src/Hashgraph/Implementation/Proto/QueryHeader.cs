#pragma warning disable CS0612
using System;

namespace Proto
{
    public sealed partial class QueryHeader
    {
        // Note, this is an expensive operation, only use
        // when you're throwing an exception and this is the
        // only way you can dig out the transaction ID to
        // create the exception.
        internal TransactionID? getTransactionId()
        {
            var signedBytes = Payment?.SignedTransactionBytes;
            if(signedBytes is not null)
            {
                var bodyBytes = SignedTransaction.Parser.ParseFrom(signedBytes).BodyBytes;
                if(bodyBytes is not null)
                {
                    return TransactionBody.Parser.ParseFrom(bodyBytes)?.TransactionID;
                }                
            }
            return null;
        }
    }
}
