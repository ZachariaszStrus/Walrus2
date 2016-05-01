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

        public static GeometryModel3D GetLine(Point3D p0, Point3D p1, SolidColorBrush color, double width = 0.2)
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

            mesh.Positions.Add(new Point3D(p0.X, p0.Y - width, p0.Z + width));
            mesh.Positions.Add(new Point3D(p0.X, p0.Y - width, p0.Z - width));
            mesh.Positions.Add(new Point3D(p0.X, p0.Y + width, p0.Z + width));
            mesh.Positions.Add(new Point3D(p0.X, p0.Y + width, p0.Z - width));

            mesh.Positions.Add(new Point3D(p0.X + length, p0.Y - width, p0.Z + width));
            mesh.Positions.Add(new Point3D(p0.X + length, p0.Y - width, p0.Z - width));
            mesh.Positions.Add(new Point3D(p0.X + length, p0.Y + width, p0.Z + width));
            mesh.Positions.Add(new Point3D(p0.X + length, p0.Y + width, p0.Z - width));

            foreach (var t in triangles)
            {
                mesh.TriangleIndices.Add(t);
            }

            cube.Geometry = mesh;
            cube.Material = new DiffuseMaterial(color);

            
            double theta = Math.Atan2(delta.Y, delta.X);
            double phi = -Math.Atan2(delta.Z, Math.Sqrt(delta.X* delta.X + delta.Y * delta.Y));

            Transform3DGroup transformGroup = new Transform3DGroup();
            RotateTransform3D transXY = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), theta / 2 / Math.PI * 360));
            transXY.CenterX = p0.X;
            transXY.CenterY = p0.Y;
            transXY.CenterZ = p0.Z;
            RotateTransform3D transZ = new RotateTransform3D(new AxisAngleRotation3D(
                new Vector3D(-Math.Sin(theta), Math.Cos(theta), 0), phi / 2 / Math.PI * 360));
            transZ.CenterX = p0.X;
            transZ.CenterY = p0.Y;
            transZ.CenterZ = p0.Z;

            transformGroup.Children.Add(transXY);
            transformGroup.Children.Add(transZ);

            cube.Transform = transformGroup;
            return cube;
        }
    }
}
