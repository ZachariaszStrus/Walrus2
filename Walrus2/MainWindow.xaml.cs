using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Walrus2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Point mouseInitialPosition;
        
        public Point MouseInitialPosition
        {
            get
            {
                return mouseInitialPosition;
            }

            set
            {
                mouseInitialPosition = value;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private ModelVisual3D GetDrawedGraph(Graph graph)
        {
            Model3DGroup model3DGroup = new Model3DGroup();

            model3DGroup.Children.Clear();
            double cubeSize = Math.Sqrt(graph.Size);
            for (int i = 0; i < graph.Size; i++)
            {
                model3DGroup.Children.Add(Graphics3D.GetCube(graph.Positions[i], new Point3D(cubeSize, cubeSize, cubeSize), Brushes.LimeGreen));
            }

            for (int y = 0; y < graph.Size; y++)
            {
                for (int x = y + 1; x < graph.Size; x++)
                {
                    if (graph.NeighbourMatrix[y,x] == true)
                    {
                        model3DGroup.Children.Add(Graphics3D.GetLine(graph.Positions[y], graph.Positions[x], Brushes.Yellow, cubeSize/50));
                    }
                }
            }

            ModelVisual3D modelVisual = new ModelVisual3D();
            modelVisual.Content = model3DGroup;
            return modelVisual;
        }

        private ModelVisual3D GetLight()
        {
            ModelVisual3D lightModel = new ModelVisual3D();
            Model3DGroup group = new Model3DGroup();

            group.Children.Add(new DirectionalLight(Colors.White, new Vector3D(-2, -2, -2)));
            group.Children.Add(new DirectionalLight(Colors.White, new Vector3D(2, 2, 2)));

            lightModel.Content = group;
            return lightModel;
        }

        private PerspectiveCamera GetCamera(double radius)
        {
            PerspectiveCamera camera = new PerspectiveCamera();
            double x = Math.Sqrt(radius*radius/3);
            camera.Position = new Point3D(x, x, x);
            camera.LookDirection = new Vector3D(-x, -x, -x);
            camera.UpDirection = new Vector3D(0, 0, 1);

            return camera;
        }

        public void LoadGraph()
        {
            int n = Convert.ToInt32(textBox1.Text);
            Graph graph = new Graph(n, n * 2, 1.0 / n);

            Viewport3D viewport3D = new Viewport3D();
            viewport3D.Children.Add(GetDrawedGraph(graph));
            viewport3D.Children.Add(GetLight());
            viewport3D.Camera = GetCamera(n*8);

            TabItem newTab = new TabItem();
            newTab.Header = "Graph " + Convert.ToString(tabControl.Items.Count);
            newTab.Content = viewport3D;

            tabControl.Items.Add(newTab);
            tabControl.SelectedItem = newTab;
        }

        // controls ---------------------------------------------------------------------
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MouseInitialPosition = e.GetPosition(sender as IInputElement);
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            TabItem selectedTab = tabControl.SelectedItem as TabItem;
            var viewport3D = selectedTab.Content as Viewport3D;
            if (e.LeftButton == MouseButtonState.Pressed && viewport3D != null)
            {
                Point relativePoint = viewport3D.TransformToAncestor(this).Transform(new Point(0, 0));
                Point currentMousePosition = e.GetPosition(sender as IInputElement);
                if (currentMousePosition.X > relativePoint.X && currentMousePosition.Y > relativePoint.Y)
                {
                    PerspectiveCamera camera = viewport3D.Camera as PerspectiveCamera;
                    camera.RotateWithMouse(MouseInitialPosition, currentMousePosition, new Point3D(0, 0, 0));
                    MouseInitialPosition = e.GetPosition(sender as IInputElement);
                    textBox.Text = MouseInitialPosition.ToString();
                }
            }
        }
        private void Window_Wheel(object sender, MouseWheelEventArgs e)
        {
            PerspectiveCamera camera = (tabControl.SelectedContent as Viewport3D).Camera as PerspectiveCamera;
            camera.MoveWithMouseWheel(e.Delta, new Point3D(0, 0, 0));
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            LoadGraph();
        }

    }
}
