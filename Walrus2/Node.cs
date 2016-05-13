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

        private int _totalChildren;

        public const int ChildrenSearchDepth = 10;

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
        public GeometryModel3D GeometryModel { get; private set; }
        
        public Point3D Dimensions { get; private set; }

        public SolidColorBrush ModelColor { get; private set; }
        // ---------------------------------------

        public Node()
        {
            _totalChildren = 0;
            Dimensions = new Point3D(10, 10, 10);
            ModelColor = Brushes.LimeGreen;

            Parent = null;
            Children = new List<Node>();
            Edges = new Dictionary<Node, Edge>();
            
            Position = new Point3D();
        }

        private void RefreshGeometryModel()
        {
            if(GeometryModel != null)
            {
                Graphics3D.RefreshCubeGeometryModel(GeometryModel, Position, Dimensions);
            }
            else
            {
                GeometryModel = Graphics3D.GetCube(Position, Dimensions, ModelColor);
            }
        }

        private void RefreshEdge(Node child)
        {
            Edges[child].EndNode = child;
        }

        public void AddChild(Node child)
        {
            Children.Add(child);
            Edges.Add(child, new Edge(this, child));
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

        public void SetParents()
        {
            foreach (var child in Children)
            {
                child.Parent = this;
            }
        }

        public int CompareTo(Node other)
        {
            return other.TotalChildren(ChildrenSearchDepth) - TotalChildren(ChildrenSearchDepth);
        }
    }
}
