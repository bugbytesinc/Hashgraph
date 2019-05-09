using System;
using System.Text;

namespace Hashgraph
{
    /// <summary>
    /// Helper class for converting between bytes and Hex encoded string values.
    /// </summary>
    public static class Hex
    {
        /// <summary>
        /// Converts string values encoded in Hex into bytes.
        /// </summary>
        /// <param name="hex">
        /// A string containing a series of characters in hexadecimal format.
        /// </param>
        /// <returns>
        /// A blob of bytes decoded from the hex string.
        /// </returns>
        /// <exception cref="ArgumentNullException">If the input string is <code>null</code>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If the string of characters is not valid Hex.</exception>
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
        /// <summary>
        /// Converts a blob of bytes into the corresponding hex encoded string.
        /// </summary>
        /// <param name="bytes">
        /// Blob of bytes to turn into Hex.
        /// </param>
        /// <returns>
        /// String value of the bytes in Hex.
        /// </returns>
        public static string FromBytes(ReadOnlyMemory<byte> bytes)
        {
            var size = bytes.Length * 2;
            if (size == 0)
            {
                return string.Empty;
            }
            var buff = new StringBuilder(size, size);
            foreach (var b in bytes.Span)
            {
                buff.AppendFormat("{0:x2}", b);
            }
            return buff.ToString();
        }
    }
}
