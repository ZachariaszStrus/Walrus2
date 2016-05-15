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
        public static void RotateWithMouse(this PerspectiveCamera camera, Vector delta)
        {
            var center = camera.Position + camera.LookDirection;
            var theta = delta.X / 700 * 360;
            var phi = delta.Y / 700 * 360;
            
            Vector3D xyVector = Vector3D.CrossProduct((new Vector3D(0, 0, 1)),  camera.Position - center);

            var newPosition = camera.Position;
            Geometry3D.RotatePoint(ref newPosition, center, xyVector, phi);

            if ((newPosition.X - center.X)*(camera.Position.X - center.X) < 0)
            {
                newPosition.X = camera.Position.X;
                newPosition.Y = camera.Position.Y;
            }

            Geometry3D.RotatePoint(ref newPosition, center, new Vector3D(0, 0, 1), theta);

            camera.LookDirection = center - newPosition;
            camera.Position = newPosition;
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
                camera.LookDirection = (camera.Position + camera.LookDirection) - newPosition;
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
