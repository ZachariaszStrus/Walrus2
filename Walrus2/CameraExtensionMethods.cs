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
        public static void RotateWithMouse(this PerspectiveCamera camera, Point mouseInitialPosition,
            Point mousePosition, Point3D center)
        {
            Vector delta = new Vector(mousePosition.X - mouseInitialPosition.X,
                                      mousePosition.Y - mouseInitialPosition.Y);

            Point3D newPosition = new Point3D(camera.Position.X, camera.Position.Y, camera.Position.Z);

            double radius = camera.GetRadius();
            double xyRadius = Math.Sqrt(Math.Pow(camera.Position.X - center.X, 2) +
                                 Math.Pow(camera.Position.Y - center.Y, 2));
            var theta = -delta.X / 400;
            var phi = delta.Y / 200;

            //up-down
            
            newPosition.Z = camera.Position.Z * Math.Cos(phi) + xyRadius * Math.Sin(phi);
            xyRadius = xyRadius * Math.Cos(phi) - camera.Position.Z * Math.Sin(phi);

            newPosition.X = Math.Sqrt(Math.Pow(xyRadius, 2) /
                                        (1 + Math.Pow(camera.Position.Y / camera.Position.X, 2))) *
                            (camera.Position.X < 0 ? -1 : 1);
            newPosition.Y = camera.Position.Y / camera.Position.X * newPosition.X;

            if(xyRadius < 1)
            {
                newPosition = new Point3D(camera.Position.X, camera.Position.Y, camera.Position.Z);
            }
            
          
            //left-right
            double nx = newPosition.X * Math.Cos(theta) - newPosition.Y * Math.Sin(theta);
            double ny = newPosition.Y * Math.Cos(theta) + newPosition.X * Math.Sin(theta);

            newPosition.X = nx;
            newPosition.Y = ny;

            camera.Position = newPosition;
            camera.LookDirection = new Vector3D(-camera.Position.X, -camera.Position.Y, -camera.Position.Z);
            camera.UpDirection = new Vector3D(0, 0, 1);
        }

        public static void MoveWithMouseWheel(this PerspectiveCamera camera, int delta, Point3D center)
        {
            delta /= 10;
            double k = Math.Max(Math.Abs(camera.Position.X), 
                Math.Max(Math.Abs(camera.Position.Y), Math.Abs(camera.Position.Z)));

            Point3D newPosition = new Point3D(camera.LookDirection.X * delta / k + camera.Position.X,
                                              camera.LookDirection.Y * delta / k + camera.Position.Y,
                                              camera.LookDirection.Z * delta / k + camera.Position.Z);

            double r = Math.Sqrt(Math.Pow(newPosition.X - center.X, 2) +
                                 Math.Pow(newPosition.Y - center.Y, 2) +
                                 Math.Pow(newPosition.Z - center.Z, 2));


            if (Math.Abs(r) >= 20)
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
