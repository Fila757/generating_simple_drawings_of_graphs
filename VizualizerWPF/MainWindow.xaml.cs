using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
    public enum StateCalculation { Intersections, KEdges, AMKEdges, AMAMKEdges, AMAMAMKEdges, ReferenceFace}; //AM = AtMost

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

        double cx = 81.25, cy = 219.75; 
        public double sizeOfVertex;
        public double scale;

        double actualHeight = 0;
        double actualWidth = 0;

        double lambda = 1.1;

        static Point farFarAway = new Point { X = 10000, Y = 10000 };

        bool savedGraphs = false;

        Point facePoint = farFarAway;

        List<Vertex> invariantWithRescpectTo = new List<Vertex>();
        int Smoothing => (int)SmoothingUpDown.Value;

        List<Vertex> selectedVertices = new List<Vertex>();
        List<Vertex> selectedCanvasPlaces = new List<Vertex>();

        GraphCoordinates graphCoordinates = new GraphCoordinates();

        public IEnumerable<Line> LinesIterator()
        {
            foreach (var edge in graphCoordinates.edges)
            {
                foreach (var line in edge.lines)
                {
                    yield return line;
                }
            }
        }

        public IEnumerable<Point> PointsIterator()
        {
            foreach (var edge in graphCoordinates.edges)
            {
                foreach(var point in edge.points)
                {
                    yield return point;
                }
            }
        }


        public List<object> PrintingLines()
        {
            List<object> lines = new List<object>();
            foreach (var edge in graphCoordinates.edges)
            {
                foreach (var line in edge.lines)
                {
                    lines.Add(new { line.X1, line.Y1, line.X2, line.Y2 });
                }
            }
            return lines;
        }

        public List<object> PrintingVertices()
        {
            List<object> vertices = new List<object>();
            foreach(var vertex in graphCoordinates.vertices)
            {
                vertices.Add(new { vertex.center.X, vertex.center.Y });
            }
            return vertices;
        }

        Brush[] colors = new Brush[] {Brushes.Red, Brushes.Orange, Brushes.Yellow, Brushes.LightGreen, Brushes.ForestGreen,
            Brushes.LightSkyBlue, Brushes.Blue, Brushes.DarkBlue, Brushes.Purple, Brushes.Pink };


        public MainWindow()
        {
            foreach (StateCalculation state in Enum.GetValues(typeof(StateCalculation)))
                statesCalculation[state] = false;

            statesCalculation[StateCalculation.Intersections] = true;

            InitializeComponent();

            FlushFromBackUpToNormal(); // at the beggining get everything I draw last time

            //StateChanged += ResizeWindowEvent;
            //SizeChanged += Canvas_Loaded;
            SizeChanged += ResizeWindowEvent;

            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            dispatcherTimer.Start();

            sizeOfVertex = 15;
            scale = 1.5;

            CollisionDetection.Init(this);
            ForceDirectedAlgorithms.Init(this);



            graphGenerator = new GraphGenerator((int)NextDrawingUpDown.Value); 
            graphCoordinates = graphGenerator.GenerateNextDrawing();
            numberOfDrawing.Text = graphGenerator.counter.ToString();

            //DrawGraph(graphCoordinates, 1);

            //graphCoordinates = ForceDirectedAlgorithms.CountAndMoveByForces(graphCoordinates);
            //DrawGraph(graphCoordinates, 1, true);

            MakeSmootherAndDraw();

            //MessageBox.Show("Testing");


        }


        private void Canvas_Loaded(object sender, RoutedEventArgs e)
        {
            actualWidth = mainCanvas.ActualWidth;
            actualHeight = mainCanvas.ActualHeight;

            mainCanvas.Children.Add(new Line { StrokeThickness = 10, Fill = Brushes.Black, X1 = 0, Y1 = 0, X2 = actualWidth, Y2 = 0 });
            mainCanvas.Children.Add(new Line { StrokeThickness = 10, Fill = Brushes.Black, X1 = 0, Y1 = actualHeight, X2 = actualWidth, Y2 = actualHeight });
        }

        private void MakeAllLinesNotSharp()
        {
            foreach(var edge in graphCoordinates.edges)
            {
                edge.Shorten(graphCoordinates);
                /*
                 * foreach (var vertex in removed)
                 *   graphCoordinates.vertices.Remove(vertex);
                */
            }
            CreateVerticesFromLines(graphCoordinates);
        }

        private void CreateVerticesFromLines(GraphCoordinates graphCoordinates)
        {
            var allPoints = new HashSet<Point>(PointsIterator());
            var newVertices = new HashSet<Vertex>();
            foreach(var vertex in graphCoordinates.vertices)
            {
                if (allPoints.Contains(vertex.center))
                    newVertices.Add(vertex);
            }

            graphCoordinates.vertices = newVertices;
        }

        private void MakeSmootherAndDraw()
        {
            SubDivideEdges(graphCoordinates);
            for (int i = 0; i < Smoothing; i++)
            {
                graphCoordinates = ForceDirectedAlgorithms.CountAndMoveByForces(graphCoordinates);

                //DrawGraph(graphCoordinates, 1, true);
                
                //DrawGraph(graphCoordinates, 1, true);
            }
            MakeAllLinesNotSharp();
            DrawGraph(graphCoordinates, 1);
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
        private void RedrawGraph(GraphCoordinates graphCoordinates)
        {
            mainCanvas.Children.Clear();

            (double, double, double, double) coordinatesAndScale = FindClipingSizes(GetAllPoints(graphCoordinates));

            cx = coordinatesAndScale.Item1;
            cy = coordinatesAndScale.Item2;
            var scaleX = coordinatesAndScale.Item3;
            var scaleY = coordinatesAndScale.Item4;


            /* Updating Vertices */
            var vertices = new HashSet<Vertex>();
            foreach (var vertex in graphCoordinates.vertices)
            {
                Vertex vertexTemp = vertex;
                vertexTemp.center = vertexTemp.center.Add(new Point(cx, cy)).Scale(scaleX, scaleY);

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
                    line.X1 = (line.X1 + cx) * scaleX;
                    line.Y1 = (line.Y1 + cy) * scaleY;
                    line.X2 = (line.X2 + cx) * scaleX;
                    line.Y2 = (line.Y2 + cy) * scaleY;
                    line.StrokeThickness *= 1;// scale;

                    mainCanvas.Children.Add(line);
                }

                var pointsTemp = new List<Point>();
                foreach(var point in edge.points)
                {
                    pointsTemp.Add(point.Add(new Point(cx, cy)).Scale(scaleX, scaleY));
                }

                edge.points = pointsTemp; 
            }

            /* Updating neighbors */
            var neighborsTemp = new Dictionary<Vertex, List<Edge>>();
            foreach (var (vertex, listOfEdges) in graphCoordinates.neighbors)
            {
                Vertex vertexTemp = vertex;
                vertexTemp.center = vertexTemp.center.Add(new Point(cx, cy)).Scale(scaleX, scaleY);

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
            Canvas_Loaded(sender, new RoutedEventArgs());

            RedrawGraph(graphCoordinates);

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
            if (stateChanging != StateChanging.Invariant)
            {
                MakeAllVerticesBlue();
                invariantWithRescpectTo = new List<Vertex>();
            }


        }

        private void MakeAllVerticesBlue()
        {
            foreach (var vertex in graphCoordinates.neighbors.Keys)
                vertex.ellipse.Fill = Brushes.Blue;
        }

        private void RefferenceFace_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            button.Background = button.Background == Brushes.Red ?
               (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFDDDDDD")) : Brushes.Red;

            statesCalculation[StateCalculation.ReferenceFace] = !statesCalculation[StateCalculation.ReferenceFace];
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
                foreach (var vertex in graphCoordinates.vertices)
                {
                    if (vertex.state == VertexState.Intersection)
                    {
                        vertex.ellipse.Visibility = Visibility.Hidden;
                    }
                }
            }
            else
            {
                foreach(var vertex in graphCoordinates.vertices)
                {
                    if (vertex.state == VertexState.Intersection)
                    {
                        vertex.ellipse.Visibility = Visibility.Visible;
                        numberOfIntersections++;
                    }
                }
            }

            optimal.Text = numberOfIntersections == optimalCrossingNumber() ? "YES" : "NO";
            if(optimal.Text == "YES")
            {
                MessageBox.Show("Optimal");
            }

        }

        private void SaveDrawing_Click(object sender, RoutedEventArgs e)
        {
            graphCoordinates.SaveCoordinates();
        }

        private void FlushFromBackUpToNormal()
        {
            using (var sr = new StreamReader("C:/Users/filip/source/repos/generating-simple-drawings-of-graphs/VizualizerWPF/data/savedGraphsBackUp.txt"))
            {
                using (var sw = new StreamWriter("C:/Users/filip/source/repos/generating-simple-drawings-of-graphs/VizualizerWPF/data/savedGraphs.txt", append: true))
                {
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();
                        sw.WriteLine(line);
                    }
                }
            }

            File.WriteAllText("C:/Users/filip/source/repos/generating-simple-drawings-of-graphs/VizualizerWPF/data/savedGraphsBackUp.txt", string.Empty);
        }

        /// <summary>
        /// Function to generate new drawing of clique from data
        /// If value on UpDown counter is changed then new data file is loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NextDrawing_Click(object sender, RoutedEventArgs e)
        {

            facePoint = farFarAway; //changing again to the outer face

            if ((bool)savedGraphsChechBox.IsChecked && !savedGraphs)
            {
                graphGenerator.CloseFile();
                savedGraphs = true;
                graphGenerator = new GraphGenerator();
            }

            else if (
                (!(bool)savedGraphsChechBox.IsChecked && graphGenerator.SizeOfGraph != NextDrawingUpDown.Value) 
                || 
                (savedGraphs && !(bool)savedGraphsChechBox.IsChecked)
                )
            {
                graphGenerator.CloseFile();
                savedGraphs = false;

                FlushFromBackUpToNormal();

                graphGenerator = new GraphGenerator((int)NextDrawingUpDown.Value);
            }
                
            graphCoordinates = graphGenerator.GenerateNextDrawing();
            numberOfDrawing.Text = graphGenerator.counter.ToString();

            mainCanvas.Children.Clear();
            //DrawGraph(graphCoordinates, WindowState == WindowState.Maximized ? scale : 1);

            //graphCoordinates = ForceDirectedAlgorithms.CountAndMoveByForces(graphCoordinates);
            //DrawGraph(graphCoordinates, 1, true);

            MakeSmootherAndDraw();
           
        }

        
        private void PreviousDrawing_Click(object sender, RoutedEventArgs e)
        {
            facePoint = farFarAway;
            int tmpCounter = graphGenerator.counter;

            if (tmpCounter == 1)
            {
                MessageBox.Show("There is no previous one");
                return;
            }

            graphGenerator.CloseFile();
            graphGenerator = !(bool)savedGraphsChechBox.IsChecked ? new GraphGenerator((int)NextDrawingUpDown.Value) : new GraphGenerator();
            graphGenerator.counter = tmpCounter;

            graphCoordinates = graphGenerator.GeneratePreviousDrawing();
            numberOfDrawing.Text = graphGenerator.counter.ToString();

            mainCanvas.Children.Clear();
            //DrawGraph(graphCoordinates, WindowState == WindowState.Maximized ? scale : 1);

            MakeSmootherAndDraw();

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

            //return new Vertex();
            throw new ArgumentException("There is no such a vertex with that center");
        }

        public Vertex? FindVertexSave(Point center)
        {
            foreach (var vertex in graphCoordinates.vertices)
            {
                if (Compare(vertex.center, center))
                    return vertex;
            }

            return null;
            
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

        private void AddIntersectionsWithLines(List<Line> lines)
        {
            HashSet<Vertex> intersections = new HashSet<Vertex>( );
            foreach (var line in lines)
            {
                foreach (var l in mainCanvas.Children.OfType<Line>())
                {
                    //if (l == line) continue;

                    var intersection = CollisionDetection.TwoLinesIntersectNotAtTheEnd(line, l);
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

                    AddIntersectionsWithLines(lines);

                    /* adding after to not make problems with intersecting with others line segments of these new edge */
                    foreach (var line in lines)
                        mainCanvas.Children.Add(line);

                }

            }

            if (stateChanging == StateChanging.Removing)
            {

                /* finding edges which intersected */
                List<Line> intersectedLines = new List<Line>();
                foreach (var line in LinesIterator())
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

                    RemoveIntersectionsAndMiddleOnes(tempEdge);

                    graphCoordinates.edges.Remove(tempEdge);
                }

                /* removing vertex */
                if (ellipse.Fill == Brushes.Blue) //intersection are already deleted
                {
                    mainCanvas.Children.Remove(ellipse);
                    graphCoordinates.neighbors.Remove(FindVertex(ellipse));
                    graphCoordinates.vertices.Remove(FindVertex(ellipse));
                }
            }

            ZeroInvariantEdgesValues();

            if (stateChanging == StateChanging.Invariant)
            {

                var vertex = FindVertex(ellipse);

                if (vertex.state == VertexState.Regular)
                {
                    ellipse.Fill = Brushes.Purple;
                    invariantWithRescpectTo.Add(vertex);
                    ReCalculateKEdges(invariantWithRescpectTo);
                }
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
        private void RemoveIntersections(Line line)
        {
            var removedIntersections = new List<Ellipse>();
            foreach (var ellipse in mainCanvas.Children.OfType<Ellipse>())
            {
                if (CollisionDetection.LineAndEllipse(line, ellipse)
                    && (ellipse.Fill == Brushes.Green))
                    removedIntersections.Add(ellipse);

            }

            foreach (var ellipse in removedIntersections)
            {
                mainCanvas.Children.Remove(ellipse);
                graphCoordinates.vertices.Remove(FindVertex(ellipse));
            }
        }

        void RemoveIntersectionsAndMiddleOnes(Edge edge)
        {
            foreach (var l in edge.lines)
            {
                RemoveIntersections(l); //only intersections, middle ones remove from edge
                mainCanvas.Children.Remove(l);
            }

            foreach (var p in edge.points)
            {
                var vertexOrNull = FindVertexSave(p);
                if (vertexOrNull.HasValue && vertexOrNull.Value.state != VertexState.Regular)
                {
                    graphCoordinates.vertices.Remove(vertexOrNull.Value);
                    mainCanvas.Children.Remove(vertexOrNull.Value.ellipse);
                }
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

                RemoveIntersectionsAndMiddleOnes(tempEdge);

                graphCoordinates.edges.Remove(tempEdge);

            }

            ZeroInvariantEdgesValues();
         
            if (stateChanging == StateChanging.Invariant)
            {
                var withoutEdge = FindEdge(line);
                ReCalculateKEdges(null, withoutEdge);
            }
            else
            {
                UpdateStats();
            }

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

        bool CheckIfEdgeIsInTriangle(Vertex from, Vertex to, Vertex third, Vertex firstEdgeVertex, Vertex secondEdgeVertex)
        {
            if((from == firstEdgeVertex && third == secondEdgeVertex) || (to == firstEdgeVertex && third == secondEdgeVertex) ||
                (from == secondEdgeVertex && third == firstEdgeVertex) || (to == secondEdgeVertex && third == firstEdgeVertex))
            {
                return true;
            }
            return false;
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

        private void ReCalculateKEdges(List<Vertex> withouts = null, Edge withoutEdge = null)
        {
            //int kEdgesPicked = 0;//(int)KhranyUpDown.Value;

            var kEdgdesValues = Enumerable.Repeat(0, maximalkEdges + 1).ToArray();
            var invariantKEdges = Enumerable.Repeat(0, maximalkEdges + 1).ToArray();

            Dictionary<(Vertex, Vertex), bool> visited = new Dictionary<(Vertex, Vertex), bool>();

            foreach(var from in graphCoordinates.neighbors.Keys)
            {
                foreach(var to in graphCoordinates.neighbors.Keys)
                {
                    visited[(from, to)] = false;
                }
            }

            if(withouts != null)
            {
                foreach (var vertex in withouts)
                {
                    foreach (var v in graphCoordinates.neighbors.Keys)
                    {
                        visited[(vertex, v)] = true;
                        visited[(v, vertex)] = true;

                        //var tempEdge = FindEdgeFromVertices(vertexWithout, v);

                        //if (tempEdge == null) // it doesnt have to be clique
                        //continue;

                        //foreach(var line in tempEdge.lines)
                        //line.StrokeDashArray = DoubleCollection.Parse("");
                    }
                }
            }

            Vertex? firstEdgeVertex = null;
            Vertex? secondEdgeVertex = null;

            if (withoutEdge != null)
            {
                firstEdgeVertex = FindVertex(withoutEdge.points.First());
                secondEdgeVertex = FindVertex(withoutEdge.points.Last());

                visited[(firstEdgeVertex.Value, secondEdgeVertex.Value)] = true;
                visited[(secondEdgeVertex.Value, firstEdgeVertex.Value)] = true;

                foreach (var line in withoutEdge.lines)
                    line.StrokeDashArray = DoubleCollection.Parse("");
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
                            if (third == from || third == to
                                ||
                                (withouts != null && withouts.Contains(third))
                                ||
                                (firstEdgeVertex.HasValue
                                &&
                                secondEdgeVertex.HasValue
                                &&
                                CheckIfEdgeIsInTriangle(from, to, third, firstEdgeVertex.Value, secondEdgeVertex.Value))) 
                            {
                                continue; 
                            }

                            (Line, List<Line>) allLines = CollisionDetection.PutLinesTogether(e1, e2, e3);

                            if (CollisionDetection.GetOrientation(allLines.Item1, allLines.Item2, facePoint) > 0)
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

                    if (withouts != null || withoutEdge != null)
                    {
                        if (edge.kEdge == sum)
                        {
                            invariant = true;
                            invariantKEdges[sum]++;
                        }
                    }
                    else
                    {
                        edge.kEdge = sum;
                    }

                    foreach (var line in edge.lines)
                    {

                        if (withouts != null || withoutEdge != null)
                        {
                            if (invariant)
                                line.StrokeDashArray = DoubleCollection.Parse("4 1 1 1 1 1");
                            else
                            {
                                line.StrokeDashArray = DoubleCollection.Parse("");
                            }
                        }
                        
                        else
                            line.Stroke = colors[sum];
                    }
                }
            }


            if (withouts == null && withoutEdge == null)
            {
                CalculateAMEdgesAndPrint(kEdgdesValues, maximalkEdges);

                for (int i = 0; i <= maximalkEdges; i++)
                {
                    TextBlock textBlock = FindName($"kEdges{i}") as TextBlock;
                    textBlock.Text = kEdgdesValues[i].ToString();
                }
            }

            else
            {
                var invariantAmKEdges = Enumerable.Repeat(0, maximalkEdges + 1).ToArray();

                invariantAmKEdges[0] = invariantKEdges[0];
                TextBlock textBlock = FindName($"invariantAmKedges{0}") as TextBlock;
                textBlock.Text = invariantAmKEdges[0].ToString();

                for (int i = 1; i <= maximalkEdges; i++)
                {
                    invariantAmKEdges[i] = invariantAmKEdges[i - 1] + invariantKEdges[i];
                    textBlock = FindName($"invariantAmKedges{i}") as TextBlock;
                    textBlock.Text = invariantAmKEdges[i].ToString();
                }

                int amAmInvariant = 0;

                for (int i = 0; i <= maximalkEdges; i++)
                {
                    amAmInvariant += invariantAmKEdges[i];
                    textBlock = FindName($"invariantAmAmKedges{i}") as TextBlock;
                    textBlock.Text = amAmInvariant.ToString();
                }

                if (withoutEdge != null)
                {

                    int second, third;

                    textBlock = removedEdgeAMAMAMSecond;
                    textBlock.Text = (FindName($"invariantAmAmKedges{(int)kWhenRemovingUpDown.Value}") as TextBlock).Text.ToString();
                    second = Int32.Parse(textBlock.Text);


                    textBlock = removedEdgeAMAMAMThird;
                    textBlock.Text =
                        (((((int)kWhenRemovingUpDown.Value + 2) - withoutEdge.kEdge)
                        *
                        (((int)kWhenRemovingUpDown.Value + 1) - withoutEdge.kEdge)) / 2).ToString();
                    third = Int32.Parse(textBlock.Text);

                    removedEdgeAMAMAMFirst.Text = (Int32.Parse((FindName($"amAmAmKEdges{(int)kWhenRemovingUpDown.Value}") as TextBlock).Text) - second - third).ToString();



                    textBlock = removedEdgeAMAMSecond;
                    textBlock.Text = (FindName($"invariantAmKedges{(int)kWhenRemovingUpDown.Value}") as TextBlock).Text.ToString();
                    second = Int32.Parse(textBlock.Text);


                    textBlock = removedEdgeAMAMThird;
                    textBlock.Text = (((int)kWhenRemovingUpDown.Value + 1) - withoutEdge.kEdge).ToString();
                    third = Int32.Parse(textBlock.Text);

                    removedEdgeAMAMFirst.Text = (Int32.Parse((FindName($"amAmKEdges{(int)kWhenRemovingUpDown.Value}") as TextBlock).Text) - second - third).ToString();


                }

            }
            //textBlockKEdges.Text = $"K hran velikosti {kEdgesPicked} je {kEdgdesValues[kEdgesPicked]}    ";

            //RedrawGraph(graphCoordinates, 1);

        }

        void ZeroInvariantEdgesValues()
        {

            foreach(var edge in graphCoordinates.edges)
            {
                foreach(var line in edge.lines)
                    line.StrokeDashArray = DoubleCollection.Parse("");
            }

            for (int i = 0; i <= maximalkEdges; i++)
            {
                var textBlock = FindName($"invariantAmKedges{i}") as TextBlock;
                textBlock.Text = 0.ToString();

                textBlock = FindName($"invariantAmAmKedges{i}") as TextBlock;
                textBlock.Text = 0.ToString();

            }



            
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
            /*
            if (optimal.Text == "YES")
            {
                MessageBox.Show("OPT");
                stateAutoMoving = StateAutoMoving.None;
            }
            */

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

            if (statesCalculation[StateCalculation.ReferenceFace])
            {
                facePoint = pos;

                ZeroInvariantEdgesValues();
                UpdateStats();
            }

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

        (double, double, double, double) FindClipingSizes(HashSet<Point> points)
        {
            double leftX = 100000000, rightX = -100000000, topY = -100000000, bottomY = 100000000;

            foreach(var point in points)
            {
                if(leftX > point.X)
                {
                    leftX = point.X;
                }

                if(rightX < point.X)
                {
                    rightX = point.X;
                }

                if(topY < point.Y)
                {
                    topY = point.Y;
                }

                if(bottomY > point.Y)
                {
                    bottomY = point.Y; // in usual coordinates system
                }
            }

            double sizeX = Math.Abs(rightX - leftX);
            double sizeY = Math.Abs(topY - bottomY);

            return (-leftX + 20, -bottomY + 20, actualWidth / (sizeX * lambda), actualHeight / (sizeY * lambda)); //actual?

        }

        HashSet<Point> GetAllPoints(GraphCoordinates graphCoordinates)
        {
            HashSet<Point> result = new HashSet<Point>();
            foreach (var vertex in graphCoordinates.vertices)
                result.Add(vertex.center);

            /*
            foreach (var edge in graphCoordinates.edges)
            {
                foreach(var point in edge.points)
                {
                    result.Add(point);
                }
            }
            */

            return result;
        }

        int divisionConst = 50;  

        List<Line> CreateLinesFromPoints(List<Point> points)
        {
            var lines = new List<Line>();
            for (int i = 0; i < points.Count - 1; i++)
            {
                lines.Add(new Line
                {
                    X1 = points[i].X,
                    X2 = points[i + 1].X,
                    Y1 = points[i].Y,
                    Y2 = points[i + 1].Y,
                });
            }
            return lines;
        }

        (List<Line>, List<Point>) SubDivideLine(Line line, GraphCoordinates graphCoordinates)
        {
            var edgePoints = new List<Point>();
          
            int count = (int)Math.Ceiling(CollisionDetection.GetDistance(line) / divisionConst) ;
            double length = CollisionDetection.GetDistance(line) / count;

            Vector direction = new Vector(line.X2 - line.X1, line.Y2 - line.Y1);
            direction.Normalize();

            edgePoints.Add(new Point(line.X1, line.Y1));
            for (int i = 1; i < count; i++)
            {
                edgePoints.Add(new Point(line.X1, line.Y1) + direction * (i * length));

                graphCoordinates.vertices.Add(new Vertex(new Ellipse(), edgePoints.Last(), VertexState.Middle));
            }

            edgePoints.Add(new Point(line.X2, line.Y2));
            var edgeLines = CreateLinesFromPoints(edgePoints);

            edgePoints.RemoveAt(0); edgePoints.RemoveAt(edgePoints.Count - 1);
            return (edgeLines, edgePoints);
            
        }

        void SubDivideEdge(Edge edge, GraphCoordinates graphCoordinates)
        {

            var tempPoints = new List<Point>();

            int index = 0;

            List<Line> tempLines = new List<Line>();
            foreach (var line in edge.lines)
            {
                var linesAndPoints = SubDivideLine(line, graphCoordinates);

                tempLines.AddRange(linesAndPoints.Item1);
                tempPoints.Add(edge.points[index]);
                tempPoints.AddRange(linesAndPoints.Item2);
                index++;
            }

            tempPoints.Add(edge.points[index]);

            edge.lines = tempLines;
            edge.points = tempPoints;

        }

        void SubDivideEdges(GraphCoordinates graphCoordinates)
        {
            foreach (var edge in graphCoordinates.edges)
                SubDivideEdge(edge, graphCoordinates);
        }
        /// <summary>
        /// Function similar to ReDrawGraph function but 
        /// there is only rescaling needed
        /// Here we need to add also shift from origin
        /// and set indices and so on
        /// </summary>
        /// <param name="graphCoordinates">Class to store graph</param>
        /// <param name="scale"></param>
        void DrawGraph(GraphCoordinates graphCoordinates, double scale)
        {

            mainCanvas.Children.Clear();

            double scaleX = scale, scaleY = scale;

            if (!(actualWidth == 0 && actualHeight == 0))
            {

                (double, double, double, double) coordinatesAndScale = FindClipingSizes(GetAllPoints(graphCoordinates));

                cx = coordinatesAndScale.Item1;
                cy = coordinatesAndScale.Item2;
                scaleX = coordinatesAndScale.Item3;
                scaleY = coordinatesAndScale.Item4;
            }
            

            //if (skipShift)
            //{
            //    double copyCx = cx; double copyCy = cy;

            //    cx = 0;
            //    cy = 0;
            //}
                
            /*Updating vertices*/
            var vertices = new HashSet<Vertex>();
            foreach(var vertex in graphCoordinates.vertices)
            {

                var vertexTemp = vertex;

                var coordinates = vertexTemp.center.Add(new Point(cx, cy)).Scale(scaleX, scaleY);
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



            foreach (var edge in graphCoordinates.edges)
            {

                var tempPoints = new List<Point>();
                foreach (var point in edge.points)
                {
                    tempPoints.Add(point.Add(new Point(cx, cy)).Scale(scaleX, scaleY)); //first scale, then add
                }

                edge.points = tempPoints;

                List<Line> tempLines = new List<Line>();
                foreach (var line in edge.lines)
                {
                    var l = new Line
                    {
                        X1 = (line.X1 + cx) * scaleX,
                        Y1 = (line.Y1 + cy) * scaleY,
                        X2 = (line.X2 + cx) * scaleX,
                        Y2 = (line.Y2 + cy) * scaleY,
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
                vertexTemp.center = vertexTemp.center.Add(new Point(cx, cy)).Scale(scaleX, scaleY);

                graphCoordinates.vertices.TryGetValue(vertexTemp, out vertexTemp); //to set also ellipse on right sizes

                neighborsTemp[vertexTemp] = listOfEdges;

            }
            graphCoordinates.neighbors = neighborsTemp;

            if((bool)savedGraphsChechBox.IsChecked)
                AddIntersectionsWithLines(LinesIterator().ToList());

            ZeroInvariantEdgesValues();
            UpdateStats();

            //cx = copyCx; cy = copyCy;
        }
       
    }

}
