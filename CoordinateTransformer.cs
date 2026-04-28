using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace orthoplane
{
    public class CoordinateTransformer
    {
        // Преобразование углов Эйлера в матрицу поворота (ZYX порядок)
        public static Matrix3x3 EulerToMatrix(double yaw, double pitch, double roll)
        {
            double cosY = Math.Cos(yaw);
            double sinY = Math.Sin(yaw);
            double cosP = Math.Cos(pitch);
            double sinP = Math.Sin(pitch);
            double cosR = Math.Cos(roll);
            double sinR = Math.Sin(roll);

            // Матрица поворота для Z (yaw)
            Matrix3x3 Rz = new Matrix3x3(
                cosY, -sinY, 0,
                sinY, cosY, 0,
                0, 0, 1
            );

            // Матрица поворота для Y (pitch)
            Matrix3x3 Ry = new Matrix3x3(
                cosP, 0, sinP,
                0, 1, 0,
                -sinP, 0, cosP
            );

            // Матрица поворота для X (roll)
            Matrix3x3 Rx = new Matrix3x3(
                1, 0, 0,
                0, cosR, -sinR,
                0, sinR, cosR
            );

            // Комбинированная матрица поворота: R = Rz * Ry * Rx
            return Multiply(Multiply(Rz, Ry), Rx);
        }

        // Умножение матриц 3x3
        public static Matrix3x3 Multiply(Matrix3x3 a, Matrix3x3 b)
        {
            return new Matrix3x3(
                a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31,
                a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32,
                a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33,

                a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31,
                a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32,
                a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33,

                a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31,
                a.M31 * b.M12 + a.M32 * b.M22 + a.M33 * b.M32,
                a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33
            );
        }

        // Преобразование из системы камеры в мировую систему
        public static Vector3d CameraToWorld(Vector3d pointCamera, Matrix3x3 rotation, Vector3d cameraPosition)
        {
            // P_world = R * P_camera + T
            Vector3d rotated = rotation * pointCamera;
            return rotated + cameraPosition;
        }

        // Преобразование из мировой системы в систему камеры
        public static Vector3d WorldToCamera(Vector3d pointWorld, Matrix3x3 rotation, Vector3d cameraPosition)
        {
            // P_camera = R^T * (P_world - T)
            Vector3d translated = pointWorld - cameraPosition;
            Matrix3x3 rotationTransposed = rotation.Transpose();
            return rotationTransposed * translated;
        }

        // Преобразование градусов в радианы
        public static double DegreesToRadians(double degrees) => degrees * Math.PI / 180.0;
    }
}
