using System;
using System.Windows.Media.Media3D;

namespace Walrus2
{
    public class Graph
    {
        public bool[,] NeighbourMatrix { get; set; }

        public Point3D[] Positions { get; set; }

        public int Size { get; set; }

        public Graph(int n, int r, double p)
        {
            NeighbourMatrix = new bool[n, n];
            Positions = new Point3D[n];
            Size = n;

            Random rand = new Random();
            for (int i = 0; i < n; i++)
            {
                double x = rand.Next(-r, r);
                double y = rand.Next(-r, r);
                double z = rand.Next(-r, r);
                Positions[i] = new Point3D(x, y, z);
            }

            for (int y = 0; y < n; y++)
            {
                for (int x = y + 1; x < n; x++) 
                {
                    if(rand.Next(n) <= p*n)
                    {
                        NeighbourMatrix[x, y] = true;
                        NeighbourMatrix[y, x] = true;
                    }
                }
            }
        }
    }
}
