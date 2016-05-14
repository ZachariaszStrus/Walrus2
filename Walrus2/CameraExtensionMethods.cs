using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Walrus2
{
    public static class CameraExtensionMethods
    {
        public static void RotateWithMouse(this PerspectiveCamera camera, Vector delta, Point3D center)
        {
            var theta = delta.X / 700 * 360;
            var phi = delta.Y / 700 * 360;
            
            var originQuaternion = new Quaternion(camera.Position.X, camera.Position.Y, camera.Position.Z, 0);
            
            Vector3D xyVector = Vector3D.CrossProduct((new Vector3D(0, 0, 1)), 
                                            (new Vector3D(camera.Position.X, camera.Position.Y, camera.Position.Z)));
            var phiRotation = new Quaternion(xyVector, phi);
            var conjugatePhi = new Quaternion(xyVector, phi);
            conjugatePhi.Conjugate();

            var thetaRotation = new Quaternion(new Vector3D(0, 0, 1), theta);
            var conjugateTheta = new Quaternion(new Vector3D(0, 0, 1), theta);
            conjugateTheta.Conjugate();

            var rotatedPoint = conjugatePhi * originQuaternion * phiRotation;

            if (rotatedPoint.X * camera.Position.X < 0)
            {
                rotatedPoint.X = camera.Position.X;
                rotatedPoint.Y = camera.Position.Y;
            }

            rotatedPoint = conjugateTheta * rotatedPoint * thetaRotation;

            camera.Position = new Point3D(rotatedPoint.X, rotatedPoint.Y, rotatedPoint.Z);
            camera.LookDirection = new Point3D() - camera.Position;
            camera.UpDirection = new Vector3D(0, 0, 1);
        }

        public static void MoveWithMouseWheel(this PerspectiveCamera camera, double delta, Point3D center)
        {
            delta = camera.GetRadius() / 10 * (delta > 0 ? 1 : -1);
            double k = Math.Max(Math.Abs(camera.Position.X),
                Math.Max(Math.Abs(camera.Position.Y), Math.Abs(camera.Position.Z)));

            Point3D newPosition = new Point3D(camera.LookDirection.X * delta / k + camera.Position.X,
                                                camera.LookDirection.Y * delta / k + camera.Position.Y,
                                                camera.LookDirection.Z * delta / k + camera.Position.Z);

            double r = Math.Sqrt(Math.Pow(newPosition.X - center.X, 2) +
                                    Math.Pow(newPosition.Y - center.Y, 2) +
                                    Math.Pow(newPosition.Z - center.Z, 2));

            if (r > 1)
            {
                camera.LookDirection = new Vector3D(-newPosition.X, -newPosition.Y, -newPosition.Z);
                camera.Position = newPosition;
                camera.UpDirection = new Vector3D(0, 0, 1);
            }
            
        }

        public static void SetPosition(this PerspectiveCamera camera, Point3D position)
        {
            camera.Position = position;
            camera.LookDirection = new Vector3D(-camera.Position.X, -camera.Position.Y, -camera.Position.Z);
            camera.UpDirection = new Vector3D(0, 0, 1);
        }

        public static double GetRadius(this PerspectiveCamera camera)
        {
            Point3D center = new Point3D(0, 0, 0);
            return Math.Sqrt(Math.Pow(camera.Position.X - center.X, 2) +
                                 Math.Pow(camera.Position.Y - center.Y, 2) +
                                 Math.Pow(camera.Position.Z - center.Z, 2));
        }

    }
}
