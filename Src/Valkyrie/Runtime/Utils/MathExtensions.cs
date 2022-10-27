using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Utils
{
    public static class MathExtensions
    {
        #region BitInteger

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
        

        #endregion

        #region Vector3

        public static Vector3 Center(this IReadOnlyList<Vector3> points)
        {
            var result = Vector3.zero;
            for (var i = 0; i < points.Count; ++i)
                result += points[i];
            return result / points.Count;
        }

        public static float GetPolyLineLength(this IReadOnlyList<Vector3> points)
        {
            var r = 0f;
            for (var i = 1; i < points.Count; ++i)
                r += (points[i] - points[i - 1]).magnitude;
            return r;
        }

        public static Vector3 GetPolyLinePoint(this IReadOnlyList<Vector3> points, float length)
        {
            if (length < 0)
                return points[0];
            
            for (var i = 1; i < points.Count; ++i)
            {
                var dir = points[i] - points[i - 1];
                var segmentLength = dir.magnitude;
                if(segmentLength <= 0)
                    continue;
                
                if (segmentLength > length)
                    return Vector3.Lerp(points[i - 1], points[i], length / segmentLength);
                length -= segmentLength;
            }

            return points[^1];
        }

        #endregion
    }
}