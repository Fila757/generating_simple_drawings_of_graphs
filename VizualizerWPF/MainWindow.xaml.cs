﻿using Microsoft.VisualBasic.CompilerServices;
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

        Brush[] colors = new Brush[] {Brushes.Red, Brushes.Orange, Brushes.Yellow, Brushes.LightGreen, Brushes.ForestGreen,
            Brushes.LightSkyBlue, Brushes.Blue, Brushes.DarkBlue, Brushes.Purple, Brushes.Pink };


        public MainWindow()
        {
            foreach (StateCalculation state in Enum.GetValues(typeof(StateCalculation)))
                statesCalculation[state] = false;

            InitializeComponent();

            //MessageBox.Show((new Point { X = 1, Y = 2 }.GetHashCode() == new Point { X = 1, Y = 2 }.GetHashCode()).ToString());
            //Dictionary<Vertex, bool> temp = new Dictionary<Vertex, bool>();
            //temp.Add(new Vertex(new Ellipse {Width = 50, Height = 50, Stroke = Brushes.Yellow}, new Point { X = 1, Y = 2 }, VertexState.Regular), true);
            //temp.Add(new Vertex(new Ellipse(), new Point { X = 1, Y = 2 }, VertexState.Regular), false);

      

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

            mainCanvas.Children.Clear();

            var vertices = new HashSet<Vertex>();
            foreach (var vertex in graphCoordinates.vertices)
            {
                Vertex vertexTemp = vertex;
                vertexTemp.center = vertexTemp.center.Scale(scale);

                vertexTemp.ellipse.Margin = new Thickness(vertexTemp.center.X - scale * sizeOfVertex / 2, vertexTemp.center.Y - scale * sizeOfVertex / 2, 0, 0);

                vertexTemp.ellipse.Height *= scale;
                vertexTemp.ellipse.Width *= scale;

               mainCanvas.Children.Add(vertexTemp.ellipse);
               vertices.Add(vertexTemp);
            }

            graphCoordinates.vertices = vertices;

            foreach (var edge in graphCoordinates.edges)
            {

                foreach (var line in edge.lines)
                {
                    line.X1 *= scale;
                    line.Y1 *= scale;
                    line.X2 *= scale;
                    line.Y2 *= scale;
                    line.StrokeThickness *= scale;

                    mainCanvas.Children.Add(line);
                }

                var pointsTemp = new List<Point>();
                foreach(var point in edge.points)
                {
                    pointsTemp.Add(point.Scale(scale));
                }

                edge.points = pointsTemp; 
            }

            var neighborsTemp = new Dictionary<Vertex, List<Vertex>>();
            foreach (var (vertex, listOfVertices) in graphCoordinates.neighbors)
            {
                Vertex vertexTemp = vertex;
                vertexTemp.center = vertexTemp.center.Scale(scale);

                graphCoordinates.vertices.TryGetValue(vertexTemp, out vertexTemp);

                List<Vertex> listTemp = new List<Vertex>();
                foreach (var el in listOfVertices)
                {
                    Vertex vertexTemp2 = el;
                    vertexTemp2.center = vertexTemp2.center.Scale(scale);

                    graphCoordinates.vertices.TryGetValue(vertexTemp2, out vertexTemp2);
                    listTemp.Add(vertexTemp2);
                }

                neighborsTemp.Add(vertexTemp, listTemp);
            }

            graphCoordinates.neighbors = neighborsTemp;
        }

        private void ResizeWindowEvent(object sender, EventArgs e)
        {
            if(WindowState == WindowState.Normal)
            {
                RedrawGraph(graphCoordinates, 1 / scale);
                cx = cx / scale; cy = cy / scale; sizeOfVertex = sizeOfVertex / scale;

            }

            if (WindowState == WindowState.Maximized)
            {
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

        private void DeleteEdgeFromDictionary(Vertex from, Vertex to)
        {
            if (graphCoordinates.neighbors.ContainsKey(from))
                graphCoordinates.neighbors[from].Remove(to);
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

        private (Vertex, Vertex) FindEdgeEnds(Edge edge)
        {

            var first = new Vertex();
            var second = new Vertex();

            bool firstBool = true;

            foreach(var vertex in graphCoordinates.vertices)
            {
                if (vertex.state == VertexState.Regular &&
                    CollisionDetection.CenterOfEllipseOnLine(edge.lines[0], vertex.ellipse) && firstBool)
                {
                    first = vertex;
                    firstBool = false;
                }

                else if (vertex.state == VertexState.Regular &&
                    CollisionDetection.CenterOfEllipseOnLine(edge.lines.Last(), vertex.ellipse) && !firstBool)
                    second = vertex;
            }

            return (first, second);
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
                        new List<Point> { selectedVertices[0].center, selectedVertices[1].center },
                        new List<Line> { line }
                    ));

                    graphCoordinates.AddToDictionary(selectedVertices[0], selectedVertices[1]);
                    graphCoordinates.AddToDictionary(selectedVertices[1], selectedVertices[0]);

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
                    var (start, end) = FindEdgeEnds(tempEdge);

                    DeleteEdgeFromDictionary(start, end);
                    DeleteEdgeFromDictionary(end, start);

                    foreach(var l in tempEdge.lines)
                    {
                        RemoveIntersections(l);
                        mainCanvas.Children.Remove(l);
                    }

                    graphCoordinates.edges.Remove(tempEdge);
                }

                /* removing vertex */
                mainCanvas.Children.Remove(ellipse);
                graphCoordinates.neighbors.Remove(FindVertex(ellipse));
                graphCoordinates.vertices.Remove(FindVertex(ellipse));
            }

            textblock.Text = ReCalculateKEdges().ToString();
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
            {
                mainCanvas.Children.Remove(ellipse);
                graphCoordinates.vertices.Remove(FindVertex(ellipse));
            }
        }

        private void line_MouseDown(object sender, RoutedEventArgs e)
        {
            Line line = sender as Line;

            //MessageBox.Show(line.ToString());

            if (stateChanging == StateChanging.Removing)
            {
                var tempEdge = FindEdge(line);
                var (start, end) = FindEdgeEnds(tempEdge);

                DeleteEdgeFromDictionary(start, end);
                DeleteEdgeFromDictionary(end, start);

                foreach (var l in tempEdge.lines)
                {
                    RemoveIntersections(l);

                    mainCanvas.Children.Remove(l);
                }

                graphCoordinates.edges.Remove(tempEdge);

            }

            textblock.Text = ReCalculateKEdges().ToString();
        }

        private double Determinant(Vector a, Vector b)
        {
            return a.X * b.Y - a.Y * b.X;
        }

        
        private Edge FindEdgeFromVertices(Vertex a, Vertex b)
        {
            foreach(var edge in graphCoordinates.edges)
            {
                if ((CollisionDetection.CenterOfEllipseOnLine(edge.lines[0], a.ellipse) || CollisionDetection.CenterOfEllipseOnLine(edge.lines.Last(), a.ellipse)) 
                    &&
                    (CollisionDetection.CenterOfEllipseOnLine(edge.lines[0], b.ellipse) || CollisionDetection.CenterOfEllipseOnLine(edge.lines.Last(), b.ellipse)))
                {
                    return edge;
                }
            }

            throw new ArgumentException("No such an edge between vertices");
        }

        private int ReCalculateKEdges()
        {
            int kEdgesWithGivenValue = 0;
            int kEdgesPicked = (int)KhranyUpDown.Value;

            foreach(var (from, value) in graphCoordinates.neighbors)
            {
                foreach (var to in value)
                {
                    int sum = 0;
                    foreach (var third in graphCoordinates.neighbors[to])
                    {
                        if(graphCoordinates.neighbors[from].Contains(third)){
                            if (third == from || third == to) continue;
                            if (Determinant(to.center.Substract(from.center).ToVector(), third.center.Substract(to.center).ToVector()) >= 0)
                            {
                                sum++;
                            }
                        }
                    }

                    int inter;
                    if (sum > (inter = graphCoordinates.neighbors[from].Intersect(graphCoordinates.neighbors[to]).Count()) / 2)
                        sum = inter - sum; //pick smaller

                    if (sum == kEdgesPicked)
                        kEdgesWithGivenValue++;

                    var edge = FindEdgeFromVertices(from, to);

                    foreach (var line in edge.lines)
                        line.Stroke = colors[sum];
                }
            }


            RedrawGraph(graphCoordinates, 1);
            return kEdgesWithGivenValue;
            
        }
        

        private void canvas_MouseDown(object sender, RoutedEventArgs e)
        {

           //MessageBox.Show(Mouse.GetPosition(sender as IInputElement).ToString());
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

                var vertex = new Vertex(ellipse, new Point { X = pos.X, Y = pos.Y }, VertexState.Regular);
                graphCoordinates.vertices.Add(vertex);
                graphCoordinates.neighbors.Add(vertex, new List<Vertex>());
            }

           
        }

        void DrawGraph(GraphCoordinates graphCoordinates, double scale)
        {
            //uniqueness of vertices
            //graphCoordinates.vertices = graphCoordinates.vertices.Distinct().ToList();

            var vertices = new HashSet<Vertex>();
            foreach(var vertex in graphCoordinates.vertices)
            {

                var vertexTemp = vertex;

                var coordinates = vertexTemp.center.Scale(scale).Add(new Point(cx + sizeOfVertex / 2, cy + sizeOfVertex / 2));
                vertexTemp.center = coordinates;

                var ellipse = new Ellipse
                {
                    Width = sizeOfVertex,
                    Height = sizeOfVertex,
                    Fill = vertexTemp.state == VertexState.Regular ? Brushes.Blue : Brushes.Green,
                    Margin = new Thickness(vertexTemp.center.X - sizeOfVertex / 2, vertexTemp.center.Y - sizeOfVertex / 2, 0, 0),
                    Visibility = vertexTemp.state == VertexState.Intersection && !statesCalculation[StateCalculation.Intersections] ? Visibility.Hidden : Visibility.Visible

                };

                ellipse.MouseDown += ellipse_MouseDown;
                mainCanvas.Children.Add(ellipse);

                Panel.SetZIndex(ellipse, vertexTemp.state == VertexState.Regular ? 100 : 10);

                vertexTemp.ellipse = ellipse;

                vertices.Add(vertexTemp);
            }

            graphCoordinates.vertices = vertices;
            foreach(var edge in graphCoordinates.edges)
            {

                var tempPoints = new List<Point>();
                foreach (var point in edge.points)
                {
                    tempPoints.Add(point.Scale(scale).Add(new Point(cx + (sizeOfVertex / 2), cy + (sizeOfVertex / 2)))); //first scale, then add
                }

                edge.points = tempPoints;

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

                edge.lines = tempLines;

            }

            
            var neighborsTemp = new Dictionary<Vertex, List<Vertex> >();
            foreach(var (vertex, listOfVertices) in graphCoordinates.neighbors)
            {
                Vertex vertexTemp = vertex;
                var coordinates = vertexTemp.center.Scale(scale).Add(new Point(cx + sizeOfVertex / 2, cy + sizeOfVertex / 2));
                vertexTemp.center = coordinates;

                graphCoordinates.vertices.TryGetValue(vertexTemp, out vertexTemp);

                List<Vertex> listTemp = new List<Vertex>();
                foreach(var el in listOfVertices)
                {
                    Vertex vertexTemp2 = el;
                    var coordinates2 = vertexTemp2.center.Scale(scale).Add(new Point(cx + sizeOfVertex / 2, cy + sizeOfVertex / 2));
                    vertexTemp2.center = coordinates2;

                    graphCoordinates.vertices.TryGetValue(vertexTemp2, out vertexTemp2);
                    listTemp.Add(vertexTemp2);
                }

                neighborsTemp.Add(vertexTemp, listTemp);
            }

            graphCoordinates.neighbors = neighborsTemp;
            
        }
       
    }

}
