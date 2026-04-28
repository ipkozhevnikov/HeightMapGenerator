using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace orthoplane
{
    public class HeightMapGenerator
    {
        public static Bitmap CreateHeightMapWithInterpolation(List<Vector3d> points, int width, int height)
        {
            if (points == null || points.Count == 0)
                return new Bitmap(width, height);

            // Находим границы данных
            double minX = points.Min(p => p.X);
            double maxX = points.Max(p => p.X);
            double minY = points.Min(p => p.Y);
            double maxY = points.Max(p => p.Y);
            double minZ = points.Min(p => p.Z);
            double maxZ = points.Max(p => p.Z);

            // Создаем матрицу высот
            double[,] heightGrid = new double[width, height];
            bool[,] hasValue = new bool[width, height];

            // Масштабирующие коэффициенты
            double scaleX = (maxX - minX) > 0 ? (width - 1) / (maxX - minX) : 1;
            double scaleY = (maxY - minY) > 0 ? (height - 1) / (maxY - minY) : 1;

            // Заполняем матрицу максимальными значениями Z
            foreach (var point in points)
            {
                int pixelX = (int)((point.X - minX) * scaleX);
                int pixelY = (int)((point.Y - minY) * scaleY);

                pixelX = Math.Clamp(pixelX, 0, width - 1);
                pixelY = Math.Clamp(pixelY, 0, height - 1);

                if (!hasValue[pixelX, pixelY] || point.Z > heightGrid[pixelX, pixelY])
                {
                    heightGrid[pixelX, pixelY] = point.Z;
                    hasValue[pixelX, pixelY] = true;
                }
            }

            // Интерполируем пустые ячейки (опционально)
           InterpolateEmptyCells(heightGrid, hasValue, width, height);

            // Создаем bitmap
            Bitmap bitmap = new Bitmap(width, height);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (hasValue[x, y])
                    {
                        double normalizedZ = (heightGrid[x, y] - minZ) / (maxZ - minZ);
                        int brightness = (int)(normalizedZ * 255);
                        brightness = Math.Clamp(brightness, 0, 255);
                        bitmap.SetPixel(x, height - 1 - y, Color.FromArgb(brightness, brightness, brightness));
                    }
                    else
                    {
                        bitmap.SetPixel(x, height - 1 - y, Color.Black);
                    }
                }
            }

            return bitmap;
        }

        private static void InterpolateEmptyCells(double[,] heightGrid, bool[,] hasValue, int width, int height)
        {
            // Простая интерполяция - заполняем пустые ячейки средним значением соседей
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (!hasValue[x, y])
                    {
                        double sum = 0;
                        int count = 0;

                        // Проверяем 8 соседей
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            for (int dy = -1; dy <= 1; dy++)
                            {
                                int nx = x + dx;
                                int ny = y + dy;

                                if (nx >= 0 && nx < width && ny >= 0 && ny < height && hasValue[nx, ny])
                                {
                                    sum += heightGrid[nx, ny];
                                    count++;
                                }
                            }
                        }

                        if (count > 0)
                        {
                            heightGrid[x, y] = sum / count;
                            hasValue[x, y] = true;
                        }
                    }
                }
            }
        }
    }
}
