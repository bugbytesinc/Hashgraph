using System;
using System.Text;

namespace Hashgraph
{
    public static class Hex
    {
        public static ReadOnlyMemory<byte> ToBytes(string hex)
        {
            if (hex == null)
            {
                throw new ArgumentNullException(nameof(hex), "Hex string value cannot be null.");
            }
            else if (hex.Length == 0)
            {
                return ReadOnlyMemory<byte>.Empty;
            }
            else if (String.IsNullOrWhiteSpace(hex))
            {
                throw new ArgumentOutOfRangeException(nameof(hex), "Hex string value does not contain hex values.");
            }
            else if (hex.Length % 2 != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(hex), "String value does not appear to be properly encoded in Hex, found an odd number of characters.");
            }
            try
            {
                byte[] result = new byte[hex.Length / 2];
                for (int i = 0; i < hex.Length; i += 2)
                {
                    result[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
                }
                return result;
            }
            catch (FormatException fe)
            {
                throw new ArgumentOutOfRangeException("String value does not appear to be properly encoded in Hex.", fe);
            }
        }
        public static string FromBytes(ReadOnlyMemory<byte> bytes)
        {
            var size = bytes.Length * 2;
            if (size == 0)
            {
                return string.Empty;
            }
            var buff = new StringBuilder(size, size);
            foreach(var b in bytes.Span)
            {
                buff.AppendFormat("{0:x2}", b);
            }
            return buff.ToString();
        }
    }
}
