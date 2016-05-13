using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public Dictionary<TabItem, Graph> Graphs;

        public Point MouseInitialPosition { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Graphs = new Dictionary<TabItem, Graph>();
        }

        private void RecursiveGraphDrawing(Model3DGroup model3DGroup, Node node)
        {
            model3DGroup.Children.Add(node.GeometryModel);

            foreach (var edge in node.Edges.Values)
            {
                model3DGroup.Children.Add(edge.GeometryModel);
            }
            node.Children.ForEach(c => RecursiveGraphDrawing(model3DGroup, c));
        }

        private ModelVisual3D GetDrawedGraph(Graph graph)
        {
            Model3DGroup model3DGroup = new Model3DGroup();
            model3DGroup.Children.Add(graph.Root.GeometryModel);

            RecursiveGraphDrawing(model3DGroup, graph.Root);
            
            ModelVisual3D modelVisual = new ModelVisual3D();
            modelVisual.Content = model3DGroup;
            return modelVisual;
        }

        private ModelVisual3D GetLight()
        {
            ModelVisual3D lightModel = new ModelVisual3D();
            Model3DGroup group = new Model3DGroup();
            
            group.Children.Add(new DirectionalLight(Colors.White, new Vector3D(-2, -2, -2)));
            group.Children.Add(new DirectionalLight(Colors.White, new Vector3D(-2, -2, -2)));
            group.Children.Add(new DirectionalLight(Colors.White, new Vector3D(2, 2, 2)));
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
            Grid content = new Grid();

            Viewport3D viewport3D = new Viewport3D();
            viewport3D.Margin = new Thickness(0, 0, 0, 50);
            viewport3D.Children.Add(GetDrawedGraph(graph));
            viewport3D.Children.Add(GetLight());
            viewport3D.Camera = GetCamera(Math.Sqrt(graph.Root.TotalChildren()) * 20);
            content.Children.Add(viewport3D);

            Slider radiusSlider = new Slider();
            radiusSlider.Margin = new Thickness(0, 0, 0, 30);
            radiusSlider.VerticalAlignment = VerticalAlignment.Bottom;
            radiusSlider.Value = 1;
            radiusSlider.Maximum = 5;
            radiusSlider.Minimum = 0.1;
            radiusSlider.ValueChanged += slider_ValueChanged;
            content.Children.Add(radiusSlider);

            Slider angleSlider = new Slider();
            angleSlider.Margin = new Thickness(0, 0, 0, 5);
            angleSlider.VerticalAlignment = VerticalAlignment.Bottom;
            angleSlider.Value = 1;
            angleSlider.Maximum = 5;
            angleSlider.Minimum = 0.1;
            angleSlider.ValueChanged += slider_ValueChanged;
            content.Children.Add(angleSlider);

            TabItem newTab = new TabItem();
            newTab.Header = graph.Name;
            newTab.Content = content;
            
            Graphs.Add(newTab, graph);

            tabControl.Items.Add(newTab);
            tabControl.SelectedItem = newTab;
        }

        // controls ---------------------------------------------------------------------
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                MouseInitialPosition = e.GetPosition(sender as IInputElement);
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (tabControl.SelectedItem != null)
            {
                TabItem selectedTab = tabControl.SelectedItem as TabItem;
                var grid = selectedTab.Content as Grid;
                var viewport3D = grid.Children[0] as Viewport3D;
                var radiusSlider = grid.Children[1] as Slider;
                var angleSlider = grid.Children[2] as Slider;
                if (e.LeftButton == MouseButtonState.Pressed && viewport3D != null)
                {
                    if (viewport3D.IsDescendantOf(this))
                    {
                        Point relativePoint = viewport3D.TransformToAncestor(this).Transform(new Point(0, 0));
                        Point currentMousePosition = e.GetPosition(sender as IInputElement);
                        if (currentMousePosition.X > relativePoint.X && 
                            currentMousePosition.Y > relativePoint.Y + 20 &&
                            !radiusSlider.IsMouseCaptureWithin && !angleSlider.IsMouseCaptureWithin)
                        {
                            PerspectiveCamera camera = viewport3D.Camera as PerspectiveCamera;
                            camera.RotateWithMouse(MouseInitialPosition, currentMousePosition, new Point3D(0, 0, 0));
                            MouseInitialPosition = e.GetPosition(sender as IInputElement);
                        }
                    }
                }
            }
        }

        private void Window_Wheel(object sender, MouseWheelEventArgs e)
        {
            var grid = tabControl.SelectedContent as Grid;
            var viewport3D = grid.Children[0] as Viewport3D;
            PerspectiveCamera camera = viewport3D.Camera as PerspectiveCamera;
            camera.MoveWithMouseWheel(e.Delta, new Point3D(0, 0, 0));
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
                Graph graph = new Graph(selectedFile);
                DrawGraph(graph);
            }
        }  

        private void createButton_Click(object sender, RoutedEventArgs e)
        {
            string tmpFile = "tmp.txt";
            string graphStr = textBox.Text.Replace('\r', ' ');
            using (StreamWriter sw = new StreamWriter(tmpFile))
            {
                sw.Write(graphStr);
            }
            
            Graph graph = new Graph("tmp.txt");

            DrawGraph(graph);
            
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TabItem selectedTab = tabControl.SelectedItem as TabItem;
            var grid = selectedTab.Content as Grid;
            var radiusSlider = grid.Children[1] as Slider;
            var angleSlider = grid.Children[2] as Slider;
            if (selectedTab != null)
            {
                Graph graph = Graphs[selectedTab];
                graph.ChangeChildrenAngle(radiusSlider.Value, angleSlider.Value);
            }
        }
    }
}
