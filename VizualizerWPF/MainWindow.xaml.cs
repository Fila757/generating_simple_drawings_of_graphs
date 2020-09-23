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

    enum StateChanging { Adding, Removing, None };

    /// <summary>
    /// Enum for showing Intersection, KEdges, AtMostKEdge...
    /// </summary>
    enum StateCalculation { Intersections, KEdges, AMKEdges, AMAMKEdges, AMAMAMKEdges}; //AM = AtMost

    public partial class MainWindow : Window
    {
        /// <summary>
        /// <param name="cx">Shift from 0,0 origin</param>
        /// <param name="cy">Shift from 0,0 origin</param>
        /// <param name="stateChanging">For recognizing in which state we are</param>
        /// <param name="graphGenerator">It is class for generating all drawings</param>
        /// </summary>
        StateChanging stateChanging = StateChanging.None;
        Dictionary<StateCalculation, bool> statesCalculation = new Dictionary<StateCalculation, bool>();

        GraphGenerator graphGenerator;

        double cx, cy; 
        public double sizeOfVertex;
        double scale;

        HashSet<Vertex> selectedVertices = new HashSet<Vertex>();

        GraphCoordinates graphCoordinates = new GraphCoordinates();

        Brush[] colors = new Brush[] {Brushes.Red, Brushes.Orange, Brushes.Yellow, Brushes.LightGreen, Brushes.ForestGreen,
            Brushes.LightSkyBlue, Brushes.Blue, Brushes.DarkBlue, Brushes.Purple, Brushes.Pink };


        /// <summary>
        /// Function which remain state that maximally one of the listBox option is selected
        /// </summary>
        /// <param name="listBox">listBoc given in WPF</param>
        private void SelectOnlyOneOption(ListBox listBox)
        {
            if (listBox.SelectedIndex == 0)
            {
                statesCalculation[StateCalculation.AMKEdges] = true;
                statesCalculation[StateCalculation.AMAMKEdges] = false;
                statesCalculation[StateCalculation.AMAMAMKEdges] = false;
            }
            else if (listBox.SelectedIndex == 1)
            {
                statesCalculation[StateCalculation.AMKEdges] = false;
                statesCalculation[StateCalculation.AMAMKEdges] = true;
                statesCalculation[StateCalculation.AMAMAMKEdges] = false;
            }
            else if (listBox.SelectedIndex == 2)
            {
                statesCalculation[StateCalculation.AMKEdges] = false;
                statesCalculation[StateCalculation.AMAMKEdges] = false;
                statesCalculation[StateCalculation.AMAMAMKEdges] = true;
            }
            else
            {
                statesCalculation[StateCalculation.AMKEdges] = false;
                statesCalculation[StateCalculation.AMAMKEdges] = false;
                statesCalculation[StateCalculation.AMAMAMKEdges] = false;
            }

        }

        public MainWindow()
        {
            foreach (StateCalculation state in Enum.GetValues(typeof(StateCalculation)))
                statesCalculation[state] = false;

            InitializeComponent();

            StateChanged += new EventHandler(ResizeWindowEvent);

            cx = 300; cy = 200;
            sizeOfVertex = 15;
            scale = 1.5;

            CollisionDetection.Init(this);

            graphGenerator = new GraphGenerator((int)NextDrawingUpDown.Value);
            graphCoordinates = graphGenerator.GenerateNextDrawing();
            DrawGraph(graphCoordinates, 1);

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

                vertexTemp.ellipse.Margin = new Thickness(vertexTemp.center.X - scale * sizeOfVertex / 2, vertexTemp.center.Y - scale * sizeOfVertex / 2, 0, 0);

                vertexTemp.ellipse.Height *= scale;
                vertexTemp.ellipse.Width *= scale;

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

            /* Updating neighbors */
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
                selectedVertices.Clear();

            }

            if (WindowState == WindowState.Maximized)
            {
                RedrawGraph(graphCoordinates, scale);
                cx = cx * scale; cy = cy * scale; sizeOfVertex = sizeOfVertex * scale;
                selectedVertices.Clear();

            }

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

            stateChanging = stateChanging == StateChanging.Removing ? StateChanging.None : StateChanging.Removing;

        }
        /// <summary>
        /// Function to change if intersection are visible or not, intersection can be recognized by green color
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Function to change if K edges counting (textBlock) is visible
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Kedges_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            /*changing color*/
            button.Background = statesCalculation[StateCalculation.KEdges] ?
                (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFDDDDDD")) : Brushes.Red;

            statesCalculation[StateCalculation.KEdges] = !statesCalculation[StateCalculation.KEdges];
            if (statesCalculation[StateCalculation.KEdges])
                textBlockKEdges.Visibility = Visibility.Visible;
            else
                textBlockKEdges.Visibility = Visibility.Hidden;
        }
        /// <summary>
        /// Function to change if AtMost, AMAM and AMAMAM K edges counting (textBlock) is visible 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void KedgesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selection = sender as ListBox;
            SelectOnlyOneOption(selection);
            if (selection.SelectedIndex != -1)
                textBlockAMKEdges.Visibility = Visibility.Visible;
            else
                textBlockAMKEdges.Visibility = Visibility.Hidden;
            ReCalculateKEdges();
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

            mainCanvas.Children.Clear();
            DrawGraph(graphCoordinates, WindowState ==  WindowState.Maximized ? scale : 1);
        }

        private void DeleteEdgeFromDictionary(Vertex from, Vertex to)
        {
            if (graphCoordinates.neighbors.ContainsKey(from))
                graphCoordinates.neighbors[from].Remove(to);
        }
        /// <summary>
        /// Function to find vertex containg <c>ellipse</c>
        /// </summary>
        /// <param name="ellipse"></param>
        /// <returns></returns>
        private Vertex FindVertex(Ellipse ellipse)
        {
            foreach(var vertex in graphCoordinates.vertices)
            {
                if (vertex.ellipse == ellipse)
                    return vertex;
            }

            throw new ArgumentException("There is no such a vertex, containing ellipse");
        }
        /// <summary>
        /// Function to find ends of <c>edge</c>
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Function to either select vertices.
        /// After two are selected then line between them is created.
        /// Or to delete vertex and all edges neighbouring with it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ellipse_MouseDown(object sender, RoutedEventArgs e)
        {
            Ellipse ellipse = sender as Ellipse;
            //MessageBox.Show(new { x = ellipse.Margin.Left, y= ellipse.Margin.Top }.ToString());

            if (stateChanging == StateChanging.Adding)
            {
                if (ellipse.Fill == Brushes.Green) return;
                selectedVertices.Add(new Vertex(ellipse, new Point { X = ellipse.Margin.Left + sizeOfVertex/2, Y = ellipse.Margin.Top + sizeOfVertex/2}, VertexState.Regular));
                if (selectedVertices.Count == 2)
                {
                    var line = new Line
                    {
                        X1 = selectedVertices.ElementAt(0).center.X,
                        Y1 = selectedVertices.ElementAt(0).center.Y,
                        X2 = selectedVertices.ElementAt(1).center.X,
                        Y2 = selectedVertices.ElementAt(1).center.Y,
                        Stroke = Brushes.Red,
                        StrokeThickness = sizeOfVertex / 3
                    };
                    line.MouseDown += line_MouseDown;
                    Panel.SetZIndex(line, 1);
                    mainCanvas.Children.Add(line);

                    graphCoordinates.edges.Add(new Edge(
                        new List<Point> { selectedVertices.ElementAt(0).center, selectedVertices.ElementAt(1).center},
                        new List<Line> { line }
                    ));

                    graphCoordinates.AddToDictionary(selectedVertices.ElementAt(0), selectedVertices.ElementAt(1));
                    graphCoordinates.AddToDictionary(selectedVertices.ElementAt(1), selectedVertices.ElementAt(0));

                    selectedVertices.Clear();

                    List<Ellipse> intersections = new List<Ellipse>();
                    foreach (var l in mainCanvas.Children.OfType<Line>())
                    {
                        if (l == line) continue;

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
                            intersections.Add(ellipse2);

                            graphCoordinates.vertices.Add(new Vertex(ellipse2, new Point { X = intersection.X, Y = intersection.Y }, VertexState.Intersection));

                            //MessageBox.Show(new { intersection.x,intersection.y }.ToString());
                        }
                    }

                    foreach (var el in intersections)
                        mainCanvas.Children.Add(el);
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
                foreach (var line in intersectedLines) {

                    var tempEdge = FindEdge(line);
                    if (tempEdge is null)
                        continue;

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

                /* removing vertex */
                if (ellipse.Fill != Brushes.Green) //intersection is not in vertices and in neigbours
                {
                    mainCanvas.Children.Remove(ellipse);
                    graphCoordinates.neighbors.Remove(FindVertex(ellipse));
                    graphCoordinates.vertices.Remove(FindVertex(ellipse));
                }
            }

            ReCalculateKEdges();
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
                    && ellipse.Fill == Brushes.Green)
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

            ReCalculateKEdges();
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

            throw new ArgumentException("No such an edge between vertices");
        }

        /// <summary>
        /// Function to recalculate number of k edges
        /// and AM, AMAM, AMAMAM k edges
        /// It is done by finding all triangles upon all edges
        /// and counting <c>k</c> for every edge
        /// Then summing function for AM, AMAM, AMAMAM k edges is called
        /// </summary>
        private void ReCalculateKEdges()
        {
            int kEdgesPicked = (int)KhranyUpDown.Value;

            var kEdgdesValues = Enumerable.Repeat(0, kEdgesPicked + 1).ToArray();

            Dictionary<(Vertex, Vertex), bool> visited = new Dictionary<(Vertex, Vertex), bool>();

            foreach(var from in graphCoordinates.neighbors.Keys)
            {
                foreach(var to in graphCoordinates.neighbors.Keys)
                {
                    visited[(from, to)] = false;
                }
            }

            foreach(var (from, value) in graphCoordinates.neighbors)
            {
                foreach (var to in value)
                {
                    if (visited[(from, to)]) continue;
                    visited[(from, to)] = true;
                    visited[(to, from)] = true;

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

                    if(sum <= kEdgesPicked) // only needed this ones
                        kEdgdesValues[sum]++;

                    var edge = FindEdgeFromVertices(from, to);

                    if (sum < 0)
                        throw new ArgumentException("You cannot add edge between already connected vertices");

                    foreach (var line in edge.lines)
                        line.Stroke = colors[sum];
                }
            }



            CalculateAMEdgesAndPrint(kEdgdesValues, kEdgesPicked);
            textBlockKEdges.Text = $"K hran velikosti {kEdgesPicked} je {kEdgdesValues[kEdgesPicked]}    ";

            RedrawGraph(graphCoordinates, 1);

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

            if (statesCalculation[StateCalculation.AMKEdges])
                textBlockAMKEdges.Text = $"AM K hran velikost {size} je {AMKEdgesArray[0, size]}";
            else if (statesCalculation[StateCalculation.AMAMKEdges])
                textBlockAMKEdges.Text = $"AMAM K hran velikost {size} je {AMKEdgesArray[1, size]}";
            else if (statesCalculation[StateCalculation.AMAMAMKEdges])
                textBlockAMKEdges.Text = $"AMAMAM K hran velikost {size} je {AMKEdgesArray[2, size]}";
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
                graphCoordinates.neighbors.Add(vertex, new List<Vertex>());
            }

           
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

            /*Updating vertices*/
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

            /*Updating edges*/

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

            
            /* Updating neighbors */
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
