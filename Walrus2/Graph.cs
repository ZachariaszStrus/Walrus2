using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Media.Media3D;

namespace Walrus2
{
    public class Graph
    {
        public string Name { get; set; }

        public Dictionary<string, Node> Nodes { get; set; }

        public Node Root { get; set; }
        
        public Graph(string file)
        {
            Name = Path.GetFileName(file);
            Nodes = new Dictionary<string, Node>();

            LoadFromFile(file);
            
            foreach (var node in Nodes)
            {
                node.Value.SetParents();
            }
            
            RecursiveSphereAlgorithm(Root);
        }
        
        public void LoadFromFile(string file)
        {
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
                            Nodes[currentLineArray[0].Trim()].AddChild(Nodes[currentLineArray[j].Trim()]);
                        }
                    }
                    if (i == 0)
                    {
                        Root = Nodes[currentLineArray[0]];
                    }
                }
            }
        }

        public void RecursiveSphereAlgorithm(Node parent, double radiusFactor = 1, double angleFactor = 1)
        {
            Vector3D centralVector = new Vector3D(0, 0, 1);
            Vector3D xyVector = new Vector3D(0, 1, 0);
            if (parent != Root)
            {
                centralVector = parent.Position - parent.Parent.Position;
                centralVector.Normalize();
                xyVector = Vector3D.CrossProduct(centralVector, (new Vector3D(0, 0, 1)));
                xyVector.Normalize();
            }

            Func<int, int, int> layerFunc = delegate (int a, int rad)
            {
                return Convert.ToInt32(2 * Math.PI * Math.Sqrt(rad * rad - Math.Pow(rad * Math.Cos(a), 2)) / 20);
            };
            Func<Node, double> getRadius = delegate (Node n)
            {
                return Math.Pow(radiusFactor, 1.5) * Math.Sqrt((double)n.TotalChildren()) * 10 + n.Parent.Children.Count * 4;
            };
            Func<int, double, double> getPhi = delegate (int l, double rad)
            {
                return l * 1000.0 / rad * angleFactor;
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
            if (parent == Root)
            {
                foreach (var child in parent.Children)
                {
                    double radius = getRadius(child);
                    Point3D newPosition = new Point3D(radius, 0, 0);

                    Geometry3D.RotatePoint(ref newPosition, centralVector, theta);

                    xyVector = Vector3D.CrossProduct(centralVector,
                        (new Vector3D(newPosition.X, newPosition.Y, 0)));
                    Geometry3D.RotatePoint(ref newPosition, xyVector, phi);

                    child.Position = newPosition;
                    theta += (1.0 / parent.Children.Count) * 360;
                    phi += (1.0 / parent.Children.Count) * 90 + 270;
                    RecursiveSphereAlgorithm(child, radiusFactor, angleFactor);
                }
            }
            // children of root --------------------
            else
            {
                foreach (var child in parent.Children)
                {
                    double radius = getRadius(child);
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

                    if (layer != 0)
                    {
                        phi = getPhi(layer, radius);
                        theta = getTheta(toDoInLayer, doneInLayer);

                        Geometry3D.RotatePoint(ref newPosition, parent.Position, xyVector, phi);
                        Geometry3D.RotatePoint(ref newPosition, parent.Position, centralVector, theta);
                    }

                    child.Position = newPosition;
                    doneInLayer++;
                    childrenDone++;
                    RecursiveSphereAlgorithm(child, radiusFactor, angleFactor);
                }
            }
        }

        public void ChangeChildrenAngle(double radiusFactor = 1, double angleFactor = 1)
        {
            RecursiveSphereAlgorithm(Root, radiusFactor, angleFactor);
        }
    }
}
