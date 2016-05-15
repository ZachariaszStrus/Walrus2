using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Walrus2
{
    public class Node : IComparable<Node>
    {
        static int _maxChildrenCount = 1;

        private int _totalChildren;

        public const int ChildrenSearchDepth = 10;

        public string ID { get; set; }

        private Point3D _position;
        public Point3D Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
                RefreshGeometryModel();
                if (Parent != null) Parent.RefreshEdge(this);
            }
        }

        public List<Node> Children { get; set; }

        public Dictionary<Node, Edge> Edges { get; set; }

        public Node Parent { get; set; }

        // display -------------------------------
        public bool IsVisible { get; set; }

        public GeometryModel3D GeometryModel { get; private set; }
        
        public Point3D Dimensions { get; private set; }

        public Color ModelColor { get; private set; }
        // ---------------------------------------
        

        public Node(string id, Node parent = null)
        {
            Parent = parent;
            ID = id;
            _totalChildren = 0;
            IsVisible = true;
            Dimensions = new Point3D(10, 10, 10);
            ModelColor = Color.FromRgb(0, 255, 0);

            Parent = null;
            Children = new List<Node>();
            Edges = new Dictionary<Node, Edge>();

            Position = new Point3D();
        }

        public Node(Node n)
        {
            _totalChildren = n._totalChildren;
            IsVisible = n.IsVisible;
            Dimensions = n.Dimensions;
            ModelColor = n.ModelColor;

            Parent = n.Parent;
            Children = new List<Node>();
            n.Children.ForEach(c => Children.Add(new Node(c)));

            Edges = new Dictionary<Node, Edge>();
            Children.ForEach(c => Edges.Add(c, new Edge(this, c)));

            Position = n.Position;
        }

        private void RefreshGeometryModel()
        {
            if(GeometryModel != null)
            {
                Graphics3D.RefreshCubeGeometryModel(GeometryModel, Position, Dimensions);
            }
            else
            {
                GeometryModel = Graphics3D.GetCube(Position, Dimensions, new SolidColorBrush(ModelColor));
            }
        }

        public void RefreshColor()
        {
            double p = (double)Children.Count / _maxChildrenCount;
            double r = 0;
            double g = 0;
            if (p > 0.5)
            {
                r = 255;
                g = 2 * (1.0 - p) * 255;
            }
            else
            {
                g = 255;
                r = 2 * p * 255;
            }
            ModelColor = Color.FromRgb(Convert.ToByte(r), Convert.ToByte(g), 0);
            GeometryModel.Material = new DiffuseMaterial(new SolidColorBrush(ModelColor));
        }

        private void RefreshEdge(Node child)
        {
            Edges[child].EndNode = child;
        }

        public void AddChild(Node child)
        {
            Children.Add(child);
            Edges.Add(child, new Edge(this, child));
            _maxChildrenCount = Children.Count > _maxChildrenCount ? Children.Count : _maxChildrenCount;
        }

        public int TotalChildren(int layer = ChildrenSearchDepth)
        {
            if(_totalChildren != 0)
            {
                return _totalChildren;
            }
            if (Children.Count == 0 || layer == 0)
            {
                return 1;
            }
            _totalChildren = Children.Sum(c => c.TotalChildren(layer - 1));
            return _totalChildren;
        }
        
        public int CompareTo(Node other)
        {
            return other.TotalChildren(ChildrenSearchDepth) - TotalChildren(ChildrenSearchDepth);
        }

        public override string ToString()
        {
            return ID;
        }
    }
}
