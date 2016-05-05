using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Media3D;

namespace Walrus2
{
    public class Graph
    {
        Node Root { get; set; }

        public Dictionary<string, Node> Nodes { get; set; }

        public List<Edge> Edges { get; set; }

        public Graph(int n, double p)
        {
            Nodes = new Dictionary<string, Node>();
            Edges = new List<Edge>();

            Random rand = new Random();
            for (int i = 0; i < n; i++)
            {
                Nodes.Add(i.ToString(),new Node());
            }

            for (int y = 0; y < n; y++)
            {
                for (int x = y + 1; x < n; x++) 
                {
                    if(rand.Next(n) <= p*n)
                    {
                        Edges.Add(new Edge(Nodes[x.ToString()], Nodes[y.ToString()]));
                    }
                }
            }

            SetRandomPositions();
        }

        public Graph(string file)
        {
            Nodes = new Dictionary<string, Node>();
            Edges = new List<Edge>();
            using (StreamReader sr = new StreamReader(file))
            {
                var linesArray = sr.ReadToEnd().Split('\n');
                for (var i = 0; i < linesArray.Length; i++)
                {
                    var currentLineArray = linesArray[i].Split('\t');
                    if (currentLineArray.Length == 1)
                    {
                        currentLineArray = linesArray[i].Split(' ');
                    }
                    for (var j = 0; j < currentLineArray.Length; j++)
                    {
                        if (currentLineArray[j] == "0")
                        {
                            break;
                        }
                        if (!Nodes.ContainsKey(currentLineArray[j].Trim()))
                        {
                            Nodes.Add(currentLineArray[j].Trim(), new Node());
                        }
                        Edges.Add(new Edge(Nodes[currentLineArray[0].Trim()], Nodes[currentLineArray[j].Trim()]));
                    }
                    if (i == 0)
                    {
                        Root = Nodes[currentLineArray[0]];
                    }
                }
            }
            SetRandomPositions();
        }

        public void SetRandomPositions()
        {
            Random r = new Random();
            int b = Nodes.Count * 2;
            foreach (var node in Nodes)
            {
                double x = r.Next(-b, b);
                double y = r.Next(-b, b);
                double z = r.Next(-b, b);
                node.Value.Position = new Point3D(x, y, z);
            }
            Root.Position = new Point3D(0, 0, 0);
        }
    }
}
