using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Media3D;

namespace Walrus2
{
    public class Graph
    {
        public Dictionary<string, Node> Nodes { get; set; }

        public List<Edge> Edges { get; set; }

        public Node Root { get; set; }
        
        public int MinDistance { get; set; }


        public Graph(string file)
        {
            Nodes = new Dictionary<string, Node>();
            Edges = new List<Edge>();

            LoadFromFile(file);
            foreach (var node in Nodes)
            {
                node.Value.SetParents();
            }


            //SetRandomPositions();
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
        }

        public void SphereAlgorithm()
        {

            Random r = new Random();
            int radius;
            int count = 1;
            while (true)
            {
                foreach(var node in Nodes)
                {
                    Node parent = node.Value;
                    if (parent.Position.X == 0 && parent.Position.Y == 0 && parent.Position.Z == 0 &&
                        node.Value != Root)
                    {
                        continue;
                    }

                    Vector3D centralVector = new Vector3D(0, 0, 1);
                    Vector3D xyVector = new Vector3D();
                    if (node.Value != Root)
                    {
                        centralVector = new Vector3D();
                        centralVector = parent.Position - parent.Parent.Position;
                        centralVector.Normalize();
                        xyVector = Vector3D.CrossProduct(centralVector, (new Vector3D(0, 0, 1)));
                        xyVector.Normalize();
                    }

                    double theta = 0;
                    double phi = 0;
                    int layer = 0;
                    int toDoInLayer = 1;
                    int doneInLayer = 0;
                    int childrenDone = 0;
                    foreach (var child in parent.Children)
                    {
                        radius = (int)Math.Sqrt((double)child.TotalChildren()) * 30 + parent.Children.Count;

                        if(node.Value == Root)
                        {
                            var originQuaternion = new Quaternion(radius, 0, 0, 0);
                            phi = r.Next(-180, 180);

                            var thetaRotation = new Quaternion(centralVector, theta);
                            var conjugateTheta = new Quaternion(centralVector, theta);
                            conjugateTheta.Conjugate();

                            var rotatedPoint = conjugateTheta * originQuaternion * thetaRotation;

                            xyVector = Vector3D.CrossProduct(centralVector, (new Vector3D(rotatedPoint.X, rotatedPoint.Y, 0)));
                            var phiRotation = new Quaternion(xyVector, phi);
                            var conjugatePhi = new Quaternion(xyVector, phi);
                            conjugatePhi.Conjugate();

                            rotatedPoint = conjugatePhi * rotatedPoint * phiRotation;

                            double x = rotatedPoint.X;
                            double y = rotatedPoint.Y;
                            double z = rotatedPoint.Z;

                            child.Position = new Point3D(x + parent.Position.X, y + parent.Position.Y, z + parent.Position.Z);
                            theta += (1.0 / parent.Children.Count) * 360;
                            count++;
                        }
                        else
                        {
                            if(doneInLayer == toDoInLayer)
                            {
                                layer++;
                                doneInLayer = 0;
                                toDoInLayer = Math.Min(layer * 3 + 1, parent.Children.Count - childrenDone);
                                if((parent.Children.Count - childrenDone - toDoInLayer) < 0.25 * (layer * 3 + 1))
                                {
                                    toDoInLayer = parent.Children.Count - childrenDone;
                                }
                            }
                            

                            Point3D newPosition = new Point3D(0, 0, 0);
                            newPosition += centralVector * radius;

                            var originQuaternion = new Quaternion(newPosition.X, newPosition.Y, newPosition.Z, 0);

                            var phiRotation = new Quaternion(xyVector, phi);
                            var conjugatePhi = new Quaternion(xyVector, phi);
                            conjugatePhi.Conjugate();
                            
                            var thetaRotation = new Quaternion(centralVector, theta);
                            var conjugateTheta = new Quaternion(centralVector, theta);
                            conjugateTheta.Conjugate();


                            var rotatedPoint = conjugatePhi * originQuaternion * phiRotation;
                            rotatedPoint = conjugateTheta * rotatedPoint * thetaRotation;
                            newPosition.X = rotatedPoint.X;
                            newPosition.Y = rotatedPoint.Y;
                            newPosition.Z = rotatedPoint.Z;

                            newPosition.X += parent.Position.X;
                            newPosition.Y += parent.Position.Y;
                            newPosition.Z += parent.Position.Z;
                            child.Position = newPosition;

                            phi = layer * 20;
                            theta += (1.0 / toDoInLayer) * 360;
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

    }
}
