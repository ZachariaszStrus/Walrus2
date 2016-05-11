using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Media3D;

namespace Walrus2
{
    public class Graph
    {
        public string Name { get; set; }

        public Dictionary<string, Node> Nodes { get; set; }

        public List<Edge> Edges { get; set; }

        public Node Root { get; set; }
        
        public int MinDistance { get; set; }
        
        public Graph(string file)
        {
            Name = Path.GetFileName(file);
            Nodes = new Dictionary<string, Node>();
            Edges = new List<Edge>();

            using(StreamReader sr = new StreamReader(file))
            {
                LoadFromFile(sr);
            }
            foreach (var node in Nodes)
            {
                node.Value.SetParents();
            }
        }
        
        public void LoadFromFile(StreamReader sr)
        {
            var linesArray = sr.ReadToEnd().Split('\n');
            for (var i = 0; i < linesArray.Length; i++)
            {
                var currentLineArray = linesArray[i].Split('\t');
                if (currentLineArray.Length == 1)
                {
                    currentLineArray = linesArray[i].Split(' ');
                }
                if (!Nodes.ContainsKey(currentLineArray[0].Trim()))
                {
                    Nodes.Add(currentLineArray[0].Trim(), new Node());
                }
                for (var j = 1; j < currentLineArray.Length; j++)
                {
                    if (currentLineArray[j] == "0")
                    {
                        break;
                    }
                    if (!Nodes.ContainsKey(currentLineArray[j].Trim()))
                    {
                        Nodes.Add(currentLineArray[j].Trim(), new Node());
                        Nodes[currentLineArray[0].Trim()].Children.Add(Nodes[currentLineArray[j].Trim()]);
                        Edges.Add(new Edge(Nodes[currentLineArray[0].Trim()], Nodes[currentLineArray[j].Trim()]));
                    }
                }
                if (i == 0)
                {
                    Root = Nodes[currentLineArray[0]];
                }
            }
        }

        public void SphereAlgorithm()
        {

            Random r = new Random();
            int count = 1;
            while (true)
            {
                foreach (var node in Nodes)
                {
                    Node parent = node.Value;
                    if (parent.Position.X == 0 && parent.Position.Y == 0 && parent.Position.Z == 0 &&
                        node.Value != Root)
                    {
                        continue;
                    }

                    Vector3D centralVector = new Vector3D(0, 0, 1);
                    Vector3D xyVector = new Vector3D(0, 1, 0);
                    if (node.Value != Root)
                    {
                        centralVector = new Vector3D();
                        centralVector = parent.Position - parent.Parent.Position;
                        centralVector.Normalize();
                        xyVector = Vector3D.CrossProduct(centralVector, (new Vector3D(0, 0, 1)));
                        xyVector.Normalize();
                    }

                    Func<int, int, int> layerFunc = delegate (int a, int rad)
                    {
                        return Convert.ToInt32(2 * Math.PI * Math.Sqrt(rad * rad - Math.Pow(rad * Math.Cos(a), 2)) / 20);
                    };
                    Func<Node, int> getRadius = delegate (Node n)
                    {
                        return (int)Math.Sqrt((double)n.TotalChildren()) * 10 + n.Parent.Children.Count * 4;
                    };
                    Func<int, int, double> getPhi = delegate (int l, int rad)
                    {
                        return l * 1000.0 / rad;
                    };
                    Func<int, int, double> getTheta = delegate (int toDo, int done)
                    {
                        return (1.0 / toDo) * 360 * done;
                    };

                    double theta = 0;
                    double phi = 0;
                    int layer = 0;
                    int toDoInLayer = 1;
                    int doneInLayer = 0;
                    int childrenDone = 0;
                    // root --------------------
                    if (node.Value == Root)
                    {
                        foreach(var child in parent.Children)
                        {
                            int radius = getRadius(child);
                            Point3D newPosition = new Point3D(radius, 0, 0);

                            phi = r.Next(-180, 180);
                            Geometry3D.RotatePoint(ref newPosition, centralVector, theta);

                            xyVector = Vector3D.CrossProduct(centralVector, 
                                (new Vector3D(newPosition.X, newPosition.Y, 0)));
                            Geometry3D.RotatePoint(ref newPosition, xyVector, phi);

                            child.Position = newPosition;
                            theta += (1.0 / parent.Children.Count) * 360;
                            count++;   
                            
                        }
                        AdjustRoot(1000);
                    }
                    // children of root --------------------
                    else
                    {
                        foreach (var child in parent.Children)
                        {
                            int radius = getRadius(child);
                            if (doneInLayer == toDoInLayer)
                            {
                                layer++;
                                doneInLayer = 0;
                                toDoInLayer = layer * 3 + 1;
                                toDoInLayer = Math.Min(toDoInLayer, parent.Children.Count - childrenDone);
                                if ((parent.Children.Count - childrenDone - toDoInLayer) < 0.25 * (layer * 3 + 1))
                                {
                                    toDoInLayer = parent.Children.Count - childrenDone;
                                }
                            }
                            
                            Point3D newPosition = parent.Position + centralVector * radius;

                            phi = getPhi(layer, radius);
                            theta = getTheta(toDoInLayer, doneInLayer);

                            Geometry3D.RotatePoint(ref newPosition, parent.Position, xyVector, phi);
                            Geometry3D.RotatePoint(ref newPosition, parent.Position, centralVector, theta);
                            
                            child.Position = newPosition;
                            doneInLayer++;
                            childrenDone++;
                            count++;
                        }
                    }
                    if (count >= Nodes.Count)
                    {
                        return;
                    }
                }
            }
        }

        public void AdjustRoot(int n)
        {
            for (int i = 0; i < n; i++)
            {
                foreach (var child1 in Root.Children)
                {
                    Node closestNode = null;
                    double delta = 0;
                    foreach (var child2 in Root.Children)
                    {
                        if (child1 != child2 &&
                            (closestNode == null || Geometry3D.DistanceBetweenPoints(child1.Position, child2.Position) < delta))
                        {
                            closestNode = child2;
                            delta = Geometry3D.DistanceBetweenPoints(child1.Position, child2.Position);
                        }
                    }
                    if (delta > 100) continue;

                    Vector3D v1 = closestNode.Position - (new Point3D());
                    Vector3D v2 = child1.Position - (new Point3D());
                    Vector3D rotationVector = Vector3D.CrossProduct(v1, v2);

                    Point3D newPosition = closestNode.Position;
                    double angle = closestNode.TotalChildren();
                    Geometry3D.RotatePoint(ref newPosition, rotationVector, angle);
                    closestNode.Position = newPosition;
                }
            }
        }
    }
}
