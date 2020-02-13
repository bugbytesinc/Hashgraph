using Google.Protobuf;
using Proto;
using System;
using System.Collections.Generic;

namespace Hashgraph.Implementation
{
    internal sealed class Invoice : IInvoice
    {
        private readonly TxId _txId;
        private readonly string _memo;
        private readonly ReadOnlyMemory<byte> _txBytes;
        private readonly Dictionary<ByteString, SignaturePair> _signatures;

        TxId IInvoice.TxId { get { return _txId; } }
        string IInvoice.Memo { get { return _memo; } }
        ReadOnlyMemory<byte> IInvoice.TxBytes { get { return _txBytes; } }
        internal Invoice(TransactionBody transactionBody)
        {
            _txId = Protobuf.FromTransactionId(transactionBody.TransactionID);
            _memo = transactionBody.Memo;
            _txBytes = transactionBody.ToByteArray();
            _signatures = new Dictionary<ByteString, SignaturePair>();
        }
        void IInvoice.AddSignature(KeyType type, ReadOnlyMemory<byte> publicPrefix, ReadOnlyMemory<byte> signature)
        {
            var key = ByteString.CopyFrom(publicPrefix.Span);
            var value = ByteString.CopyFrom(signature.Span);
            var pair = new Proto.SignaturePair { PubKeyPrefix = key };
            switch (type)
            {
                case KeyType.Ed25519:
                    pair.Ed25519 = value;
                    break;
                case KeyType.ECDSA384:
                    pair.ECDSA384 = value;
                    break;
                case KeyType.RSA3072:
                    pair.RSA3072 = value;
                    break;
            }
            _signatures[key] = pair;
        }
        internal Transaction GetSignedTransaction()
        {
            if (_signatures.Count == 0)
            {
                throw new InvalidOperationException("A transaction or query requires at least one signature, sometimes more.  None were found, did you forget to assign a Signatory to the context, transaction or query?");
            }
            var signatures = new SignatureMap();
            foreach (var signature in _signatures.Values)
            {
                signatures.SigPair.Add(signature);
            }
            return new Transaction
            {
                BodyBytes = ByteString.CopyFrom(_txBytes.Span),
                SigMap = signatures
            };
        }
    }
}

