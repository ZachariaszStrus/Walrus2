using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Walrus2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Point MouseInitialPosition { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private ModelVisual3D GetDrawedGraph(Graph graph)
        {
            Model3DGroup model3DGroup = new Model3DGroup();

            model3DGroup.Children.Clear();
            double cubeSize = 10;
            foreach (var node in graph.Nodes)
            {
                GeometryModel3D cube = Graphics3D.GetCube(node.Value.Position,
                                                                new Point3D(cubeSize, cubeSize, cubeSize),
                                                                Brushes.LimeGreen);
                model3DGroup.Children.Add(cube);
            }

            foreach (var edge in graph.Edges)
            {
                model3DGroup.Children.Add(Graphics3D.GetLine(edge.StartNode.Position, edge.EndNode.Position, 
                                                             Brushes.White, cubeSize / 50));
            }

            model3DGroup.Children.Add(Graphics3D.GetCube(new Point3D(0, 0, 0),
                                                                new Point3D(cubeSize, cubeSize, cubeSize),
                                                                Brushes.Red));

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

        public void DrawGraph(Graph graph)
        {
            Viewport3D viewport3D = new Viewport3D();
            viewport3D.Children.Add(GetDrawedGraph(graph));
            viewport3D.Children.Add(GetLight());
            viewport3D.Camera = GetCamera(graph.Nodes.Count*8);

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
            if (tabControl.SelectedItem != null)
            {
                TabItem selectedTab = tabControl.SelectedItem as TabItem;
                var viewport3D = selectedTab.Content as Viewport3D;
                if (e.LeftButton == MouseButtonState.Pressed && viewport3D != null)
                {
                    if (viewport3D.IsDescendantOf(this))
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
            }
        }

        private void Window_Wheel(object sender, MouseWheelEventArgs e)
        {
            PerspectiveCamera camera = (tabControl.SelectedContent as Viewport3D).Camera as PerspectiveCamera;
            camera.MoveWithMouseWheel(e.Delta, new Point3D(0, 0, 0));
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            int n = Convert.ToInt32(textBox1.Text);
            DrawGraph(new Graph(n, 1.0 / n));
        }

        private void CloseTab_Clicked(object sender, RoutedEventArgs e)
        {
            TabItem selectedTab = tabControl.SelectedItem as TabItem;
            if(selectedTab  != null)
            {
                tabControl.Items.Remove(selectedTab);
            }
        }

        private void CloseAllTabs_Clicked(object sender, RoutedEventArgs e)
        {
            tabControl.Items.Clear();
        }

        private void loadButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog fileDialog =
                new System.Windows.Forms.OpenFileDialog();
            fileDialog.ShowDialog();

            string selectedFile = fileDialog.FileName;
            if (File.Exists(selectedFile))
            {
                DrawGraph(new Graph(selectedFile));
            }
        }
    }
}
