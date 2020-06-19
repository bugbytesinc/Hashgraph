using Google.Protobuf;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Linq;

namespace Proto
{
    public sealed partial class Key
    {
        internal Key(Endorsement endorsement) : this()
        {
            switch (endorsement.Type)
            {
                case KeyType.Ed25519:
                    Ed25519 = ByteString.CopyFrom(((NSec.Cryptography.PublicKey)endorsement._data).Export(NSec.Cryptography.KeyBlobFormat.PkixPublicKey).TakeLast(32).ToArray());
                    break;
                case KeyType.RSA3072:
                    RSA3072 = ByteString.CopyFrom(((ReadOnlyMemory<byte>)endorsement._data).ToArray());
                    break;
                case KeyType.ECDSA384:
                    ECDSA384 = ByteString.CopyFrom(((ReadOnlyMemory<byte>)endorsement._data).ToArray());
                    break;
                case KeyType.Contract:
                    ContractID = new ContractID((Address)Abi.DecodeAddressPart((ReadOnlyMemory<byte>)endorsement._data));
                    break;
                case KeyType.List:
                    ThresholdKey = new ThresholdKey
                    {
                        Threshold = endorsement.RequiredCount,
                        Keys = new KeyList((Endorsement[])endorsement._data)
                    };
                    break;
                default:
                    throw new InvalidOperationException("Endorsement is Empty.");
            }
        }
        internal Endorsement ToEndorsement()
        {
            switch (KeyCase)
            {
                case KeyOneofCase.Ed25519: return new Endorsement(KeyType.Ed25519, new ReadOnlyMemory<byte>(Keys.publicKeyPrefix.Concat(Ed25519.ToByteArray()).ToArray()));
                case KeyOneofCase.RSA3072: return new Endorsement(KeyType.RSA3072, RSA3072.ToByteArray());
                case KeyOneofCase.ECDSA384: return new Endorsement(KeyType.ECDSA384, ECDSA384.ToByteArray());
                case KeyOneofCase.ContractID: return new Endorsement(KeyType.Contract, Abi.EncodeAddressPart(ContractID.ToAddress()));
                case KeyOneofCase.ThresholdKey:
                    return ThresholdKey.Keys.Keys.Count == 0 ?
                        Endorsement.None :
                        new Endorsement(ThresholdKey.Threshold, ThresholdKey.Keys.ToEndorsements());
                case KeyOneofCase.KeyList:
                    return
                        KeyList.Keys.Count == 0 ?
                        Endorsement.None :
                        new Endorsement(KeyList.ToEndorsements());
            }
            throw new InvalidOperationException($"Unknown Key Type {KeyCase}.  Do we have a network/library version mismatch?");
        }
    }
}
