using System.Numerics;

namespace Utils
{
    public static class MathExtensions
    {
        public static BigInteger Pow(this BigInteger f, BigInteger p)
        {
            if (p < 1)
                return 1;
            var r = (BigInteger)1;
            for (var i = 0; i < p; ++i)
                r *= f;
            return r;
        }
        
        public static decimal Pow(this decimal f, BigInteger p)
        {
            if (p < 1)
                return 1;
            var r = (decimal)1;
            for (var i = 0; i < p; ++i)
                r *= f;
            return r;
        }
    }
}