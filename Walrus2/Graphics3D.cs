using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Walrus2
{
    class Graphics3D
    {
        public static GeometryModel3D GetCube(Point3D center, Point3D dimensions, SolidColorBrush color)
        {
            int[] triangles = { 2, 3, 1, 2, 1, 0,
                                7, 1, 3, 7, 5, 1,
                                6, 5, 7, 6, 4, 5,
                                6, 2, 0, 6, 0, 4,
                                2, 7, 3, 2, 6, 7,
                                0, 1, 5, 0, 5, 4 };

            GeometryModel3D cube = new GeometryModel3D();
            MeshGeometry3D mesh = new MeshGeometry3D();

            mesh.Positions.Add(new Point3D(
                center.X - dimensions.X / 2, center.Y - dimensions.Y / 2, center.Z - dimensions.Z / 2));
            mesh.Positions.Add(new Point3D(
                center.X + dimensions.X / 2, center.Y - dimensions.Y / 2, center.Z - dimensions.Z / 2));
            mesh.Positions.Add(new Point3D(
                center.X - dimensions.X / 2, center.Y + dimensions.Y / 2, center.Z - dimensions.Z / 2));
            mesh.Positions.Add(new Point3D(
                center.X + dimensions.X / 2, center.Y + dimensions.Y / 2, center.Z - dimensions.Z / 2));
            mesh.Positions.Add(new Point3D(
                center.X - dimensions.X / 2, center.Y - dimensions.Y / 2, center.Z + dimensions.Z / 2));
            mesh.Positions.Add(new Point3D(
                center.X + dimensions.X / 2, center.Y - dimensions.Y / 2, center.Z + dimensions.Z / 2));
            mesh.Positions.Add(new Point3D(
                center.X - dimensions.X / 2, center.Y + dimensions.Y / 2, center.Z + dimensions.Z / 2));
            mesh.Positions.Add(new Point3D(
                center.X + dimensions.X / 2, center.Y + dimensions.Y / 2, center.Z + dimensions.Z / 2));

            foreach (var t in triangles)
            {
                mesh.TriangleIndices.Add(t);
            }

            cube.Geometry = mesh;
            cube.Material = new DiffuseMaterial(color);
            return cube;
        }

        public static GeometryModel3D GetLine(Point3D p0, Point3D p1, SolidColorBrush color, double width = 0.5)
        {
            Vector3D delta = new Vector3D(p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
            double length = Math.Sqrt(Math.Pow(delta.X, 2) + Math.Pow(delta.Y, 2) + Math.Pow(delta.Z, 2));
            int[] triangles = { 2, 3, 1, 2, 1, 0,
                                7, 1, 3, 7, 5, 1,
                                6, 5, 7, 6, 4, 5,
                                6, 2, 0, 6, 0, 4,
                                2, 7, 3, 2, 6, 7,
                                0, 1, 5, 0, 5, 4 };

            GeometryModel3D cube = new GeometryModel3D();
            MeshGeometry3D mesh = new MeshGeometry3D();

            Point3DCollection points = new Point3DCollection();

            Vector3D prepVector = Vector3D.CrossProduct(delta, new Vector3D(0, 0, 1));
            prepVector.Normalize();
            prepVector *= width;

            double theta = 0;
            for (int i = 0; i < 4; i++)
            {
                Point3D point = p0 + prepVector;
                Geometry3D.RotatePoint(ref point, p0, delta, theta);
                points.Add(point);
                theta += 90;
            }

            theta = 0;
            for (int i = 0; i < 4; i++)
            {
                Point3D point = p1 + prepVector;
                Geometry3D.RotatePoint(ref point, p0, delta, theta);
                points.Add(point);
                theta += 90;
            }
            mesh.Positions = points;

            foreach (var t in triangles)
            {
                mesh.TriangleIndices.Add(t);
            }

            cube.Geometry = mesh;
            cube.Material = new DiffuseMaterial(color);
            return cube;
        }
        
    }
}
