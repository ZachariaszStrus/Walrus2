using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Walrus2
{
    public class Edge
    {
        private Node _startNode;
        public Node StartNode
        {
            get
            {
                return _startNode;
            }
            set
            {
                _startNode = value;
                RefreshGeometryModel();
            }
        }

        private Node _endNode;
        public Node EndNode
        {
            get
            {
                return _endNode;
            }
            set
            {
                _endNode = value;
                RefreshGeometryModel();
            }
        }

        // display -------------------------------
        public bool IsVisible { get; set; }

        public GeometryModel3D GeometryModel { get; private set; }

        public double Gauge { get; private set; }

        public SolidColorBrush ModelColor { get; private set; }
        // ---------------------------------------

        public Edge(Node n1, Node n2)
        {
            Gauge = 0.5;
            IsVisible = true;
            ModelColor = Brushes.White;

            StartNode = n1;
            EndNode = n2;
        }

        public void RefreshGeometryModel()
        {
            if(StartNode != null && EndNode != null)
            {
                if(GeometryModel != null)
                {
                    Graphics3D.RefreshLineGeometryModel(GeometryModel, StartNode.Position, EndNode.Position, Gauge);
                }
                else
                {
                    GeometryModel = Graphics3D.GetLine(StartNode.Position, EndNode.Position, ModelColor, Gauge);
                }
            }
        }
    }
}
