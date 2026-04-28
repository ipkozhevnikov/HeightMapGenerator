using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace orthoplane
{
    public class Vector3d
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Vector3d(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Vector3d operator +(Vector3d a, Vector3d b) =>
            new Vector3d(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        public static Vector3d operator -(Vector3d a, Vector3d b) =>
            new Vector3d(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        public static Vector3d operator *(Vector3d v, float scalar) =>
        new Vector3d(v.X * scalar, v.Y * scalar, v.Z * scalar);

        public static Vector3d operator /(Vector3d v, float scalar) =>
            new Vector3d(v.X / scalar, v.Y / scalar, v.Z / scalar);

        public float Length() => (float)Math.Sqrt(X * X + Y * Y + Z * Z);

        public Vector3d Normalize()
        {
            float length = Length();
            if (length > 0)
                return this / length;
            return this;
        }

        public override string ToString() => $"({X:F2}, {Y:F2}, {Z:F2})";
    }
}
