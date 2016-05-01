﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Walrus2
{
    public class Graph
    {
        bool[,] neighbourMatrix;
        Point3D[] positions;
        int size;

        public bool[,] NeighbourMatrix
        {
            get
            {
                return neighbourMatrix;
            }

            set
            {
                neighbourMatrix = value;
            }
        }

        public Point3D[] Positions
        {
            get
            {
                return positions;
            }

            set
            {
                positions = value;
            }
        }

        public int Size
        {
            get
            {
                return size;
            }

            set
            {
                size = value;
            }
        }

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