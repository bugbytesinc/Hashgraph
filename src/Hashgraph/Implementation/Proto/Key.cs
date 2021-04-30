using Google.Protobuf;
using Hashgraph;
using Hashgraph.Implementation;
using Org.BouncyCastle.Crypto.Parameters;
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
                    Ed25519 = ByteString.CopyFrom(((Ed25519PublicKeyParameters)endorsement._data).GetEncoded());
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
            return KeyCase switch
            {
                KeyOneofCase.Ed25519 => new Endorsement(KeyType.Ed25519, new ReadOnlyMemory<byte>(Ed25519Util.publicKeyPrefix.Concat(Ed25519.ToByteArray()).ToArray())),
                KeyOneofCase.RSA3072 => new Endorsement(KeyType.RSA3072, RSA3072.ToByteArray()),
                KeyOneofCase.ECDSA384 => new Endorsement(KeyType.ECDSA384, ECDSA384.ToByteArray()),
                KeyOneofCase.ContractID => new Endorsement(KeyType.Contract, Abi.EncodeAddressPart(ContractID.AsAddress())),
                KeyOneofCase.ThresholdKey => ThresholdKey.Keys.Keys.Count == 0 ? Endorsement.None : new Endorsement(ThresholdKey.Threshold, ThresholdKey.Keys.ToEndorsements()),
                KeyOneofCase.KeyList => KeyList.Keys.Count == 0 ? Endorsement.None : new Endorsement(KeyList.ToEndorsements()),
                _ => throw new InvalidOperationException($"Unknown Key Type {KeyCase}.  Do we have a network/library version mismatch?"),
            };
        }
    }
}
