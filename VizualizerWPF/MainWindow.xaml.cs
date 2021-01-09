using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Threading;
using Path = System.Windows.Shapes.Path;

/// <summary>
/// In this project Syncfusion.WPF nuget is required because Syncfusion UpDown is used
/// Also english version of file system is assumed because decimal dots are used (25.4)
/// </summary>

namespace VizualizerWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    /// <summary>
    /// In MainWindow.xaml we have one <c>mainCanvas</c>
    /// Also 5 buttons
    /// Přidávání - adding mode 
    /// Mazání - removing mode
    /// Průsečíky - vizualizing intersections
    /// K hrany - vizualizing counting k edges
    /// Další jednoduché nakreslení - generate next good drawing of clique
    /// 
    /// Also consists of ListBox to choose which AM edges we want
    /// Finally there to UpDowns to select which k edges we want
    /// and how big clique is chosen
    /// 
    /// At the bottom there is a legend to know
    /// which color describe which number for k edges
    /// </summary>


    enum StateAutoMoving { Auto, None };

    enum StateChanging { AddingPolyline, Adding, Removing, Invariant, None };

    /// <summary>
    /// Enum for showing Intersection, KEdges, AtMostKEdge...
    /// </summary>
    public enum StateCalculation { Intersections, KEdges, AMKEdges, AMAMKEdges, AMAMAMKEdges}; //AM = AtMost

    public partial class MainWindow : Window
    {
        /// <summary>
        /// <param name="cx">Shift from 0,0 origin</param>
        /// <param name="cy">Shift from 0,0 origin</param>
        /// <param name="stateChanging">For recognizing in which state we are</param>
        /// <param name="graphGenerator">It is class for generating all drawings</param>
        /// </summary>
        StateChanging stateChanging = StateChanging.None;
        public Dictionary<StateCalculation, bool> statesCalculation = new Dictionary<StateCalculation, bool>();

        StateAutoMoving stateAutoMoving = StateAutoMoving.None;
        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();

        GraphGenerator graphGenerator;

        double cx, cy; 
        public double sizeOfVertex;
        public double scale;

        int Smoothing => 0;

        List<Vertex> selectedVertices = new List<Vertex>();
        List<Vertex> selectedCanvasPlaces = new List<Vertex>();

        GraphCoordinates graphCoordinates = new GraphCoordinates();

        Brush[] colors = new Brush[] {Brushes.Red, Brushes.Orange, Brushes.Yellow, Brushes.LightGreen, Brushes.ForestGreen,
            Brushes.LightSkyBlue, Brushes.Blue, Brushes.DarkBlue, Brushes.Purple, Brushes.Pink };


        public MainWindow()
        {
            foreach (StateCalculation state in Enum.GetValues(typeof(StateCalculation)))
                statesCalculation[state] = false;

            statesCalculation[StateCalculation.Intersections] = true;

            InitializeComponent();

            StateChanged += ResizeWindowEvent;

            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            dispatcherTimer.Start();

            cx = 350; cy = 300;
            sizeOfVertex = 15;
            scale = 1.5;

            CollisionDetection.Init(this);
            ForceDirectedAlgorithms.Init(this);



            graphGenerator = new GraphGenerator((int)NextDrawingUpDown.Value); ;
            graphCoordinates = graphGenerator.GenerateNextDrawing();

            DrawGraph(graphCoordinates, 1);

            //graphCoordinates = ForceDirectedAlgorithms.CountAndMoveByForces(graphCoordinates);
            //DrawGraph(graphCoordinates, 1, true);

            MakeSmoother();

            //MessageBox.Show("Testing");


        }


        private void MakeSmoother()
        {
            for (int i = 0; i < Smoothing; i++)
            {
                graphCoordinates = ForceDirectedAlgorithms.CountAndMoveByForces(graphCoordinates);
                //DrawGraph(graphCoordinates, 1, true);
            }
            DrawGraph(graphCoordinates, 1, true);
        }

        private int optimalCrossingNumber() {

            int numberOfVertices = 0;
            foreach (var vertex in graphCoordinates.vertices)
                numberOfVertices += (vertex.state == VertexState.Regular) ? 1 : 0;

            return (numberOfVertices / 2) *
                ((numberOfVertices - 1) / 2) *
                ((numberOfVertices - 2) / 2) *
                ((numberOfVertices - 3) / 2) / 4;
        }
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            dispatcherTimer.Interval = TimeSpan.FromSeconds(1 / (double)AutoMovingUpDown.Value);
            if(stateAutoMoving == StateAutoMoving.Auto)
                NextDrawing_Click(sender, new RoutedEventArgs());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graphCoordinates">To store graph topical</param>
        /// <param name="scale">To know if it is maximized or minimized</param>
        private void RedrawGraph(GraphCoordinates graphCoordinates, double scale)
        {
            mainCanvas.Children.Clear();

            /* Updating Vertices */
            var vertices = new HashSet<Vertex>();
            foreach (var vertex in graphCoordinates.vertices)
            {
                Vertex vertexTemp = vertex;
                vertexTemp.center = vertexTemp.center.Scale(scale);

                vertexTemp.ellipse.Margin = new Thickness(vertexTemp.center.X - sizeOfVertex / 2, vertexTemp.center.Y - sizeOfVertex / 2, 0, 0);

                vertexTemp.ellipse.Height *= 1; //scale;
                vertexTemp.ellipse.Width *= 1; //scale;

               mainCanvas.Children.Add(vertexTemp.ellipse);
               vertices.Add(vertexTemp);
            }

            graphCoordinates.vertices = vertices;


            /*Updating edges */
            foreach (var edge in graphCoordinates.edges)
            {
                foreach (var line in edge.lines)
                {
                    line.X1 *= scale;
                    line.Y1 *= scale;
                    line.X2 *= scale;
                    line.Y2 *= scale;
                    line.StrokeThickness *= 1;// scale;

                    mainCanvas.Children.Add(line);
                }

                var pointsTemp = new List<Point>();
                foreach(var point in edge.points)
                {
                    pointsTemp.Add(point.Scale(scale));
                }

                edge.points = pointsTemp; 
            }

            /* Updating neighbors */
            var neighborsTemp = new Dictionary<Vertex, List<Edge>>();
            foreach (var (vertex, listOfEdges) in graphCoordinates.neighbors)
            {
                Vertex vertexTemp = vertex;
                vertexTemp.center = vertexTemp.center.Scale(scale);

                graphCoordinates.vertices.TryGetValue(vertexTemp, out vertexTemp);

                neighborsTemp[vertexTemp] = listOfEdges;

            }
            graphCoordinates.neighbors = neighborsTemp;
        }

        /// <summary>
        /// Function to hangle minimazing and maximazing window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResizeWindowEvent(object sender, EventArgs e)
        {
            if(WindowState == WindowState.Normal)
            {
                RedrawGraph(graphCoordinates, 1 / scale);
                cx = cx / scale; cy = cy / scale; sizeOfVertex = sizeOfVertex; // scale;
                selectedVertices.Clear();

            }

            if (WindowState == WindowState.Maximized)
            {
                RedrawGraph(graphCoordinates, scale);
                cx = cx * scale; cy = cy * scale; sizeOfVertex = sizeOfVertex; //* scale;
                selectedVertices.Clear();

            }

        }

        private void AutoButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            button.Background = stateAutoMoving == StateAutoMoving.Auto ?
              (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFDDDDDD")) : Brushes.BlueViolet;

            stateAutoMoving = stateAutoMoving == StateAutoMoving.Auto ? StateAutoMoving.None : StateAutoMoving.Auto;
        }

        /// <summary>
        /// Function to change the state to Adding
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Adding_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            /*changing color*/
            button.Background = stateChanging == StateChanging.Adding ?
                (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFDDDDDD")) : Brushes.Red;
            Removing.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFDDDDDD"));
            AddingPolyline.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFDDDDDD"));
            Invariant.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFDDDDDD"));

            stateChanging = stateChanging == StateChanging.Adding ? StateChanging.None : StateChanging.Adding;
        }

        /// <summary>
        /// Function to change the state to Removing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Removing_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            /*changing color*/
            button.Background = stateChanging == StateChanging.Removing ?
                (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFDDDDDD")) : Brushes.Red;
            Adding.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFDDDDDD"));
            AddingPolyline.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFDDDDDD"));
            Invariant.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFDDDDDD"));

            stateChanging = stateChanging == StateChanging.Removing ? StateChanging.None : StateChanging.Removing;

        }

        private void AddingPolyline_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            /*changing color*/
            button.Background = stateChanging == StateChanging.AddingPolyline ?
                (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFDDDDDD")) : Brushes.Red;
            Adding.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFDDDDDD"));
            Removing.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFDDDDDD"));
            Invariant.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFDDDDDD"));

            stateChanging = stateChanging == StateChanging.AddingPolyline ? StateChanging.None : StateChanging.AddingPolyline;

        }

        private void Invariant_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            /*changing color*/
            button.Background = stateChanging == StateChanging.Invariant ?
                (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFDDDDDD")) : Brushes.Red;
            Adding.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFDDDDDD"));
            Removing.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFDDDDDD"));
            AddingPolyline.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFDDDDDD"));

            stateChanging = stateChanging == StateChanging.Invariant ? StateChanging.None : StateChanging.Invariant;

        }


        
        /// <summary>
        /// Function to change if intersection are visible or not, intersection can be recognized by green color
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Intersections_Click(object sender, RoutedEventArgs e)
        {

            int numberOfIntersections = 0;

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
                    if (FindVertex(vertex).state == VertexState.Intersection)
                    {
                        vertex.Visibility = Visibility.Hidden;
                    }
                }
            }
            else
            {
                foreach(var vertex in mainCanvas.Children.OfType<Ellipse>())
                {
                    if (FindVertex(vertex).state == VertexState.Intersection)
                    {
                        vertex.Visibility = Visibility.Visible;
                        numberOfIntersections++;
                    }
                }
            }

            optimal.Text = numberOfIntersections == optimalCrossingNumber() ? "YES" : "NO";

        }


        /// <summary>
        /// Function to generate new drawing of clique from data
        /// If value on UpDown counter is changed then new data file is loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NextDrawing_Click(object sender, RoutedEventArgs e)
        {

            if (graphGenerator.SizeOfGraph != NextDrawingUpDown.Value)
            {
                graphGenerator.CloseFile();
                graphGenerator = new GraphGenerator((int)NextDrawingUpDown.Value);
            }
                
            graphCoordinates = graphGenerator.GenerateNextDrawing();
            numberOfDrawing.Text = graphGenerator.counter.ToString();

            if(graphGenerator.counter == 10516)
            {
                Console.WriteLine(5);
            }

            mainCanvas.Children.Clear();
            DrawGraph(graphCoordinates, WindowState == WindowState.Maximized ? scale : 1);

            //graphCoordinates = ForceDirectedAlgorithms.CountAndMoveByForces(graphCoordinates);
            //DrawGraph(graphCoordinates, 1, true);

            MakeSmoother();
           
        }

        
        private void PreviousDrawing_Click(object sender, RoutedEventArgs e)
        {
            int tmpCounter = graphGenerator.counter;

            if (tmpCounter == 1)
            {
                MessageBox.Show("There is no previous one");
                return;
            }

            graphGenerator.CloseFile();
            graphGenerator = new GraphGenerator((int)NextDrawingUpDown.Value);
            graphGenerator.counter = tmpCounter;

            graphCoordinates = graphGenerator.GeneratePreviousDrawing();
            numberOfDrawing.Text = graphGenerator.counter.ToString();

            mainCanvas.Children.Clear();
            DrawGraph(graphCoordinates, WindowState == WindowState.Maximized ? scale : 1);

            MakeSmoother();

        }


        private void DeleteEdgeFromDictionary(Vertex from, Edge to)
        {
            //if (graphCoordinates.neighbors.ContainsKey(from))
            graphCoordinates.neighbors[from].Remove(to);
        }
        /// <summary>
        /// Function to find vertex containg <c>ellipse</c>
        /// </summary>
        /// <param name="ellipse"></param>
        /// <returns></returns>
        private Vertex FindVertex(Ellipse ellipse)
        {

            return FindVertex(new Point(ellipse.Margin.Left + sizeOfVertex / 2, ellipse.Margin.Top + sizeOfVertex / 2));
            /*
            foreach(var vertex in graphCoordinates.vertices)
            {
                if (vertex.ellipse == ellipse)
                    return vertex;
            }
            */
            throw new ArgumentException("There is no such a vertex, containing ellipse");
        }


        public bool Compare(Point a, Point b)
        {
            if (Math.Abs(a.X - b.X) < 0.1 && Math.Abs(a.Y - b.Y) < 0.1)
                return true;
            return false;
        }

        public Vertex FindVertex(Point center)
        {
            foreach (var vertex in graphCoordinates.vertices)
            {
                if (Compare(vertex.center, center))
                    return vertex;
            }

            throw new ArgumentException("There is no such a vertex with that center");
        }

        /// <summary>
        /// Function to find ends of <c>edge</c>
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        private (Vertex, Vertex) FindEdgeEnds(Edge edge)
        {

            var first = FindVertex(edge.points[0]);
            var second = FindVertex(edge.points.Last());

            return (first, second);
        }

        /// <summary>
        /// Function to either select vertices.
        /// After two are selected then line between them is created.
        /// Or to delete vertex and all edges neighbouring with it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ellipse_MouseDown(object sender, RoutedEventArgs e)
        {
            Ellipse ellipse = sender as Ellipse;
            //MessageBox.Show(new { x = ellipse.Margin.Left, y= ellipse.Margin.Top }.ToString());

            if (stateChanging == StateChanging.Adding || stateChanging == StateChanging.AddingPolyline)
            {
                if (ellipse.Fill == Brushes.Green || ellipse.Fill == Brushes.Red) return;
                selectedVertices.Add(new Vertex(ellipse, new Point { X = ellipse.Margin.Left + sizeOfVertex / 2, Y = ellipse.Margin.Top + sizeOfVertex / 2 }, VertexState.Regular));

                if (selectedVertices.Count == 1)
                {
                    selectedCanvasPlaces.Clear();
                }
                if (selectedVertices.Count == 2)
                {

                    if (selectedVertices[0] == selectedVertices[1])
                    {
                        selectedVertices.RemoveAt(1);
                        return;
                    }
                    if (FindEdgeFromVertices(selectedVertices.ElementAt(0), selectedVertices.ElementAt(1)) != null)
                    {
                        MessageBox.Show("You cannot add edge where there is already put one");
                        selectedVertices.Clear();
                        selectedCanvasPlaces.Clear();
                        return;
                    }

                    var allVertices = new List<Vertex> { selectedVertices[0] };
                    foreach (var p in selectedCanvasPlaces)
                        allVertices.Add(p);
                    allVertices.Add(selectedVertices[1]);


                    var lines = new List<Line>();

                    for (int i = 0; i < allVertices.Count - 1; i++)
                    {
                        var line = new Line
                        {
                            X1 = allVertices[i].center.X,
                            Y1 = allVertices[i].center.Y,
                            X2 = allVertices[i + 1].center.X,
                            Y2 = allVertices[i + 1].center.Y,
                            Stroke = Brushes.Red,
                            StrokeThickness = sizeOfVertex / 3
                        };
                        line.MouseDown += line_MouseDown;
                        Panel.SetZIndex(line, 1);
                        // mainCanvas.Children.Add(line);

                        lines.Add(line);
                    }

                    var allPoints = new List<Point>();
                    foreach (var vertex in allVertices)
                        allPoints.Add(vertex.center);

                    var edge = new Edge(
                        allPoints,
                        lines
                    );

                    graphCoordinates.edges.Add(edge);

                    graphCoordinates.AddToDictionary(selectedVertices[0], edge);
                    graphCoordinates.AddToDictionary(selectedVertices[1], edge);

                    foreach (var vertex in selectedCanvasPlaces)
                    {
                        graphCoordinates.vertices.Add(vertex);
                        mainCanvas.Children.Add(vertex.ellipse);
                    }

                    selectedVertices.Clear();
                    selectedCanvasPlaces.Clear();

                    List<Vertex> intersections = new List<Vertex>();
                    foreach (var line in lines)
                    {
                        foreach (var l in mainCanvas.Children.OfType<Line>())
                        {
                            //if (l == line) continue;

                            var intersection = CollisionDetection.TwoLines(line, l);
                            if (!(intersection.X == -1 && intersection.Y == -1))
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
                                //intersections.Add(ellipse2);

                                intersections.Add(new Vertex(ellipse2, new Point { X = intersection.X, Y = intersection.Y }, VertexState.Intersection));

                                //MessageBox.Show(new { intersection.x,intersection.y }.ToString());
                            }
                        }
                    }

                    foreach (var el in intersections)
                    {
                        bool collision = false;
                        foreach (var vertex in graphCoordinates.vertices)
                        {
                            if (vertex.state == VertexState.Regular && CollisionDetection.CenterInsideEllipse(el.center, vertex.ellipse))
                            {
                                collision = true;
                                break;
                            }
                        }

                        if (!collision)
                        {
                            graphCoordinates.vertices.Add(el);
                            mainCanvas.Children.Add(el.ellipse);
                        }
                    }


                    /* adding after to not make problems with intersecting with others line segments of these new edge */
                    foreach (var line in lines)
                        mainCanvas.Children.Add(line);

                }

            }

            if (stateChanging == StateChanging.Removing)
            {

                /* finding edges which intersected */
                List<Line> intersectedLines = new List<Line>();
                foreach (var line in mainCanvas.Children.OfType<Line>())
                {
                    if (CollisionDetection.CenterOfEllipseOnLine(line, ellipse)) // colision at the end can be used if it would not work
                        intersectedLines.Add(line);
                }

                /* removing edges */
                foreach (var line in intersectedLines)
                {

                    var tempEdge = FindEdge(line);
                    if (tempEdge is null)
                        continue;

                    var (start, end) = FindEdgeEnds(tempEdge);

                    DeleteEdgeFromDictionary(start, tempEdge);
                    DeleteEdgeFromDictionary(end, tempEdge);

                    foreach (var l in tempEdge.lines)
                    {
                        RemoveIntersectionsAndMiddleOnes(l);
                        mainCanvas.Children.Remove(l);
                    }

                    graphCoordinates.edges.Remove(tempEdge);
                }

                /* removing vertex */
                if (ellipse.Fill == Brushes.Blue) //intersection is not in vertices and in neigbours
                {
                    mainCanvas.Children.Remove(ellipse);
                    graphCoordinates.neighbors.Remove(FindVertex(ellipse));
                    graphCoordinates.vertices.Remove(FindVertex(ellipse));
                }
            }

            if (stateChanging == StateChanging.Invariant)
            {
                var vertex = FindVertex(ellipse);

                if(vertex.state == VertexState.Regular)
                    ReCalculateKEdges(vertex);
            }
            else
            { 
                UpdateStats();
            }
        }

        /// <summary>
        /// Function to find edge containg <c>line</c>
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private Edge FindEdge(Line line)
        {
            foreach (var edge in graphCoordinates.edges)
            {
                foreach (var l in edge.lines)
                {
                    if (l == line)
                    {
                        return edge;
                    }
                }
            }
            return null; //it is possible it could be deleted yet
        }
        /// <summary>
        /// Function to remove all intersection with <c>line</c> from canvas 
        /// </summary>
        /// <param name="line"></param>
        private void RemoveIntersectionsAndMiddleOnes(Line line)
        {
            var removedIntersections = new List<Ellipse>();
            foreach (var ellipse in mainCanvas.Children.OfType<Ellipse>())
            {
                if (CollisionDetection.LineAndEllipse(line, ellipse)
                    && (ellipse.Fill == Brushes.Green || ellipse.Fill == Brushes.Red))
                    removedIntersections.Add(ellipse);

            }

            foreach (var ellipse in removedIntersections)
            {
                mainCanvas.Children.Remove(ellipse);
                graphCoordinates.vertices.Remove(FindVertex(ellipse));
            }
        }

        /// <summary>
        /// Function to remove line if clicked on
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void line_MouseDown(object sender, RoutedEventArgs e)
        {
            Line line = sender as Line;

            //MessageBox.Show(line.ToString());

            if (stateChanging == StateChanging.Removing)
            {
                var tempEdge = FindEdge(line); 
                var (start, end) = FindEdgeEnds(tempEdge);

                DeleteEdgeFromDictionary(start, tempEdge);
                DeleteEdgeFromDictionary(end, tempEdge);

                foreach (var l in tempEdge.lines)
                {
                    RemoveIntersectionsAndMiddleOnes(l);

                    mainCanvas.Children.Remove(l);
                }

                graphCoordinates.edges.Remove(tempEdge);

            }

            UpdateStats();
        }

        private double Determinant(Vector a, Vector b)
        {
            return a.X * b.Y - a.Y * b.X;
        }

        /// <summary>
        /// Function to find edge between two vertices 
        /// </summary>
        /// <param name="a">First vertex</param>
        /// <param name="b">Second vertex</param>
        /// <returns>Found edge</returns>
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

            return null; // so such edge
        }

        /// <summary>
        /// Function to recalculate number of k edges
        /// and AM, AMAM, AMAMAM k edges
        /// It is done by finding all triangles upon all edges
        /// and counting <c>k</c> for every edge
        /// Then summing function for AM, AMAM, AMAMAM k edges is called
        /// </summary>
        /// 
        int maximalkEdges = 8;

        private void ReCalculateKEdges(Vertex? without = null)
        {
            //int kEdgesPicked = 0;//(int)KhranyUpDown.Value;

            var kEdgdesValues = Enumerable.Repeat(0, maximalkEdges + 1).ToArray();

            Dictionary<(Vertex, Vertex), bool> visited = new Dictionary<(Vertex, Vertex), bool>();

            foreach(var from in graphCoordinates.neighbors.Keys)
            {
                foreach(var to in graphCoordinates.neighbors.Keys)
                {
                    visited[(from, to)] = false;
                }
            }

            if(without.HasValue)
            {
                var vertexWithout = without.Value;
                foreach(var v in graphCoordinates.neighbors.Keys)
                {
                    visited[(vertexWithout, v)] = true;
                    visited[(v, vertexWithout)] = true;

                    var tempEdge = FindEdgeFromVertices(vertexWithout, v);

                    if (tempEdge == null) // it doesnt have to be clique
                        continue;

                    foreach(var line in tempEdge.lines)
                        line.StrokeDashArray = DoubleCollection.Parse("");
                }
            }

            foreach(var (from, value) in graphCoordinates.neighbors)
            {
                foreach (var e1 in value)
                {

                    var to = FindVertex(CollisionDetection.ChooseOppositeOne(e1, from.center));

                    if (visited[(from, to)]) continue;
                    visited[(from, to)] = true;
                    visited[(to, from)] = true;

                    int sumRight = 0;
                    int sumLeft = 0;
                    foreach (var e2 in graphCoordinates.neighbors[to])
                    {
                        var third = FindVertex(CollisionDetection.ChooseOppositeOne(e2, to.center));
                        var e3 = graphCoordinates.neighbors[from].ContainsEnd(from.center, third.center);
                        if (e3 != null){
                            if (third == from || third == to || (without.HasValue && without.Value == third)) continue;

                            (Line, List<Line>) allLines = CollisionDetection.PutLinesTogether(e1, e2, e3);

                            if (CollisionDetection.GetOrientation(allLines.Item1, allLines.Item2) > 0)
                                sumRight++;
                            else
                                sumLeft++;
                        }
                    }

                    int sum = sumRight < sumLeft ? sumRight : sumLeft; 

                    /*
                    int inter;
                    if (sum > (inter =
                        graphCoordinates.neighbors[from].GetEnds(from.center)
                        .Intersect(graphCoordinates.neighbors[to].GetEnds(to.center)).Count()) / 2)
                        sum = inter - sum; //pick smaller
                    */
                    /*
                    if (sum < 0)
                        throw new ArgumentException("You cannot add edge between already connected vertices");
                    */
                    /*
                    if (sum <= kEdgesPicked) // only needed this ones */

                    kEdgdesValues[sum]++;
                    
                    var edge = FindEdgeFromVertices(from, to);

                    //if (edge == null) 
                    //    continue;

                    bool invariant = false;

                    if (without.HasValue)
                    {
                        if (edge.kEdge == sum)
                            invariant = true;
                    }
                    else
                    {
                        edge.kEdge = sum;
                    }

                    foreach (var line in edge.lines)
                    {

                        if (without.HasValue)
                        {
                            if (invariant)
                                line.StrokeDashArray = DoubleCollection.Parse("4 1 1 1 1 1");
                            else
                            {
                                line.StrokeDashArray = DoubleCollection.Parse("");
                            }
                        }
                        
                        if(!without.HasValue)
                            line.Stroke = colors[sum];
                    }
                }
            }


            if (!without.HasValue)
            {
                CalculateAMEdgesAndPrint(kEdgdesValues, maximalkEdges);

                for (int i = 0; i <= maximalkEdges; i++)
                {
                    TextBlock textBlock = FindName($"kEdges{i}") as TextBlock;
                    textBlock.Text = kEdgdesValues[i].ToString();
                }
            }
            //textBlockKEdges.Text = $"K hran velikosti {kEdgesPicked} je {kEdgdesValues[kEdgesPicked]}    ";

            //RedrawGraph(graphCoordinates, 1);

        }
        /// <summary>
        /// Summing function to find wanted values
        /// </summary>
        /// <param name="kEdgesArray">array with how many k edges are for every k</param>
        /// <param name="size">for which k we want to count</param>
        private void CalculateAMEdgesAndPrint(int[] kEdgesArray, int size)
        {
            int[,] AMKEdgesArray = new int[3, size + 1];
            for (int i = 0; i < 3; i++)
                for (int j = 0; j <= size; j++)
                    AMKEdgesArray[i, j] = 0;

            //Enumerable.Repeat(0, size + 1).ToArray();
            AMKEdgesArray[0, 0] = kEdgesArray[0];
            for (int i = 1; i <= size; i++)
            {
                AMKEdgesArray[0, i] = AMKEdgesArray[0, i - 1] + kEdgesArray[i];
            }

            for (int k = 1; k < 3; k++)
            {
                AMKEdgesArray[k, 0] = AMKEdgesArray[k - 1, 0];
                for (int i = 1; i <= size; i++)
                {
                    AMKEdgesArray[k, i] = AMKEdgesArray[k, i - 1] + AMKEdgesArray[k - 1, i];
                }
            }

            for(int i = 0; i <= size; i++)
            {
                TextBlock textBlock = FindName($"amKEdges{i}") as TextBlock;
                textBlock.Text = AMKEdgesArray[0, i].ToString();
            }

            for (int i = 0; i <= size; i++)
            {
                TextBlock textBlock = FindName($"amAmKEdges{i}") as TextBlock;
                textBlock.Text = AMKEdgesArray[1, i].ToString();
            }


            for (int i = 0; i <= size; i++)
            {
                TextBlock textBlock = FindName($"amAmAmKEdges{i}") as TextBlock;
                textBlock.Text = AMKEdgesArray[2, i].ToString();


                
                textBlock = FindName($"theoremAmAmAmKEdges{i}") as TextBlock;
                if (graphCoordinates.edges.Count >= (((2 * i + 3) * (2 * i + 2)) / 2))
                {
                    textBlock.Text = (AMKEdgesArray[2, i] >= (3 * (((i + 4) * (i + 3) * (i + 2) * (i + 1)) / 24))) ? "T" : "F";
                }
                else
                {
                    textBlock.Text = "-";
                }

                if (textBlock.Text == "F")
                    MessageBox.Show("HEUREKA WRONG");
            }
 

            /*
            if (statesCalculation[StateCalculation.AMKEdges])
                textBlockAMKEdges.Text = $"AM K hran velikost {size} je {AMKEdgesArray[0, size]}";
            else if (statesCalculation[StateCalculation.AMAMKEdges])
                textBlockAMKEdges.Text = $"AMAM K hran velikost {size} je {AMKEdgesArray[1, size]}";
            else if (statesCalculation[StateCalculation.AMAMAMKEdges])
                textBlockAMKEdges.Text = $"AMAMAM K hran velikost {size} je {AMKEdgesArray[2, size]}";
            */
        }

        private void UpdateStats()
        {
            int numberOfIntersections = 0;
            foreach (var vertex in graphCoordinates.vertices)
            {
                if (vertex.state == VertexState.Intersection)
                {
                    numberOfIntersections++;
                }
            }

            optimal.Text = numberOfIntersections == optimalCrossingNumber() ? "YES" : "NO";
            crossings.Text = numberOfIntersections.ToString();

            ReCalculateKEdges();
        }
        
        /// <summary>
        /// Function to add vertex when the place is empty
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                graphCoordinates.neighbors.Add(vertex, new List<Edge>());
            }

            if(stateChanging == StateChanging.AddingPolyline)
            {

                //PrintNumberOfEllipses();

                Ellipse ellipse = new Ellipse
                {
                    Width = sizeOfVertex,
                    Height = sizeOfVertex,
                    Fill = Brushes.Red,
                    Margin = new Thickness(pos.X - sizeOfVertex / 2, pos.Y - sizeOfVertex / 2, 0, 0),
                    Visibility = Visibility.Hidden
                };
                Panel.SetZIndex(ellipse, 100);
                ellipse.MouseDown += ellipse_MouseDown;
                
                //mainCanvas.Children.Add(ellipse);

                var vertex = new Vertex(ellipse, new Point { X = pos.X, Y = pos.Y }, VertexState.Middle);
                
                //graphCoordinates.vertices.Add(vertex);

                selectedCanvasPlaces.Add(vertex);

                //PrintNumberOfEllipses();
            }

           
        }

        void PrintNumberOfEllipses()
        {
            int localCounter = 0;
            foreach (var p in mainCanvas.Children.OfType<Ellipse>())
                localCounter++;
            MessageBox.Show(localCounter.ToString());

        }

        /// <summary>
        /// Function similar to ReDrawGraph function but 
        /// there is only rescaling needed
        /// Here we need to add also shift from origin
        /// and set indices and so on
        /// </summary>
        /// <param name="graphCoordinates">Class to store graph</param>
        /// <param name="scale"></param>
        void DrawGraph(GraphCoordinates graphCoordinates, double scale, bool skipShift = false)
        {

            mainCanvas.Children.Clear();
            double copy_cx = cx; double copy_cy = cy;
            if (skipShift)
            {
                cx = 0;
                cy = 0;
            }
                
            /*Updating vertices*/
            var vertices = new HashSet<Vertex>();
            foreach(var vertex in graphCoordinates.vertices)
            {

                var vertexTemp = vertex;

                var coordinates = vertexTemp.center.Scale(scale).Add(new Point(cx, cy));
                vertexTemp.center = coordinates;

                var ellipse = new Ellipse
                {
                    Width = sizeOfVertex,
                    Height = sizeOfVertex,
                    Fill = vertexTemp.state == VertexState.Regular ? Brushes.Blue :
                    ((vertexTemp.state == VertexState.Intersection) ? Brushes.Green : Brushes.Red),
                    Margin = new Thickness(vertexTemp.center.X - sizeOfVertex / 2, vertexTemp.center.Y - sizeOfVertex / 2, 0, 0),
                    Visibility = ((vertexTemp.state == VertexState.Intersection && !statesCalculation[StateCalculation.Intersections]) ||
                    (vertexTemp.state == VertexState.Middle)) ? Visibility.Hidden : Visibility.Visible

                };

                ellipse.MouseDown += ellipse_MouseDown;
                mainCanvas.Children.Add(ellipse);

                Panel.SetZIndex(ellipse, vertexTemp.state == VertexState.Regular ? 100 : 10);

                vertexTemp.ellipse = ellipse;

                vertices.Add(vertexTemp);
            }

            graphCoordinates.vertices = vertices;

            /*Updating edges*/

            foreach (var edge in graphCoordinates.edges)
            {

                var tempPoints = new List<Point>();
                foreach (var point in edge.points)
                {
                    tempPoints.Add(point.Scale(scale).Add(new Point(cx, cy))); //first scale, then add
                }

                edge.points = tempPoints;

                List<Line> tempLines = new List<Line>();
                foreach (var line in edge.lines)
                {
                    var l = new Line
                    {
                        X1 = line.X1 * scale + cx,
                        Y1 = line.Y1 * scale + cy,
                        X2 = line.X2 * scale + cx,
                        Y2 = line.Y2 * scale + cy,
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


            /* Updating neighbors */

            var neighborsTemp = new Dictionary<Vertex, List<Edge>>();
            foreach (var (vertex, listOfEdges) in graphCoordinates.neighbors)
            {
                Vertex vertexTemp = vertex;
                vertexTemp.center = vertexTemp.center.Scale(scale).Add(new Point(cx, cy));

                graphCoordinates.vertices.TryGetValue(vertexTemp, out vertexTemp); //to set also ellipse on right sizes

                neighborsTemp[vertexTemp] = listOfEdges;

            }
            graphCoordinates.neighbors = neighborsTemp;

            UpdateStats();

            cx = copy_cx; cy = copy_cy;
        }
       
    }

}
