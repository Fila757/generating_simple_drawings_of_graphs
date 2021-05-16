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
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Windows.Media.Animation;

/// <summary>
/// In MainWindow.xaml we have one <c>mainCanvas</c>, topbar for setting constant and resizing/closing the window
/// There are also some information about a drawing. To operate with the drawing, there are some buttons to help us, adding, removing, ...
/// Then values we want to consider are shown, for more see User's guide
/// </summary>

namespace VizualizerWPF
{



   
    enum StateAutoMoving { Auto, None };

    /// <summary>
    /// States to operate with a drawings
    /// </summary>
    enum StateChanging { AddingPolyline, Adding, Removing, Invariant, None };

    /// <summary>
    /// Enum for showing Intersections or changing reference face
    /// </summary>
    public enum StateCalculation { Intersections, ReferenceFace}; //AM = AtMost

    public partial class MainWindow : Window//, INotifyPropertyChanged
    {
       
 
        StateChanging stateChanging = StateChanging.None;
        public Dictionary<StateCalculation, bool> statesCalculation = new Dictionary<StateCalculation, bool>();

        StateAutoMoving stateAutoMoving = StateAutoMoving.None;
        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
        /// <summary>
        /// It is class for generating all drawings
        /// </summary>
        GraphGenerator graphGenerator;

        /// <summary>
        /// Shift from 0,0 origin
        /// Shift from 0,0 origin
        /// </summary>
        double cx = 81.25, cy = 219.75; 
        public double sizeOfVertex;
        public double scale;

        /// <summary>
        /// Actual Height and Actual Width of canvas due to initialization set manually
        /// </summary>
        double actualHeight = 500;
        double actualWidth = 1000;
        /// <summary>
        /// Resizing paramater to get a drawing little bit from borders
        /// </summary>
        double lambda = 1.1;

        /// <summary>
        /// Default point determining outer face</param>
        /// </summary>


        /// <summary>
        /// Whether we use custom files at moment.
        /// </summary>
        bool savedGraphs = false;


        /// <summary>
        /// Set of vertices for which we count invariant edges
        /// </summary>

        List<Vertex> invariantWithRescpectTo = new List<Vertex>();

        /// <summary>
        /// Smoothing constant determining the number of iterations of force-directed algorithm
        /// </summary>
        int Smoothing => SmoothingTextBox.Text.Length == 0 ? 0 : (int)Double.Parse(SmoothingTextBox.Text);

        /// <summary>
        /// List of vertices forming new edge
        /// </summary>
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

        public List<object> PrintingLines(List<Line> listLines)
        {
            List<object> lines = new List<object>();
            foreach (var line in listLines)
            {
                lines.Add(new { line.X1, line.Y1, line.X2, line.Y2 });
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

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"[^0-9\.]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void SmoothingChecker(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void MinMaxValueEdgeChecker(object sender, TextCompositionEventArgs e)
        {
            if ((sender as TextBox).Text.Length != 0)
            {
                e.Handled = true;
            }
            else
            {
                e.Handled = (!e.Text.Any(x => (Char.IsDigit(x))) || !e.Text.Any(x => x - '0' >= 0 && 8 >= x - '0'));
            }
            
        }

        private void MinMaxValueVertexChecker(object sender, TextCompositionEventArgs e)
        {
            if ((sender as TextBox).Text.Length != 0)
            {
                e.Handled = true;
            }
            else
            {
                e.Handled = (!e.Text.Any(x => (Char.IsDigit(x))) || !e.Text.Any(x => x - '0' >= 4 && 8 >= x - '0'));
            }
        }

        public MainWindow()
        {
            foreach (StateCalculation state in Enum.GetValues(typeof(StateCalculation)))
                statesCalculation[state] = false;

            statesCalculation[StateCalculation.Intersections] = true;

            InitializeComponent();
            DataContext = this;

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

            InitializeRightValuesOfKedges();



            graphGenerator = new GraphGenerator((int)Double.Parse(VerticesTextBox.Text));
            graphCoordinates = graphGenerator.GenerateNextDrawing();
            numberOfDrawing.Text = graphGenerator.counter.ToString();

            MakeSmootherAndDraw();


        }


      

       


      
        /// <summary>
        /// Function to initialize wanted values
        /// </summary>
        private void InitializeRightValuesOfKedges()
        {
            string atMost = "kEdges";
            for(int i = 1;i <= 4; i++)
            {
                for(int k = 0; k <= 8; k++)
                {
                    TextBlock textBlock = FindName($"{atMost}{k}RightValue") as TextBlock;
                    textBlock.Text = (3 * CustomMath.CombCoeff(k + i, i)).ToString();
                }
                switch (i)
                {
                    case 1:
                        atMost = "amKEdges";
                        break;
                    case 2:
                        atMost = "amAmKEdges";
                        break;
                    case 3:
                        atMost = "amAmAmKEdges";
                        break;
                }
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            // Begin dragging the window
            this.DragMove();
        }

        /// <summary>
        /// Function to set width and height of canvas due to dynamic setting (at the beggining, it is 0) 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Canvas_Loaded(object sender, RoutedEventArgs e)
        {
            actualWidth = (ActualWidth * 5) / 7;
            actualHeight = ActualHeight - 30;

            mainCanvas.Children.Add(new Line { StrokeThickness = 10, Fill = Brushes.Black, X1 = 0, Y1 = 0, X2 = actualWidth, Y2 = 0 });
            mainCanvas.Children.Add(new Line { StrokeThickness = 10, Fill = Brushes.Black, X1 = 0, Y1 = actualHeight, X2 = actualWidth, Y2 = actualHeight });
        }


        /// <summary>
        /// Shortening all edges to redraw uselessly long edges after force-directed algorithm
        /// </summary>
        private void MakeAllLinesNotSharp()
        {
            
            foreach (var edge in graphCoordinates.edges)
            {
                edge.Shorten(graphCoordinates);

            }
            CreateVerticesFromPoints(graphCoordinates);
            
            
        }

        /// <summary>
        /// Update the vertices to contains only the points after shortening
        /// </summary>
        /// <param name="graphCoordinates"></param>
        private void CreateVerticesFromPoints(GraphCoordinates graphCoordinates)
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
        

        /// <summary>
        /// Subdivide edges, apply force-directed algorithm Smoothing-times, shorten the edges and then draw on canvas
        /// </summary>
        private void MakeSmootherAndDraw()
        {
            SubDivideEdges(graphCoordinates);
            for (int i = 0; i < Smoothing; i++)
            {
                graphCoordinates = ForceDirectedAlgorithms.CountAndMoveByForces(graphCoordinates);
            }
            MakeAllLinesNotSharp();
            DrawGraph(graphCoordinates, 1);
        }

        /// <summary>
        /// Get the value of Z(n)
        /// </summary>
        /// <returns></returns>
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
            double frequence = Double.TryParse(AutoTextBox.Text, out frequence) ? frequence : 1;
            dispatcherTimer.Interval = TimeSpan.FromSeconds(1 / frequence);
            if(stateAutoMoving == StateAutoMoving.Auto)
                NextDrawing_Click(sender, new RoutedEventArgs());
        }

        /// <summary>
        /// Function to redraw the drawing (when canvas resize)
        /// </summary>
        /// <param name="graphCoordinates">To store graph topical</param>
        /// <param name="scale">To know if it is maximized or minimized</param>
        private void RedrawGraph(GraphCoordinates graphCoordinates)
        {
            mainCanvas.Children.Clear();

            (double, double, double, double) coordinatesAndScale = FindClipingSizes(graphCoordinates.GetAllPoints());

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
        /// <summary>
        /// Function to change the state of Automoving Button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void AutoButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            button.Background = stateAutoMoving == StateAutoMoving.Auto ?
              (SolidColorBrush)(new BrushConverter().ConvertFrom("#673ab7")) : Brushes.BlueViolet;

            stateAutoMoving = stateAutoMoving == StateAutoMoving.Auto ? StateAutoMoving.None : StateAutoMoving.Auto;
        }

        /// <summary>
        /// Function to change the state of Adding Button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Adding_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            /*changing color*/
            button.Background = stateChanging == StateChanging.Adding ?
                (SolidColorBrush)(new BrushConverter().ConvertFrom("#673ab7")) : Brushes.Red;
            Removing.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#673ab7"));
            AddingPolyline.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#673ab7"));
            Invariant.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#673ab7"));

            stateChanging = stateChanging == StateChanging.Adding ? StateChanging.None : StateChanging.Adding;
        }

        /// <summary>
        /// Function to change the state of Removing Button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Removing_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            /*changing color*/
            button.Background = stateChanging == StateChanging.Removing ?
                (SolidColorBrush)(new BrushConverter().ConvertFrom("#673ab7")) : Brushes.Red;
            Adding.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#673ab7"));
            AddingPolyline.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#673ab7"));
            Invariant.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#673ab7"));

            stateChanging = stateChanging == StateChanging.Removing ? StateChanging.None : StateChanging.Removing;

        }

        /// <summary>
        /// Function to change the state of AddingPolyline Button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddingPolyline_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            /*changing color*/
            button.Background = stateChanging == StateChanging.AddingPolyline ?
                (SolidColorBrush)(new BrushConverter().ConvertFrom("#673ab7")) : Brushes.Red;
            Adding.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#673ab7"));
            Removing.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#673ab7"));
            Invariant.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#673ab7"));

            stateChanging = stateChanging == StateChanging.AddingPolyline ? StateChanging.None : StateChanging.AddingPolyline;

        }

        /// <summary>
        /// Function to change the state of Invariant Button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Invariant_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            /*changing color*/
            button.Background = stateChanging == StateChanging.Invariant ?
                (SolidColorBrush)(new BrushConverter().ConvertFrom("#673ab7")) : Brushes.Red;
            Adding.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#673ab7"));
            Removing.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#673ab7"));
            AddingPolyline.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#673ab7"));

            stateChanging = stateChanging == StateChanging.Invariant ? StateChanging.None : StateChanging.Invariant;
            if (stateChanging != StateChanging.Invariant)
            {
                MakeAllVerticesBlue();
                invariantWithRescpectTo = new List<Vertex>();
                ZeroInvariantEdgesValues();
            }


        }


        private void MakeAllVerticesBlue()
        {
            foreach (var vertex in graphCoordinates.neighbors.Keys)
                vertex.ellipse.Fill = Brushes.Blue;
        }

        /// <summary>
        /// Function to change the state of Face button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RefferenceFace_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            button.Background = button.Background == Brushes.Red ?
               (SolidColorBrush)(new BrushConverter().ConvertFrom("#673ab7")) : Brushes.Red;

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
                (SolidColorBrush)(new BrushConverter().ConvertFrom("#673ab7")) : Brushes.Red;

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


        }

        /// <summary>
        /// Function to save the current drawing when we click on Save Button 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveDrawing_Click(object sender, RoutedEventArgs e)
        {
            graphCoordinates.SaveCoordinates();
        }

        /// <summary>
        /// Function to flush the data when we change to custom file working
        /// </summary>
        private void FlushFromBackUpToNormal()
        {
            using (var sr = new StreamReader(@"../../../data/savedGraphsBackUp.txt"))
            {
                using (var sw = new StreamWriter(@"../../../data/savedGraphs.txt", append: true))
                {
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();
                        sw.WriteLine(line);
                    }
                }
            }

            File.WriteAllText(@"../../../data/savedGraphsBackUp.txt", string.Empty);
        }

        /// <summary>
        /// Function to generate new drawing of clique from data
        /// If value of vertices textbox has changed then new data file is loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NextDrawing_Click(object sender, RoutedEventArgs e)
        {


            if ((bool)faceCheckBox.IsChecked)
            {
                GraphCoordinates.facePoint = GraphCoordinates.farFarAway; //changing again to the outer face
            }

            if ((bool)savedGraphsChechBox.IsChecked && !savedGraphs)
            {
                graphGenerator.CloseFile();
                savedGraphs = true;
                graphGenerator = new GraphGenerator();
            }

            else if (
                (!(bool)savedGraphsChechBox.IsChecked && graphGenerator.SizeOfGraph != Int32.Parse(VerticesTextBox.Text)) 
                || 
                (savedGraphs && !(bool)savedGraphsChechBox.IsChecked)
                )
            {
                graphGenerator.CloseFile();
                savedGraphs = false;

                FlushFromBackUpToNormal();

                graphGenerator = new GraphGenerator((int)Double.Parse(VerticesTextBox.Text));
            }
                
            graphCoordinates = graphGenerator.GenerateNextDrawing();
            numberOfDrawing.Text = graphGenerator.counter.ToString();

            mainCanvas.Children.Clear();


            MakeSmootherAndDraw();


        }

        /// <summary>
        /// Function to generate previous drawing of clique from data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PreviousDrawing_Click(object sender, RoutedEventArgs e)
        {
            if(!(bool)faceCheckBox.IsChecked)
                GraphCoordinates.facePoint = GraphCoordinates.farFarAway;

            int tmpCounter = graphGenerator.counter;

            if (tmpCounter == 1)
            {
                MessageBox.Show("There is no previous one");
                return;
            }

            graphGenerator.CloseFile();
            graphGenerator = !(bool)savedGraphsChechBox.IsChecked ? new GraphGenerator((int)Double.Parse(VerticesTextBox.Text)) : new GraphGenerator();
            graphGenerator.counter = tmpCounter;

            graphCoordinates = graphGenerator.GeneratePreviousDrawing();
            numberOfDrawing.Text = graphGenerator.counter.ToString();

            mainCanvas.Children.Clear();
            //DrawGraph(graphCoordinates, WindowState == WindowState.Maximized ? scale : 1);

            MakeSmootherAndDraw();

        }



        public Vertex? FindVertexSave(Point center)
        {
            foreach (var vertex in graphCoordinates.vertices)
            {
                if (Vertex.Compare(vertex.center, center))
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

            var first = graphCoordinates.FindVertex(edge.points[0]);
            var second = graphCoordinates.FindVertex(edge.points.Last());

            return (first, second);
        }

        private void AddIntersectionsWithLines(List<Line> lines)
        {
            HashSet<Vertex> intersections = new HashSet<Vertex>( );
            foreach (var line in lines)
            {
                foreach (var l in LinesIterator())
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
                    if (graphCoordinates.FindEdgeFromVertices(selectedVertices.ElementAt(0), selectedVertices.ElementAt(1)) != null)
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

                    graphCoordinates.DeleteEdgeFromDictionary(start, tempEdge);
                    graphCoordinates.DeleteEdgeFromDictionary(end, tempEdge);

                    RemoveIntersectionsAndMiddleOnes(tempEdge);

                    graphCoordinates.edges.Remove(tempEdge);
                }

                /* removing vertex */
                if (ellipse.Fill == Brushes.Blue) //intersection are already deleted
                {
                    mainCanvas.Children.Remove(ellipse);
                    graphCoordinates.neighbors.Remove(graphCoordinates.FindVertex(ellipse, sizeOfVertex));
                    graphCoordinates.vertices.Remove(graphCoordinates.FindVertex(ellipse, sizeOfVertex));
                }
            }

            ZeroInvariantEdgesValues();

            if (stateChanging == StateChanging.Invariant)
            {

                var vertex = graphCoordinates.FindVertex(ellipse, sizeOfVertex);

                if (vertex.state == VertexState.Regular)
                {
                    if (ellipse.Fill == Brushes.Purple)
                    {
                        invariantWithRescpectTo.Remove(vertex);
                        ellipse.Fill = Brushes.Blue;
                    }
                    else
                    {
                        invariantWithRescpectTo.Add(vertex);
                        ellipse.Fill = Brushes.Purple;
                    }
                    RecalculateKEdgesAndUpdateButtonTexts(invariantWithRescpectTo);
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
        /// Function to remove all intersection with <c>line</c> from canvas and coordinates
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
                graphCoordinates.vertices.Remove(graphCoordinates.FindVertex(ellipse, sizeOfVertex));
            }
        }


        void RemoveIntersectionsAndMiddleOnes(Edge edge)
        {
            foreach (var l in edge.lines)
            {
                RemoveIntersections(l); //only intersections, middle ones remove later
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
        /// If remove button activated then the edge is removed
        /// If Invariant button is activated then invariant edges are counted for this edge
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

                graphCoordinates.DeleteEdgeFromDictionary(start, tempEdge);
                graphCoordinates.DeleteEdgeFromDictionary(end, tempEdge);

                RemoveIntersectionsAndMiddleOnes(tempEdge);

                graphCoordinates.edges.Remove(tempEdge);

            }

            ZeroInvariantEdgesValues();
         
            if (stateChanging == StateChanging.Invariant)
            {
                var withoutEdge = FindEdge(line);
                RecalculateKEdgesAndUpdateButtonTexts(null, withoutEdge);
            }
            else
            {
                UpdateStats();
            }

        }



        
        
        void RecalculateKEdgesAndUpdateButtonTexts(List<Vertex> withouts = null, Edge withoutEdge = null)  
        {

            (var kEdgesValues, var invariantKEdges) = graphCoordinates.ReCalculateKEdges(withouts, withoutEdge);
            UpdateButtonTexts(withouts, withoutEdge, kEdgesValues, invariantKEdges);
        }

        /// <summary>
        /// Function to update buttons values by <c>kEdgesValues</c> and <c>invariantKEdges</c> values 
        /// depending on invariant state
        /// </summary>
        /// <param name="withouts"></param>
        /// <param name="withoutEdge"></param>
        /// <param name="kEdgesValues"></param>
        /// <param name="invariantKEdges"></param>
        void UpdateButtonTexts(List<Vertex> withouts, Edge withoutEdge, int [] kEdgesValues, int [] invariantKEdges)
        {
            if (withouts == null && withoutEdge == null)
            {
                var AMKEdgesArray = CustomMath.CalculateAmEdges(GraphCoordinates.maximalKEdges, kEdgesValues);

                PrintAMKEdges(GraphCoordinates.maximalKEdges, AMKEdgesArray);

                for (int i = 0; i <= GraphCoordinates.maximalKEdges; i++)
                {
                    TextBlock textBlock = FindName($"kEdges{i}") as TextBlock;
                    textBlock.Text = kEdgesValues[i].ToString();
                }
            }

            else
            {
                var invariantAmKEdges = Enumerable.Repeat(0, GraphCoordinates.maximalKEdges + 1).ToArray();

                invariantAmKEdges[0] = invariantKEdges[0];
                TextBlock textBlock = FindName($"invariantAmKedges{0}") as TextBlock;
                textBlock.Text = invariantAmKEdges[0].ToString();

                for (int i = 1; i <= GraphCoordinates.maximalKEdges; i++)
                {
                    invariantAmKEdges[i] = invariantAmKEdges[i - 1] + invariantKEdges[i];
                    textBlock = FindName($"invariantAmKedges{i}") as TextBlock;
                    textBlock.Text = invariantAmKEdges[i].ToString();
                }

                int amAmInvariant = 0;

                for (int i = 0; i <= GraphCoordinates.maximalKEdges; i++)
                {
                    amAmInvariant += invariantAmKEdges[i];
                    textBlock = FindName($"invariantAmAmKedges{i}") as TextBlock;
                    textBlock.Text = amAmInvariant.ToString();
                }

                if (withoutEdge != null)
                {

                    int second, third;

                    textBlock = removedEdgeAMAMAMSecond;
                    textBlock.Text = (FindName($"invariantAmAmKedges{Int32.Parse(kWhenRemovingUpDown.Text)}") as TextBlock).Text.ToString();
                    second = Int32.Parse(textBlock.Text);


                    textBlock = removedEdgeAMAMAMThird;
                    textBlock.Text =
                        ((((Int32.Parse(kWhenRemovingUpDown.Text) + 2) - withoutEdge.kEdge)
                        *
                        ((Int32.Parse(kWhenRemovingUpDown.Text) + 1) - withoutEdge.kEdge)) / 2).ToString();
                    third = Int32.Parse(textBlock.Text);

                    removedEdgeAMAMAMFirst.Text = (Int32.Parse((FindName($"amAmAmKEdges{Int32.Parse(kWhenRemovingUpDown.Text)}") as TextBlock).Text) - second - third).ToString();



                    textBlock = removedEdgeAMAMSecond;
                    textBlock.Text = (FindName($"invariantAmKedges{Int32.Parse(kWhenRemovingUpDown.Text)}") as TextBlock).Text.ToString();
                    second = Int32.Parse(textBlock.Text);


                    textBlock = removedEdgeAMAMThird;
                    textBlock.Text = ((Int32.Parse(kWhenRemovingUpDown.Text) + 1) - withoutEdge.kEdge).ToString();
                    third = Int32.Parse(textBlock.Text);

                    removedEdgeAMAMFirst.Text = (Int32.Parse((FindName($"amAmKEdges{Int32.Parse(kWhenRemovingUpDown.Text)}") as TextBlock).Text) - second - third).ToString();


                }

            }
        }

     
        /// <summary>
        /// Function to restore values of invariant textblock to zeroes and redraw all lines to non-dashed
        /// </summary>
        void ZeroInvariantEdgesValues()
        {

            foreach(var edge in graphCoordinates.edges)
            {
                foreach(var line in edge.lines)
                    line.StrokeDashArray = DoubleCollection.Parse("");
            }

            for (int i = 0; i <= GraphCoordinates.maximalKEdges; i++)
            {
                var textBlock = FindName($"invariantAmKedges{i}") as TextBlock;
                textBlock.Text = 0.ToString();

                textBlock = FindName($"invariantAmAmKedges{i}") as TextBlock;
                textBlock.Text = 0.ToString();

            }            
        }

  

        /// <summary>
        /// Function to Print values of <c>AMKEdgesArray</c> to respective textblocks 
        /// Checking also the 3AMK theorem (a messagebox shows up when something wrong)
        /// </summary>
        /// <param name="size"></param>
        /// <param name="AMKEdgesArray"></param>
        private void PrintAMKEdges(int size, int[,] AMKEdgesArray)
        {

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
                {
                    mainCanvas.Children.Add(new Ellipse
                    {
                        Width = sizeOfVertex,
                        Height = sizeOfVertex,
                        Fill = Brushes.Turquoise,
                        Margin = new Thickness(GraphCoordinates.facePoint.X - sizeOfVertex / 2, GraphCoordinates.facePoint.Y - sizeOfVertex / 2, 0, 0)

                    });
                    MessageBox.Show("HEUREKA WRONG");

                   
                }
            }
 
        }

        /// <summary>
        /// Function to coun intersections and to update all other stats regading $k$-edges
        /// </summary>
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

            RecalculateKEdgesAndUpdateButtonTexts();
        }
        
        /// <summary>
        /// Function to add vertex when the place is empty and to detect a change of a reference face 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void canvas_MouseDown(object sender, RoutedEventArgs e)
        {


           //MessageBox.Show(Mouse.GetPosition(sender as IInputElement).ToString());
            if (e.OriginalSource is Ellipse || e.OriginalSource is Line)
                return;

            Point pos = Mouse.GetPosition(mainCanvas);
            //MessageBox.Show((mainCanvas.ActualWidth).ToString() + " " + pos.ToString());


            if (statesCalculation[StateCalculation.ReferenceFace])
            {
                GraphCoordinates.facePoint = pos;

                ZeroInvariantEdgesValues();
                UpdateStats();


                if (invariantWithRescpectTo.Count != 0)
                    RecalculateKEdgesAndUpdateButtonTexts(invariantWithRescpectTo);
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

                //MessageBox.Show((new { X = pos.X, Y = pos.Y }).ToString());
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

        /// <summary>
        /// Function to find appropriate shift and rescaling so all the <c>points</c> fit into the canvas
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>

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


        int divisionConst = 50;

        /*
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        */

        /// <summary>
        /// Function to connect consecutive points and get the line segments
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Function to subdivide line depending on the divisionConstant 
        /// (line is divided so that every line segment is now shorter than that divisionConstant)
        /// </summary>
        /// <param name="line"></param>
        /// <param name="graphCoordinates"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Function to subdivide all lines contained in the <c>edge</c>
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="graphCoordinates"></param>
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

        /// <summary>
        /// Function to subdivide all the edges in <c>graphCoordinates</c>
        /// </summary>
        /// <param name="graphCoordinates"></param>
        void SubDivideEdges(GraphCoordinates graphCoordinates)
        {
            foreach (var edge in graphCoordinates.edges)
                SubDivideEdge(edge, graphCoordinates);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
            else if(WindowState == WindowState.Normal)
                WindowState = WindowState.Maximized;
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void UpdateCanvasWidthWhenClose()
        {
            actualWidth = FirstColumn.ActualWidth;
        }

        private void UpdateCanvasWidthWhenOpen()
        {
            actualWidth = FirstColumn.ActualWidth + SecondColumn.ActualWidth;
        }

        private void ButtonOpenMenu_Click(object sender, RoutedEventArgs e)
        {
            OpenMenuButton.Visibility = Visibility.Collapsed;
            CloseMenuButton.Visibility = Visibility.Visible;

            var sb = (Storyboard)BottomGrid.FindResource("MenuOpen");
            sb.Begin();

            UpdateCanvasWidthWhenClose();
            RedrawGraph(graphCoordinates);

        }


        private void ButtonCloseMenu_Click(object sender, RoutedEventArgs e)
        {
            OpenMenuButton.Visibility = Visibility.Visible;
            CloseMenuButton.Visibility = Visibility.Collapsed;

            var sb = (Storyboard)BottomGrid.FindResource("MenuClose");
            sb.Begin();

            UpdateCanvasWidthWhenOpen();
            RedrawGraph(graphCoordinates);
        }



        /// <summary>
        /// Function to draw a graph even when the canvas is not loaded yet
        /// The main difference to ReDraw the graph that here we first create ellipses, lines and so on, because
        /// graphCoordinates consists only of Vertices and Edges only containing points after generating
        /// </summary>
        /// <param name="graphCoordinates">Class to store graph</param>
        /// <param name="scale"></param>
        void DrawGraph(GraphCoordinates graphCoordinates, double scale)
        {

            mainCanvas.Children.Clear();

            double scaleX = scale, scaleY = scale;

            if (!(actualWidth == 0 && actualHeight == 0))
            {

                (double, double, double, double) coordinatesAndScale = FindClipingSizes(graphCoordinates.GetAllPoints());

                cx = coordinatesAndScale.Item1;
                cy = coordinatesAndScale.Item2;
                scaleX = coordinatesAndScale.Item3;
                scaleY = coordinatesAndScale.Item4;
            }
            
                
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
