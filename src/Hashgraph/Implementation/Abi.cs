using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hashgraph.Implementation
{
    /// <summary>
    /// Internal Helper class for ABI encoding
    /// of parameter arguments sent to smart 
    /// contract methods.
    /// </summary>
    internal static class Abi
    {
        internal static ReadOnlyMemory<byte> EncodeFunctionWithArguments(string functionName, object[] functionArgs)
        {
            var selector = GetFunctionSelector(functionName, functionArgs);
            var arguments = EncodeArguments(functionArgs);
            var result = new byte[selector.Length + arguments.Length];
            selector.CopyTo(result.AsMemory());
            arguments.CopyTo(result.AsMemory(selector.Length));
            return result;
        }
        internal static ReadOnlyMemory<byte> EncodeArguments(object[] args)
        {
            if (args is null || args.Length == 0)
            {
                return ReadOnlyMemory<byte>.Empty;
            }
            var headerSize = 0;
            var totalSize = 0;
            var argsCount = args.Length;
            var parts = new (bool isDynamic, ReadOnlyMemory<byte> bytes)[argsCount];
            for (int i = 0; i < args.Length; i++)
            {
                var (isDynamic, bytes) = parts[i] = EncodePart(args[i]);
                if (isDynamic)
                {
                    headerSize += 32;
                    totalSize += 32 + bytes.Length;
                }
                else
                {
                    headerSize += bytes.Length;
                    totalSize += bytes.Length;
                }
            }
            var result = new byte[totalSize];
            var headerPtr = 0;
            var dataPtr = headerSize;
            for (int i = 0; i < argsCount; i++)
            {
                var (isDynamic, bytes) = parts[i];
                if (isDynamic)
                {
                    WriteInt256(result.AsSpan(headerPtr), dataPtr);
                    bytes.CopyTo(result.AsMemory(dataPtr));
                    headerPtr += 32;
                    dataPtr += bytes.Length;
                }
                else
                {
                    bytes.CopyTo(result.AsMemory(headerPtr));
                    headerPtr += bytes.Length;
                }
            }
            return result;
        }
        internal static object[] DecodeArguments(ReadOnlyMemory<byte> data, params Type[] types)
        {
            if (types is null || types.Length == 0)
            {
                return new object[0];
            }
            var results = new object[types.Length];
            var headerPtr = 0;
            for (int i = 0; i < types.Length; i++)
            {
                var typeMapping = GetMapping(types[i]);
                if (typeMapping.IsDynamic)
                {
                    var positionPtr = (int)ReadUint256(data.Slice(headerPtr));
                    results[i] = typeMapping.Decode(data.Slice(positionPtr));
                    headerPtr += typeMapping.HeaderSize;
                }
                else
                {
                    results[i] = typeMapping.Decode(data.Slice(headerPtr));
                    headerPtr += typeMapping.HeaderSize;
                }
            }
            return results;
        }
        private static (bool isDynamic, ReadOnlyMemory<byte> bytes) EncodePart(object value)
        {
            var mapping = GetMapping(value);
            return (mapping.IsDynamic, mapping.Encode(value));
        }
        private static ReadOnlyMemory<byte> GetFunctionSelector(string functionName, object[] functionArgs)
        {
            var buffer = new StringBuilder(100);
            buffer.Append(functionName);
            buffer.Append('(');
            if (functionArgs != null && functionArgs.Length > 0)
            {
                buffer.Append(GetMapping(functionArgs[0]).AbiCode);
                for (int i = 1; i < functionArgs.Length; i++)
                {
                    buffer.Append(',');
                    buffer.Append(GetMapping(functionArgs[i]).AbiCode);
                }
            }
            buffer.Append(')');
            var bytes = Encoding.ASCII.GetBytes(buffer.ToString());
            var digest = new KeccakDigest(256);
            digest.BlockUpdate(bytes, 0, bytes.Length);
            var hash = new byte[digest.GetByteLength()];
            digest.DoFinal(hash, 0);
            return hash.AsMemory(0, 4);
        }
        private static ReadOnlyMemory<byte> EncodeStringPart(object value)
        {
            #nullable disable
            return EncodeByteArrayPart(Encoding.UTF8.GetBytes(Convert.ToString(value)));
            #nullable enable
        }
        private static object DecodeStringPart(ReadOnlyMemory<byte> arg)
        {
            return Encoding.UTF8.GetString((byte[])DecodeByteArrayPart(arg));
        }
        private static ReadOnlyMemory<byte> EncodeByteArrayPart(object value)
        {
            var bytes = (byte[])value;
            var words = (bytes.Length / 32) + (bytes.Length % 32 > 0 ? 2 : 1);
            var result = new byte[32 * words];
            WriteInt256(result.AsSpan(0, 32), bytes.Length);
            bytes.CopyTo(result.AsSpan(32));
            return result;
        }
        private static object DecodeByteArrayPart(ReadOnlyMemory<byte> arg)
        {
            var size = (int)ReadInt256(arg.Slice(0, 32));
            return arg.Slice(32, size).ToArray();
        }
        private static ReadOnlyMemory<byte> EncodeReadOnlyMemoryPart(object value)
        {
            var bytes = (ReadOnlyMemory<byte>)value;
            var words = (bytes.Length / 32) + (bytes.Length % 32 > 0 ? 2 : 1);
            var result = new byte[32 * words];
            WriteInt256(result.AsSpan(0, 32), bytes.Length);
            bytes.CopyTo(result.AsMemory(32));
            return result;
        }
        private static object DecodeReadOnlyMemoryPart(ReadOnlyMemory<byte> arg)
        {
            var size = (int)ReadInt256(arg.Slice(0, 32));
            return arg.Slice(32, size);
        }
        private static ReadOnlyMemory<byte> EncodeInt32Part(object value)
        {
            var bytes = new byte[32];
            WriteInt256(bytes.AsSpan(), Convert.ToInt32(value));
            return bytes;
        }
        private static object DecodeInt32Part(ReadOnlyMemory<byte> arg)
        {
            return (int)ReadInt256(arg.Slice(0, 32));
        }
        private static ReadOnlyMemory<byte> EncodeInt64Part(object value)
        {
            var bytes = new byte[32];
            WriteInt256(bytes.AsSpan(), Convert.ToInt64(value));
            return bytes;
        }
        private static object DecodeInt64Part(ReadOnlyMemory<byte> arg)
        {
            return ReadInt256(arg.Slice(0, 32));
        }
        private static ReadOnlyMemory<byte> EncodeUInt32Part(object value)
        {
            var bytes = new byte[32];
            WriteUint256(bytes.AsSpan(), Convert.ToUInt32(value));
            return bytes;
        }
        private static object DecodeUInt32Part(ReadOnlyMemory<byte> arg)
        {
            return (uint)ReadUint256(arg.Slice(0, 32));
        }
        private static ReadOnlyMemory<byte> EncodeUInt64Part(object value)
        {
            var bytes = new byte[32];
            WriteUint256(bytes.AsSpan(), Convert.ToUInt64(value));
            return bytes;
        }
        private static object DecodeUInt64Part(ReadOnlyMemory<byte> arg)
        {
            return ReadUint256(arg.Slice(0, 32));
        }
        private static ReadOnlyMemory<byte> EncodeBoolPart(object value)
        {
            var bytes = new byte[32];
            WriteInt256(bytes.AsSpan(), Convert.ToBoolean(value) ? 1 : 0);
            return bytes;
        }
        private static object DecodeBoolPart(ReadOnlyMemory<byte> arg)
        {
            return ReadInt256(arg.Slice(0, 32)) > 0;
        }
        private static ReadOnlyMemory<byte> EncodeAddressPart(object value)
        {
            // For 20 bytes total (aka uint160)
            // byte 0 to 3 are shard
            // byte 4 to 11 are realm
            // byte 12 to 19 are account number
            // Note: packed in 32 bytes, right aligned
            if (value is Address address)
            {
                var bytes = new byte[32];
                var shard = BitConverter.GetBytes(address.ShardNum);
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(shard);
                }
                shard[^4..^0].CopyTo(bytes, 12);
                var realm = BitConverter.GetBytes(address.RealmNum);
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(realm);
                }
                realm.CopyTo(bytes, 16);
                var num = BitConverter.GetBytes(address.AccountNum);
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(num);
                }
                num.CopyTo(bytes, 24);
                return bytes;
            }
            throw new ArgumentException("Argument was not an address.", nameof(value));
        }
        private static object DecodeAddressPart(ReadOnlyMemory<byte> arg)
        {
            // See EncodeAddressPart for packing notes
            var shardAsBytes = arg.Slice(12, 4).ToArray();
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(shardAsBytes);
            }
            var shard = BitConverter.ToInt32(shardAsBytes);

            var realmAsBytes = arg.Slice(16, 8).ToArray();
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(realmAsBytes);
            }
            var realm = BitConverter.ToInt64(realmAsBytes);

            var numAsBytes = arg.Slice(24, 8).ToArray();
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(numAsBytes);
            }
            var num = BitConverter.ToInt64(numAsBytes);

            return new Address(shard, realm, num);
        }
        private static void WriteInt256(Span<byte> buffer, long value)
        {
            var valueAsBytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(valueAsBytes);
            }
            valueAsBytes.CopyTo(buffer.Slice(24));
        }
        private static long ReadInt256(ReadOnlyMemory<byte> buffer)
        {
            var valueAsBytes = buffer.Slice(24, 8).ToArray();
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(valueAsBytes);
            }
            return BitConverter.ToInt64(valueAsBytes);
        }
        private static void WriteUint256(Span<byte> buffer, ulong value)
        {
            var valueAsBytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(valueAsBytes);
            }
            valueAsBytes.CopyTo(buffer.Slice(24));
        }
        private static ulong ReadUint256(ReadOnlyMemory<byte> buffer)
        {
            var valueAsBytes = buffer.Slice(24, 8).ToArray();
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(valueAsBytes);
            }
            return BitConverter.ToUInt64(valueAsBytes);
        }
        private static TypeMapping GetMapping(object value)
        {
            if (value is null)
            {
                return _typeMap[typeof(int)];
            }
            return GetMapping(value.GetType());
        }
        private static TypeMapping GetMapping(Type type)
        {
            #nullable disable
            if (_typeMap.TryGetValue(type, out TypeMapping mapping))
            {
                return mapping;
            }
            throw new InvalidOperationException($"Encoding of type {type.Name} is not currently supported.");
            #nullable enable
        }
        private static readonly Dictionary<Type, TypeMapping> _typeMap;
        static Abi()
        {
            _typeMap = new Dictionary<Type, TypeMapping>();
            _typeMap.Add(typeof(bool), new TypeMapping("bool", false, 32, EncodeBoolPart, DecodeBoolPart));
            _typeMap.Add(typeof(int), new TypeMapping("int32", false, 32, EncodeInt32Part, DecodeInt32Part));
            _typeMap.Add(typeof(long), new TypeMapping("int64", false, 32, EncodeInt64Part, DecodeInt64Part));
            _typeMap.Add(typeof(uint), new TypeMapping("uint32", false, 32, EncodeUInt32Part, DecodeUInt32Part));
            _typeMap.Add(typeof(ulong), new TypeMapping("uint64", false, 32, EncodeUInt64Part, DecodeUInt64Part));
            _typeMap.Add(typeof(string), new TypeMapping("string", true, 32, EncodeStringPart, DecodeStringPart));
            _typeMap.Add(typeof(byte[]), new TypeMapping("bytes", true, 32, EncodeByteArrayPart, DecodeByteArrayPart));
            _typeMap.Add(typeof(ReadOnlyMemory<byte>), new TypeMapping("bytes", true, 32, EncodeReadOnlyMemoryPart, DecodeReadOnlyMemoryPart));
            _typeMap.Add(typeof(Address), new TypeMapping("address", false, 32, EncodeAddressPart, DecodeAddressPart));
        }
        internal class TypeMapping
        {
            internal readonly string AbiCode;
            internal readonly bool IsDynamic;
            internal readonly int HeaderSize;
            internal readonly Func<object, ReadOnlyMemory<byte>> Encode;
            internal readonly Func<ReadOnlyMemory<byte>, object> Decode;
            public TypeMapping(string abiCode, bool isDynamic, int headerSize, Func<object, ReadOnlyMemory<byte>> encode, Func<ReadOnlyMemory<byte>, object> decode)
            {
                AbiCode = abiCode;
                IsDynamic = isDynamic;
                HeaderSize = headerSize;
                Encode = encode;
                Decode = decode;
            }
        }
    }
}
