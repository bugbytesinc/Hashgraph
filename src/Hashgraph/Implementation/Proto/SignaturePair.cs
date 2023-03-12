using Hashgraph;
using System;

namespace Proto;

public sealed partial class SignaturePair
{
    internal void AddSignatureToInvoice(IInvoice invoice)
    {
        switch (signatureCase_)
        {
            case SignatureOneofCase.Ed25519:
                invoice.AddSignature(KeyType.Ed25519, PubKeyPrefix.Memory, Ed25519.Memory);
                break;
            case SignatureOneofCase.ECDSASecp256K1:
                invoice.AddSignature(KeyType.ECDSASecp256K1, PubKeyPrefix.Memory, ECDSASecp256K1.Memory);
                break;
            case SignatureOneofCase.Contract:
                invoice.AddSignature(KeyType.Contract, PubKeyPrefix.Memory, Contract.Memory);
                break;
            default:
                throw new ArgumentException($"Unsupported Signing Key Type {signatureCase_}");
        }
    }
}