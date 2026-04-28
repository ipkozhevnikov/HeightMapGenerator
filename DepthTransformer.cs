using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static orthoplane.MainForm;

namespace orthoplane
{
    public class DepthTransformer
    {
        public static List<Vector3d> ConvertDepthToVoxelMap(
            Bitmap depthMap,
            Camera camera)
        {
            float centerX = depthMap.Width / 2; //центр карты x
            float centerY = depthMap.Height / 2; //центр карты y
            /*BitmapData depthData = depthMap.LockBits(
                new Rectangle(0, 0, depthMap.Width, depthMap.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);


            byte[] pixelBuffer = new byte[depthData.Stride * depthMap.Height];
            System.Runtime.InteropServices.Marshal.Copy(depthData.Scan0, pixelBuffer, 0, pixelBuffer.Length);
            */
            List<Vector3d> result = new List<Vector3d>();

            for (int v = 0; v < depthMap.Height; v++)
            {
                for (int u = 0; u < depthMap.Width; u++)
                {
                    /*int index = v * depthData.Stride + u * 4;
                    if (pixelBuffer[index] == 0)
                        continue;*/
                    //todo нужно инвертировать или нет?
                    //double z = (255 - pixelBuffer[index]);// * (10000 / 255);
                    //убираем всю черноту
                    if (depthMap.GetPixel(u, v).R == 0)
                    {
                        continue;
                    }
                    double z = (255 - depthMap.GetPixel(u, v).R) * 2 + 255;//чем ярче тем ближе (для инвертированной карты глубины)
                    double x = (u - centerX) * z / camera.FocalLength;
                    double y = (v - centerY) * z / camera.FocalLength;
                    Vector3d pointCamera = new Vector3d(x, y, z); // координаты точки в системе координат камеры
                    Vector3d pointWorld = CoordinateTransformer.CameraToWorld(pointCamera, camera.Rotation, camera.Position); //переводим координаты в мировые
                    result.Add(pointWorld);
                }
            }

            //depthMap.UnlockBits(depthData);
            return result;
        }

        public static List<Vector3d> ConvertVoxelMapToHeightMap(
            List<Vector3d> voxelMap,
            Camera camera)
        {
            List<Vector3d> result = new List<Vector3d>();
            foreach (var point in voxelMap)
            {
                result.Add(CoordinateTransformer.WorldToCamera(point, camera.Rotation, camera.Position));
            }
            return result;
        }
    }
}
