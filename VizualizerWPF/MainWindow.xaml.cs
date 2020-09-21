using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.Windows.Shapes.Path;

namespace VizualizerWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    enum StateChanging { Adding, Removing, None };
    enum StateCalculation { Intersections, KEdges, AMKEdges, AMAMKEdges, AMAMAMKEdges}; //AM = AtMost

    public partial class MainWindow : Window
    {
        StateChanging stateChanging = StateChanging.None;
        Dictionary<StateCalculation, bool> statesCalculation = new Dictionary<StateCalculation, bool>();

        GraphGenerator graphGenerator;

        double cx, cy;
        public double sizeOfVertex;
        double scale;

        //Dictionary<Line, List<Line>> edges = new Dictionary<Line, List<Line> >();
        //Dictionary<Ellipse, List<Ellipse> > vertices = new Dictionary<Ellipse, List<Ellipse> >();

        List<Vertex> selectedVertices = new List<Vertex>();

        GraphCoordinates graphCoordinates = new GraphCoordinates();


        public MainWindow()
        {
            foreach (StateCalculation state in Enum.GetValues(typeof(StateCalculation)))
                statesCalculation[state] = false;

            InitializeComponent();

            Point[] points1 = {
                new Point(60, 30),
                new Point(200, 130),
                new Point(100, 150),
                new Point(200, 50),
            };
            Path path1 = PolybezierPathMaker.MakeCurve(points1, 0);
            
            path1.Stroke = Brushes.LightGreen;
            path1.StrokeThickness = 5;
            mainCanvas.Children.Add(path1);

            Path path2 = PolybezierPathMaker.MakeCurve(new Point[2] { new Point(50, 50), new Point(122, 122) }, 0);
            path2.Stroke = Brushes.LightCoral;
            path2.StrokeThickness = 5;
            mainCanvas.Children.Add(path2);

            //MessageBox.Show(CollisionDetection.TwoPaths(path1, path2).ToString());

            StateChanged += new EventHandler(ResizeWindowEvent);

            cx = 300; cy = 200;
            sizeOfVertex = 15;
            scale = 1.5;

            CollisionDetection.Init(this);

            graphGenerator = new GraphGenerator((int)NextDrawingUpDown.Value);
            graphCoordinates = graphGenerator.GenerateNextDrawing();
            DrawGraph(graphCoordinates, 1);

        }

        private void RedrawGraph(GraphCoordinates graphCoordinates, double scale)
        {
            var verticesTemp = new List<Vertex>();
            foreach (var vertex in graphCoordinates.vertices)
            {
                var vertexTemp = vertex;
                vertexTemp.center = new Point { X = vertex.center.X * scale,
                    Y = vertex.center.Y * scale};

                vertexTemp.ellipse.Margin = new Thickness(vertexTemp.center.X - scale * sizeOfVertex / 2, vertexTemp.center.Y - scale * sizeOfVertex / 2, 0, 0);

                vertexTemp.ellipse.Height *= scale;
                vertexTemp.ellipse.Width *= scale;

                verticesTemp.Add(vertexTemp);

                mainCanvas.Children.Add(vertexTemp.ellipse);
            }

            graphCoordinates.vertices = verticesTemp;

            foreach (var edge in graphCoordinates.edges)
            {
                var edgeTemp = edge;
                foreach (var line in edgeTemp.lines)
                {
                    line.X1 *= scale;
                    line.Y1 *= scale;
                    line.X2 *= scale;
                    line.Y2 *= scale;
                    line.StrokeThickness *= scale;

                    mainCanvas.Children.Add(line);
                }

                HashSet<Point> pointsTemp = new HashSet<Point>();
                foreach(var point in edgeTemp.points)
                {
                    pointsTemp.Add(point.Scale(scale));
                }

                edgeTemp.points = pointsTemp; 
            }
        }

        private void ResizeWindowEvent(object sender, EventArgs e)
        {
            if(WindowState == WindowState.Normal)
            {
                mainCanvas.Children.Clear();
                RedrawGraph(graphCoordinates, 1 / scale);
                cx = cx / scale; cy = cy / scale; sizeOfVertex = sizeOfVertex / scale;

            }

            if (WindowState == WindowState.Maximized)
            {
                mainCanvas.Children.Clear();
                RedrawGraph(graphCoordinates, scale);
                cx = cx * scale; cy = cy * scale; sizeOfVertex = sizeOfVertex * scale;

            }

        }

        private void Adding_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            /*changing color*/
            button.Background = stateChanging == StateChanging.Adding ?
                (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFDDDDDD")) : Brushes.Red;
            Removing.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFDDDDDD"));

            stateChanging = stateChanging == StateChanging.Adding ? StateChanging.None : StateChanging.Adding;
        }

        private void Removing_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            /*changing color*/
            button.Background = stateChanging == StateChanging.Removing ?
                (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFDDDDDD")) : Brushes.Red;
            Adding.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFDDDDDD"));

            stateChanging = stateChanging == StateChanging.Removing ? StateChanging.None : StateChanging.Removing;

        }

        private void Intersections_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            /*changing color*/
            button.Background = statesCalculation[StateCalculation.Intersections] ?
                (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFDDDDDD")) : Brushes.Red;

            statesCalculation[StateCalculation.Intersections] = !statesCalculation[StateCalculation.Intersections];

            if (!statesCalculation[StateCalculation.Intersections])
            {
                //textblock.Text = 
                foreach (var vertex in mainCanvas.Children.OfType<Ellipse>())
                {
                    if (vertex.Fill == Brushes.Green)
                        vertex.Visibility = Visibility.Hidden;
                }
            }
            else
            {
                foreach(var vertex in mainCanvas.Children.OfType<Ellipse>())
                {
                    if (vertex.Fill == Brushes.Green)
                        vertex.Visibility = Visibility.Visible;
                }
            }

        }

        private void Kedges_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            /*changing color*/
            button.Background = statesCalculation[StateCalculation.KEdges] ?
                (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFDDDDDD")) : Brushes.Red;

            statesCalculation[StateCalculation.KEdges] = !statesCalculation[StateCalculation.KEdges];
        }

        private void KedgesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void NextDrawing_Click(object sender, RoutedEventArgs e)
        {

            if (graphGenerator.SizeOfGraph != NextDrawingUpDown.Value)
            {
                graphGenerator.CloseFile();
                graphGenerator = new GraphGenerator((int)NextDrawingUpDown.Value);
            }
                
            graphCoordinates = graphGenerator.GenerateNextDrawing();

            mainCanvas.Children.Clear();
            DrawGraph(graphCoordinates, WindowState ==  WindowState.Maximized ? scale : 1);
        }

        private void DeleteEdgeFromDictionary(Edge edge)
        {
            foreach(var (key, _) in graphCoordinates.neighbors)
            {
                if (graphCoordinates.neighbors[key].Contains(edge))
                    graphCoordinates.neighbors[key].Remove(edge);
            }
        }

        private Vertex FindVertex(Ellipse ellipse)
        {
            foreach(var vertex in graphCoordinates.vertices)
            {
                if (vertex.ellipse == ellipse)
                    return vertex;
            }

            throw new ArgumentException("There is no such a vertex, containing ellipse");
        }

        private void ellipse_MouseDown(object sender, RoutedEventArgs e)
        {
            Ellipse ellipse = sender as Ellipse;
            //MessageBox.Show(new { x = ellipse.Margin.Left, y= ellipse.Margin.Top }.ToString());


            if (stateChanging == StateChanging.Adding)
            {
                selectedVertices.Add(new Vertex(ellipse, new Point { X = ellipse.Margin.Left + sizeOfVertex/2, Y = ellipse.Margin.Top + sizeOfVertex/2}, VertexState.Regular));
                if (selectedVertices.Count == 2)
                {
                    var line = new Line
                    {
                        X1 = selectedVertices[0].center.X,
                        Y1 = selectedVertices[0].center.Y,
                        X2 = selectedVertices[1].center.X,
                        Y2 = selectedVertices[1].center.Y,
                        Stroke = Brushes.Red,
                        StrokeThickness = sizeOfVertex / 3
                    };
                    line.MouseDown += line_MouseDown;
                    Panel.SetZIndex(line, 1);
                    mainCanvas.Children.Add(line);

                    graphCoordinates.edges.Add(new Edge(
                        new HashSet<Point> { selectedVertices[0].center, selectedVertices[1].center },
                        new List<Line> { line }
                    ));

                    graphCoordinates.AddToDictionary(selectedVertices[0], graphCoordinates.edges.Last());
                    graphCoordinates.AddToDictionary(selectedVertices[1], graphCoordinates.edges.Last());

                    selectedVertices.Clear();

                    List<Ellipse> intersections = new List<Ellipse>();
                    foreach (var l in mainCanvas.Children.OfType<Line>())
                    {
                        if (l == line) continue;

                        var intersection = CollisionDetection.TwoLines(line, l);
                        if(!(intersection.X == -1 && intersection.Y == -1))
                        {
                            var ellipse2 = new Ellipse
                            {
                                Width = sizeOfVertex,
                                Height = sizeOfVertex,
                                Fill = Brushes.Green,
                                Margin = new Thickness(intersection.X - sizeOfVertex / 2, intersection.Y - sizeOfVertex / 2, 0, 0),
                                Visibility = statesCalculation[StateCalculation.Intersections] ? Visibility.Visible : Visibility.Hidden
                            };
                            ellipse2.MouseDown += ellipse_MouseDown;
                            Panel.SetZIndex(ellipse2, 10);
                            intersections.Add(ellipse2);

                            graphCoordinates.vertices.Add(new Vertex(ellipse2, new Point { X = intersection.X, Y = intersection.Y }, VertexState.Intersection));

                           //MessageBox.Show(new { intersection.x,intersection.y }.ToString());
                        }
                    }

                    foreach (var el in intersections)
                        mainCanvas.Children.Add(el);
                }

            
            }

            if(stateChanging == StateChanging.Removing)
            {
                
                /* finding edges which intersected */
                List<Line> intersectedLines = new List<Line>();
                foreach(var line in mainCanvas.Children.OfType<Line>())
                {
                    if (CollisionDetection.CenterOfEllipseOnLine(line, ellipse)) // colision at the end can be used if it would not work
                        intersectedLines.Add(line);
                }

                /* removing edges */
                foreach (var line in intersectedLines) {

                    var tempEdge = FindEdge(line);
                    DeleteEdgeFromDictionary(tempEdge);

                    foreach(var l in tempEdge.lines)
                    {
                        RemoveIntersections(l);
                        mainCanvas.Children.Remove(l);
                    }
                }

                /* removing vertex */
                mainCanvas.Children.Remove(ellipse);

                graphCoordinates.neighbors.Remove(FindVertex(ellipse));
            }
        }


        private Edge FindEdge(Line line)
        {
            var tempEdge = new Edge();
            foreach (var edge in graphCoordinates.edges)
            {
                foreach (var l in edge.lines)
                {
                    if (l == line)
                    {
                        tempEdge = edge;
                    }
                }
            }
            return tempEdge;
        }

        private void RemoveIntersections(Line line)
        {
            var removedIntersections = new List<Ellipse>();
            foreach (var ellipse in mainCanvas.Children.OfType<Ellipse>())
            {
                if (CollisionDetection.LineAndEllipse(line, ellipse)
                    && ellipse.Fill == Brushes.Green)
                    removedIntersections.Add(ellipse);

            }

            foreach (var ellipse in removedIntersections)
                mainCanvas.Children.Remove(ellipse);
        }

        private void line_MouseDown(object sender, RoutedEventArgs e)
        {
            Line line = sender as Line;

            //MessageBox.Show(line.ToString());

            if (stateChanging == StateChanging.Removing)
            {
                var tempEdge = FindEdge(line);
                DeleteEdgeFromDictionary(tempEdge);

                foreach (var l in tempEdge.lines)
                {
                    RemoveIntersections(l);

                    mainCanvas.Children.Remove(l);
                }
            }
        }

        private double Determinant(Point a, Point b)
        {
            return a.X * b.Y - a.Y * b.X;
        }

        /*
        private int ReCalculateKEdges()
        {
            int sum = 0;
            foreach(var (from, _) in graphCoordinates.neighbors)
            {
                foreach(var (to, _) in graphCoordinates.neighbors)
                {
                    if(from != to)
                    {
                        foreach(var (third, _) in graphCoordinates.neighbors)
                        {
                            if()
                        }
                    }
                }
            }
        }
        */

        private void canvas_MouseDown(object sender, RoutedEventArgs e)
        {

            //textblock.Text = ReCalculateKEdges().ToString();

            if (e.OriginalSource is Ellipse || e.OriginalSource is Line)
                return;

            Point pos = Mouse.GetPosition(mainCanvas);
            if(stateChanging == StateChanging.Adding)
            {

                Ellipse ellipse = new Ellipse
                {
                    Width = sizeOfVertex,
                    Height = sizeOfVertex,
                    Fill = Brushes.Blue,
                    Margin = new Thickness(pos.X - sizeOfVertex / 2, pos.Y - sizeOfVertex / 2, 0, 0)
                };
                Panel.SetZIndex(ellipse, 100);
                ellipse.MouseDown += ellipse_MouseDown;
                mainCanvas.Children.Add(ellipse);

                graphCoordinates.vertices.Add(new Vertex(ellipse, new Point { X = pos.X, Y = pos.Y }, VertexState.Regular));
                    
            }

           
        }

        void DrawGraph(GraphCoordinates graphCoordinates, double scale)
        {
            //uniqueness of vertices
            graphCoordinates.vertices = graphCoordinates.vertices.Distinct().ToList();


            List<Vertex> graphCoordinatesTemp = new List<Vertex>();
            foreach(var vertex in graphCoordinates.vertices)
            {
                var coordinates = vertex.center.Scale(scale).Add(new Point(cx + sizeOfVertex / 2, cy + sizeOfVertex / 2));
                graphCoordinatesTemp.Add(new Vertex(vertex.ellipse, coordinates, vertex.state));
            }

            graphCoordinates.vertices = graphCoordinatesTemp;


            graphCoordinatesTemp = new List<Vertex>();
            foreach (var vertex in graphCoordinates.vertices)
            {
                //MessageBox.Show(vertex.ToString());
                var ellipse = new Ellipse
                {
                    Width = sizeOfVertex,
                    Height = sizeOfVertex,
                    Fill = vertex.state == VertexState.Regular ? Brushes.Blue : Brushes.Green,
                    Margin = new Thickness(vertex.center.X - sizeOfVertex / 2, vertex.center.Y - sizeOfVertex / 2, 0, 0),
                    Visibility = vertex.state == VertexState.Intersection && !statesCalculation[StateCalculation.Intersections] ? Visibility.Hidden : Visibility.Visible

                };
                ellipse.MouseDown += ellipse_MouseDown;
                mainCanvas.Children.Add(ellipse);

                Panel.SetZIndex(ellipse, vertex.state == VertexState.Regular ? 100 : 10);

                graphCoordinatesTemp.Add(new Vertex(ellipse, vertex.center, vertex.state));
            }

            graphCoordinates.vertices = graphCoordinatesTemp;

            var graphCoordinatesEdgesTemp = new GraphCoordinates();
            foreach(var edge in graphCoordinates.edges)
            {
                var edgeTemp = new Edge();

                HashSet<Point> tempPoints = new HashSet<Point>();
                foreach (var point in edge.points)
                {
                    tempPoints.Add(point.Scale(scale).Add(new Point(cx + (sizeOfVertex / 2), cy + (sizeOfVertex / 2)))); //first scale, then add
                }

                edgeTemp.points = tempPoints;

                List<Line> tempLines = new List<Line>();
                foreach (var line in edge.lines)
                {
                    var l = new Line
                    {
                        X1 = line.X1 * scale + cx + sizeOfVertex / 2,
                        Y1 = line.Y1 * scale + cy + sizeOfVertex / 2,
                        X2 = line.X2 * scale + cx + sizeOfVertex / 2,
                        Y2 = line.Y2 * scale + cy + sizeOfVertex / 2,
                        Stroke = Brushes.Red,
                        StrokeThickness = sizeOfVertex / 3
                    };
                    Panel.SetZIndex(l, 1);
                    l.MouseDown += line_MouseDown;
                    mainCanvas.Children.Add(l);

                    tempLines.Add(l);
                }

                edgeTemp.lines = tempLines;

                graphCoordinatesEdgesTemp.edges.Add(edgeTemp);
            }

            graphCoordinates.edges = graphCoordinatesEdgesTemp.edges;

        }
       
    }

}
