using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Walrus2
{
    public class Edge
    {
        public Node StartNode { get; set; }

        public Node EndNode { get; set; }


        public Edge(Node n1, Node n2)
        {
            StartNode = n1;
            EndNode = n2;
        }
    }
}
