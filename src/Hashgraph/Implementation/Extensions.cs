using Google.Protobuf;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Hashgraph.Implementation
{
    /// <summary>
    /// Internal Helper Convenience Extensions
    /// </summary>
    internal static class Extensions
    {
        internal static ByteString ToByteString(this ReadOnlyMemory<byte> data)
        {
            return ByteString.CopyFrom(data.ToArray());
        }

        internal static bool IsNullOrNone([AllowNull] this Address address)
        {
            return address is null || address == Address.None;
        }
    }
}
