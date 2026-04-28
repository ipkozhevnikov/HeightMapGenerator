using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace orthoplane
{
    internal class ElevationColorMap
    {
        public static Color GetElevationColorAsColor(int elevation)
        {
            var rgb = GetElevationColor(elevation);
            return Color.FromArgb(rgb.R, rgb.G, rgb.B);
        }

        public static (byte R, byte G, byte B) GetElevationColor(int elevation)
        {
            if (elevation < 0) elevation = 0;
            if (elevation > 255) elevation = 255;

            float normElevation = elevation / 255.0f;

            byte r, g, b;

            if (normElevation < 0.1f)
            {
                float t = normElevation / 0.1f;
                r = (byte)(10 + t * 40);
                g = (byte)(20 + t * 80);
                b = (byte)(100 + t * 100);
            }
            else if (normElevation < 0.2f)
            {
                float t = (normElevation - 0.1f) / 0.1f;
                r = (byte)(50 + t * 30);
                g = (byte)(100 + t * 80);
                b = (byte)(200 - t * 150);
            }
            else if (normElevation < 0.4f)
            {
                float t = (normElevation - 0.2f) / 0.2f;
                r = (byte)(80 - t * 20);
                g = (byte)(180 - t * 60);
                b = (byte)(50 + t * 20);
            }
            else if (normElevation < 0.6f)
            {
                float t = (normElevation - 0.4f) / 0.2f;
                r = (byte)(60 + t * 100);
                g = (byte)(120 + t * 80);
                b = (byte)(70 - t * 40);
            }
            else if (normElevation < 0.8f)
            {
                float t = (normElevation - 0.6f) / 0.2f;
                r = (byte)(160 + t * 60);
                g = (byte)(200 - t * 80);
                b = (byte)(30 + t * 20);
            }
            else
            {
                float t = (normElevation - 0.8f) / 0.2f;
                r = (byte)(220 - t * 40);
                g = (byte)(120 - t * 70);
                b = (byte)(50 + t * 10);
            }

            return (r, g, b);
        }
    }
}
