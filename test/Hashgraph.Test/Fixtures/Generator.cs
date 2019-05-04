using System;
using System.Text;

namespace Hashgraph.Test.Fixtures
{
    public static class Generator
    {
        private static Random _random = new Random();
        private static char[] _sample = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-*&%$#@!".ToCharArray();

        public static Int32 Integer(Int32 minValueInclusive, Int32 maxValueInclusive)
        {
            return _random.Next(minValueInclusive, maxValueInclusive + 1);
        }

        public static Double Double(Double minValueInclusive, Double maxValueInclusive)
        {
            return (_random.NextDouble() * (maxValueInclusive - minValueInclusive)) + minValueInclusive;
        }

        public static String String(Int32 minLengthInclusive, Int32 maxLengthInclusive)
        {
            return Code(Integer(minLengthInclusive, maxLengthInclusive));
        }

        public static String Code(Int32 length)
        {
            var buffer = new char[length];
            for (int i = 0; i < length; i++)
            {
                buffer[i] = _sample[_random.Next(0, _sample.Length)];
            }
            return new string(buffer);
        }

        public static String[] ArrayOfStrings(Int32 minCount, Int32 maxCount, Int32 minLength, Int32 maxLength)
        {
            String[] result = new String[Integer(minCount, maxCount)];
            for (Int32 index = 0; index < result.Length; index++)
            {
                result[index] = String(minLength, maxLength);
            }
            return result;
        }
    }
}
