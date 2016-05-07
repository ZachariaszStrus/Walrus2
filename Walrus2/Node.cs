using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Walrus2
{
    public class Node : IComparable<Node>
    {
        private int _totalChildren;

        public const int ChildrenSearchDepth = 5;

        public Point3D Position { get; set; }

        public HashSet<Node> Children { get; set; }

        public Node Parent { get; set; }


        public Node()
        {
            Parent = null;
            Children = new HashSet<Node>();
            _totalChildren = 0;
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
