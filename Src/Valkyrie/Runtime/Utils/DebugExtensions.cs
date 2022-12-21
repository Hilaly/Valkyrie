using System.Collections.Generic;
using UnityEngine;

namespace Valkyrie.Utils
{
    public static class DebugExtensions
    {
        private static readonly Vector4[] s_UnitSphere = MakeUnitSphere(16);
        // Square with edge of length 1
        private static readonly Vector4[] s_UnitSquare =
        {
            new Vector4(-0.5f, 0.5f, 0, 1),
            new Vector4(0.5f, 0.5f, 0, 1),
            new Vector4(0.5f, -0.5f, 0, 1),
            new Vector4(-0.5f, -0.5f, 0, 1),
        };
        // Cube with edge of length 1
        private static readonly Vector4[] s_UnitCube =
        {
            new Vector4(-0.5f,  0.5f, -0.5f, 1),
            new Vector4(0.5f,  0.5f, -0.5f, 1),
            new Vector4(0.5f, -0.5f, -0.5f, 1),
            new Vector4(-0.5f, -0.5f, -0.5f, 1),

            new Vector4(-0.5f,  0.5f,  0.5f, 1),
            new Vector4(0.5f,  0.5f,  0.5f, 1),
            new Vector4(0.5f, -0.5f,  0.5f, 1),
            new Vector4(-0.5f, -0.5f,  0.5f, 1)
        };

        private static Vector4[] MakeUnitSphere(int len)
        {
            Debug.Assert(len > 2);
            var v = new Vector4[len * 3];
            for (int i = 0; i < len; i++)
            {
                var f = i / (float)len;
                float c = Mathf.Cos(f * (float)(Mathf.PI * 2.0));
                float s = Mathf.Sin(f * (float)(Mathf.PI * 2.0));
                v[0 * len + i] = new Vector4(c, s, 0, 1);
                v[1 * len + i] = new Vector4(0, c, s, 1);
                v[2 * len + i] = new Vector4(s, 0, c, 1);
            }

            return v;
        }

        public static void DrawArrow(Vector3 startPoint, Vector3 endPoint, Color color)
        {
            const float Perp = 90f;
            const float angle = 40f;
            const float height = 0.4f;
            var dir = (startPoint - endPoint).normalized;
            var r = new List<Vector3>
            {
                startPoint + Quaternion.AngleAxis(Perp, Vector3.up) * dir * 0.5f * height,
                startPoint + Quaternion.AngleAxis(Perp, Vector3.up) * dir * 0.5f * height + endPoint - startPoint + dir * Mathf.Cos(angle * Mathf.Deg2Rad),
                endPoint + Quaternion.AngleAxis(angle, Vector3.up) * dir,
                endPoint,
                endPoint + Quaternion.AngleAxis(-angle, Vector3.up) * dir,
                startPoint + Quaternion.AngleAxis(-Perp, Vector3.up) * dir * 0.5f * height + endPoint - startPoint + dir * Mathf.Cos(angle * Mathf.Deg2Rad),
                startPoint + Quaternion.AngleAxis(-Perp, Vector3.up) * dir * 0.5f * height
            };
            DrawPolyLine(r, color, false);
        }

        public static void DrawPolyLine(this IReadOnlyList<Vector3> points, Color color, bool drawPoints = false)
        {
            if (points.Count <= 0)
                return;
            
            var p = points[0];
            for (var i = 1; i < points.Count; ++i)
            {
                var n = points[i];
                Debug.DrawLine(p, n, color);
                p = n;
            }

            if (!drawPoints) 
                return;
            
            foreach (var n in points)
                DrawPoint(n, 0.1f, color);
        }

        public static void DrawSphere(Vector4 pos, float radius, Color color)
        {
            Vector4[] v = s_UnitSphere;
            int len = s_UnitSphere.Length / 3;
            for (int i = 0; i < len; i++)
            {
                var sX = pos + radius * v[0 * len + i];
                var eX = pos + radius * v[0 * len + (i + 1) % len];
                var sY = pos + radius * v[1 * len + i];
                var eY = pos + radius * v[1 * len + (i + 1) % len];
                var sZ = pos + radius * v[2 * len + i];
                var eZ = pos + radius * v[2 * len + (i + 1) % len];
                Debug.DrawLine(sX, eX, color);
                Debug.DrawLine(sY, eY, color);
                Debug.DrawLine(sZ, eZ, color);
            }
        }
        public static void DrawBox(Vector4 pos, Vector3 size, Color color)
        {
            Vector4[] v = s_UnitCube;
            Vector4 sz = new Vector4(size.x, size.y, size.z, 1);
            for (int i = 0; i < 4; i++)
            {
                var s = pos + Vector4.Scale(v[i], sz);
                var e = pos + Vector4.Scale(v[(i + 1) % 4], sz);
                Debug.DrawLine(s , e , color);
            }
            for (int i = 0; i < 4; i++)
            {
                var s = pos + Vector4.Scale(v[4 + i], sz);
                var e = pos + Vector4.Scale(v[4 + ((i + 1) % 4)], sz);
                Debug.DrawLine(s , e , color);
            }
            for (int i = 0; i < 4; i++)
            {
                var s = pos + Vector4.Scale(v[i], sz);
                var e = pos + Vector4.Scale(v[i + 4], sz);
                Debug.DrawLine(s , e , color);
            }
        }

        public static void DrawBox(Matrix4x4 transform, Color color)
        {
            Vector4[] v = s_UnitCube;
            Matrix4x4 m = transform;
            for (int i = 0; i < 4; i++)
            {
                var s = m * v[i];
                var e = m * v[(i + 1) % 4];
                Debug.DrawLine(s , e , color);
            }
            for (int i = 0; i < 4; i++)
            {
                var s = m * v[4 + i];
                var e = m * v[4 + ((i + 1) % 4)];
                Debug.DrawLine(s , e , color);
            }
            for (int i = 0; i < 4; i++)
            {
                var s = m * v[i];
                var e = m * v[i + 4];
                Debug.DrawLine(s , e , color);
            }
        }
        
        public static void DrawPoint(Vector4 pos, float scale, Color color)
        {
            var sX = pos + new Vector4(+scale, 0, 0);
            var eX = pos + new Vector4(-scale, 0, 0);
            var sY = pos + new Vector4(0, +scale, 0);
            var eY = pos + new Vector4(0, -scale, 0);
            var sZ = pos + new Vector4(0, 0, +scale);
            var eZ = pos + new Vector4(0, 0, -scale);
            Debug.DrawLine(sX , eX , color);
            Debug.DrawLine(sY , eY , color);
            Debug.DrawLine(sZ , eZ , color);
        }

        public static void DrawAxes(Vector4 pos, float scale = 1.0f)
        {
            Debug.DrawLine(pos, pos + new Vector4(scale, 0, 0), Color.red);
            Debug.DrawLine(pos, pos + new Vector4(0, scale, 0), Color.green);
            Debug.DrawLine(pos, pos + new Vector4(0, 0, scale), Color.blue);
        }

        public static void DrawAxes(Matrix4x4 transform, float scale = 1.0f)
        {
            Vector4 p = transform * new Vector4(0, 0, 0, 1);
            Vector4 x = transform * new Vector4(scale, 0, 0, 1);
            Vector4 y = transform * new Vector4(0, scale, 0, 1);
            Vector4 z = transform * new Vector4(0, 0, scale, 1);

            Debug.DrawLine(p, x, Color.red);
            Debug.DrawLine(p, y, Color.green);
            Debug.DrawLine(p, z, Color.blue);
        }

        public static void DrawQuad(Matrix4x4 transform, Color color)
        {
            Vector4[] v = s_UnitSquare;
            Matrix4x4 m = transform;
            for (int i = 0; i < 4; i++)
            {
                var s = m * v[i];
                var e = m * v[(i + 1) % 4];
                Debug.DrawLine(s , e , color);
            }
        }

        public static void DrawPlane(Plane plane, float scale, Color edgeColor, float normalScale, Color normalColor)
        {
            // Flip plane distance: Unity Plane distance is from plane to origin
            DrawPlane(new Vector4(plane.normal.x, plane.normal.y, plane.normal.z, -plane.distance), scale, edgeColor, normalScale, normalColor);
        }

        public static void DrawPlane(Vector4 plane, float scale, Color edgeColor, float normalScale, Color normalColor)
        {
            Vector3 n = Vector3.Normalize(plane);
            float   d = plane.w;

            Vector3 u = Vector3.up;
            Vector3 r = Vector3.right;
            if (n == u)
                u = r;

            r = Vector3.Cross(n, u);
            u = Vector3.Cross(n, r);

            for (int i = 0; i < 4; i++)
            {
                var s = scale * s_UnitSquare[i];
                var e = scale * s_UnitSquare[(i + 1) % 4];
                s = s.x * r + s.y * u + n * d;
                e = e.x * r + e.y * u + n * d;
                Debug.DrawLine(s, e, edgeColor);
            }

            // Diagonals
            {
                var s = scale * s_UnitSquare[0];
                var e = scale * s_UnitSquare[2];
                s = s.x * r + s.y * u + n * d;
                e = e.x * r + e.y * u + n * d;
                Debug.DrawLine(s, e, edgeColor);
            }
            {
                var s = scale * s_UnitSquare[1];
                var e = scale * s_UnitSquare[3];
                s = s.x * r + s.y * u + n * d;
                e = e.x * r + e.y * u + n * d;
                Debug.DrawLine(s, e, edgeColor);
            }

            Debug.DrawLine(n * d, n * (d + 1 * normalScale), normalColor);
        }

        public static Color GetRandomColor() => new(Random.value, Random.value, Random.value, 1);
    }
}