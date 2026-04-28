using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace orthoplane
{
    public class Camera
    {
        public Vector3d Position { get; set; }
        public Matrix3x3 Rotation { get; set; }
        public double Yaw { get;  }
        public double Pitch { get;  }
        public double Roll { get;  }
        public double FocalLength { get;  }
        public double SensorWidth { get;  }

        //public double focal

        public Camera(Vector3d position, double yawDegrees, double pitchDegrees, double rollDegrees,double focalLength)
        {
            Position = position;
            Yaw = yawDegrees;
            Pitch = pitchDegrees;
            Roll = rollDegrees;
            FocalLength = focalLength;

            double yawRad = CoordinateTransformer.DegreesToRadians(yawDegrees);
            double pitchRad = CoordinateTransformer.DegreesToRadians(pitchDegrees);
            double rollRad = CoordinateTransformer.DegreesToRadians(rollDegrees);

            Rotation = CoordinateTransformer.EulerToMatrix(yawRad, pitchRad, rollRad);
        }

        public Vector3d CameraToWorld(Vector3d pointCamera) =>
            CoordinateTransformer.CameraToWorld(pointCamera, Rotation, Position);

        public Vector3d WorldToCamera(Vector3d pointWorld) =>
            CoordinateTransformer.WorldToCamera(pointWorld, Rotation, Position);
    }
}
