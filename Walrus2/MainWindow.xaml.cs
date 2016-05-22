using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace Walrus2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Dictionary<TabItem, Graph> Graphs;

        public Point MouseInitialPosition { get; set; }

        public Node SelectedNode { get; set; }
        
        public TabItem SelectedTab
        {
            get
            {
                return tabControl.SelectedItem as TabItem;
            }
        }

        public Grid SelectedGrid
        {
            get
            {
                return SelectedTab != null ? SelectedTab.Content as Grid : null;
            }
        }

        public Viewport3D SelectedViewport3D
        {
            get
            {
                return SelectedGrid != null ? SelectedGrid.Children[0] as Viewport3D : null;
            }
        }

        public Slider SelectedRadiusSlider
        {
            get
            {
                return SelectedGrid.Children[1] as Slider;
            }
        }

        public Slider SelectedAngleSlider
        {
            get
            {
                return SelectedGrid.Children[2] as Slider;
            }
        }

        public Graph SelectedGraph
        {
            get
            {
                return SelectedTab != null ? Graphs[SelectedTab] : null;
            }
        }



        public MainWindow()
        {
            InitializeComponent();
            Graphs = new Dictionary<TabItem, Graph>();
        }
        
        // drawing graph 

        private ModelVisual3D GetDrawedGraph(Graph graph)
        {
            ModelVisual3D modelVisual = new ModelVisual3D();
            modelVisual.Content = graph.GeometryModelGroup;
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
            viewport3D.ContextMenu = TryFindResource("GraphContextMenu") as ContextMenu;
            viewport3D.Margin = new Thickness(0, 0, 0, 55);
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
            
            tabControl.Items.Add(newTab);
            tabControl.SelectedItem = newTab;

            Graphs.Add(newTab, graph);
            SelectedNode = graph.Root;
        }
        

        // moving camera 

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (SelectedTab != null && e.LeftButton == MouseButtonState.Pressed)
            {
                if (SelectedViewport3D.IsDescendantOf(this))
                {
                    Point currentMousePosition = e.GetPosition(SelectedViewport3D);
                    if (currentMousePosition.X > 0 &&
                        currentMousePosition.Y > 0 &&
                        currentMousePosition.X < SelectedViewport3D.ActualWidth &&
                        currentMousePosition.Y < SelectedViewport3D.ActualHeight &&
                        !SelectedRadiusSlider.IsMouseCaptureWithin && !SelectedAngleSlider.IsMouseCaptureWithin)
                    {
                        PerspectiveCamera camera = SelectedViewport3D.Camera as PerspectiveCamera;
                        Vector delta = currentMousePosition - MouseInitialPosition;
                        if(delta.Length < 50)
                            camera.RotateWithMouse(delta);
                    }
                    MouseInitialPosition = currentMousePosition;
                }
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (SelectedViewport3D != null)
            {
                MouseInitialPosition = e.GetPosition(SelectedViewport3D);
                if(e.ClickCount == 2)
                {
                    SelectNode();
                    PerspectiveCamera camera = SelectedViewport3D.Camera as PerspectiveCamera;
                    camera.LookDirection = SelectedNode.Position - camera.Position;
                }
            }
        }

        private void Window_Wheel(object sender, MouseWheelEventArgs e)
        {
            if(SelectedTab != null)
            {
                PerspectiveCamera camera = SelectedViewport3D.Camera as PerspectiveCamera;
                camera.MoveWithMouseWheel(e.Delta, new Point3D(0, 0, 0));
            }
        }
        

        // changing angle and radius 

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (SelectedTab != null)
            {
                SelectedGraph.AdjustGraph(SelectedRadiusSlider.Value, SelectedAngleSlider.Value);
                PerspectiveCamera camera = SelectedViewport3D.Camera as PerspectiveCamera;
                camera.LookDirection = SelectedNode.Position - camera.Position;
            }
        }


        // open graph

        private void OpenFileMenuItem_Click(object sender, RoutedEventArgs e)
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
        


        // selecting point

        private void GraphContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            SelectNode();
            var graphContextMenu = TryFindResource("GraphContextMenu") as ContextMenu;
            if (SelectedNode.Children.Count == 0)
            {
                (graphContextMenu.Items[0] as MenuItem).Header = "ID : " + SelectedNode.ID;
                foreach (var item in graphContextMenu.Items)
                {
                    var menuItem = item as MenuItem;
                    if (menuItem != null)
                        menuItem.Visibility = Visibility.Collapsed;
                }
                (graphContextMenu.Items[4] as MenuItem).Visibility = Visibility.Visible;
                (graphContextMenu.Items[0] as MenuItem).Visibility = Visibility.Visible;
            }
            else
            {
                (graphContextMenu.Items[0] as MenuItem).Header = "ID : " + SelectedNode.ID;
                (graphContextMenu.Items[1] as MenuItem).Header = "Children : " + SelectedNode.Children.Count;
                foreach (var item in graphContextMenu.Items)
                {
                    var menuItem = item as MenuItem;
                    if (menuItem != null)
                        menuItem.Visibility = Visibility.Visible;
                }
            }
        }

        private void SelectNode()
        {
            Point mouse_pos = MouseInitialPosition;
            HitTestResult result = VisualTreeHelper.HitTest(SelectedViewport3D, mouse_pos);
            RayMeshGeometry3DHitTestResult mesh_result =
                result as RayMeshGeometry3DHitTestResult;

            if (mesh_result != null)
            {
                Node closestNode = null;
                double delta = 0;
                foreach (var node in SelectedGraph.Nodes.Values)
                {
                    if (!node.IsVisible) continue;

                    if (closestNode == null)
                    {
                        closestNode = node;
                        delta = (mesh_result.PointHit - node.Position).Length;
                    }
                    else if ((mesh_result.PointHit - node.Position).Length < delta)
                    {
                        closestNode = node;
                        delta = (mesh_result.PointHit - node.Position).Length;
                    }
                }

                SelectedNode = closestNode;
                
            }
        }


        // expanding/collapsing

        private void CollapseDescendantsContextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SelectedGraph.CollapseDescendants(SelectedNode);
        }

        private void ExpandDescendantsContextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SelectedGraph.CollapseDescendants(SelectedNode);
            SelectedGraph.ExpandDescendants(SelectedNode);
            SelectedGraph.RecursiveSphereAlgorithm(SelectedNode, SelectedRadiusSlider.Value, SelectedAngleSlider.Value);
        }

        private void ExpandChildrenContextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SelectedGraph.CollapseDescendants(SelectedNode);
            SelectedGraph.ExpandChildren(SelectedNode);
            SelectedGraph.RecursiveSphereAlgorithm(SelectedNode, SelectedRadiusSlider.Value, SelectedAngleSlider.Value);
        }


        // changing root 

        private void ResetRootContextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedTab != null)
            {
                if (SelectedGraph.DefaultRoot != SelectedGraph.Root)
                {
                    SelectedRadiusSlider.Value = 1;
                    SelectedAngleSlider.Value = 1;
                    SelectedGraph.ResetRoot();
                    SelectedNode = SelectedGraph.Root;
                    PerspectiveCamera camera = SelectedViewport3D.Camera as PerspectiveCamera;
                    camera.LookDirection = new Point3D() - camera.Position;
                }
            }
        }

        private void SetRootContextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            TabItem SelectedTab = tabControl.SelectedItem as TabItem;
            if (SelectedTab != null)
            {
                if (SelectedNode != SelectedGraph.Root)
                {
                    SelectedRadiusSlider.Value = 1;
                    SelectedAngleSlider.Value = 1;
                    SelectedGraph.SetRoot(SelectedNode);
                    PerspectiveCamera camera = SelectedViewport3D.Camera as PerspectiveCamera;
                    camera.LookDirection = new Point3D() - camera.Position;
                }
            }
        }
    }
}
