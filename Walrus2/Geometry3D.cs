using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Walrus2
{
    class Geometry3D
    {
        public static void RotatePoint(ref Point3D point, Vector3D axis, double angle)
        {
            var originQuaternion = new Quaternion(point.X, point.Y, point.Z, 0);

            var rotation = new Quaternion(axis, angle);
            var conjugate = new Quaternion(axis, angle);
            conjugate.Conjugate();

            var rotatedPoint = conjugate * originQuaternion * rotation;

            point = new Point3D(rotatedPoint.X, rotatedPoint.Y, rotatedPoint.Z);
        }

        public static void RotatePoint(ref Point3D point, Point3D center, Vector3D axis, double angle)
        {
            if (axis.Length == 0) axis = new Vector3D(0, 0, 1);

            Point3D newPoint = new Point3D(point.X - center.X, point.Y - center.Y, point.Z - center.Z);
            RotatePoint(ref newPoint, axis, angle);
            newPoint += (center - new Point3D());
            point.X = newPoint.X;
            point.Y = newPoint.Y;
            point.Z = newPoint.Z;
        }

        public static double DistanceBetweenPoints(Point3D p1, Point3D p2)
        {
            return (p1 - p2).Length;
        }
    }
}
