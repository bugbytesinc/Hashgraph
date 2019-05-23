using System;
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
        private static (bool, ReadOnlyMemory<byte>) EncodePart(object arg)
        {
            switch (arg)
            {
                case null: return EncodeNull();
                case int intArg: return EncodeInt256Part(intArg);
                case long longArg: return EncodeInt256Part(longArg);
                case uint intArg: return EncodeUint256Part(intArg);
                case ulong longArg: return EncodeUint256Part(longArg);
                case string stringArg: return EncodeStringPart(stringArg);
                case byte[] bytesArrayArg: return EncodeByteArrayPart(bytesArrayArg);
                case ReadOnlyMemory<byte> bytesMemoryArg: return EncodeByteArrayPart(bytesMemoryArg.Span);
            }
            throw new InvalidOperationException($"Encoding of type {arg.GetType().Name} is not currently supported.");
        }
        private static (bool, ReadOnlyMemory<byte>) EncodeNull()
        {
            return (false, new byte[32]);
        }
        private static (bool, ReadOnlyMemory<byte>) EncodeStringPart(string value)
        {
            return EncodeByteArrayPart(Encoding.UTF8.GetBytes(value));
        }
        private static (bool, ReadOnlyMemory<byte>) EncodeByteArrayPart(ReadOnlySpan<byte> bytes)
        {
            var words = (bytes.Length / 32) + (bytes.Length % 32 > 0 ? 2 : 1);
            var result = new byte[32 * words];
            WriteInt256(result.AsSpan(0, 32), bytes.Length);
            bytes.CopyTo(result.AsSpan(32));
            return (true, result);
        }
        private static (bool, ReadOnlyMemory<byte>) EncodeInt256Part(long value)
        {
            var bytes = new byte[32];
            WriteInt256(bytes.AsSpan(), value);
            return (false, bytes);
        }
        private static (bool, ReadOnlyMemory<byte>) EncodeUint256Part(ulong value)
        {
            var bytes = new byte[32];
            WriteUint256(bytes.AsSpan(), value);
            return (false, bytes);
        }
        private static void WriteInt256(Span<byte> buffer, long value)
        {
            var valueAsBytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(valueAsBytes);
            }
            valueAsBytes.CopyTo(buffer.Slice(32 - valueAsBytes.Length));
        }
        private static void WriteUint256(Span<byte> buffer, ulong value)
        {
            var valueAsBytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(valueAsBytes);
            }
            valueAsBytes.CopyTo(buffer.Slice(32 - valueAsBytes.Length));
        }
    }
}
