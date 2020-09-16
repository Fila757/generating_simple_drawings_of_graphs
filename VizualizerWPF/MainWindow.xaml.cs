using System;
using System.Collections.Generic;
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

namespace VizualizerWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    enum StateChanging { Adding, Removing, None };
    enum StateCalculation { Intersections, KEdges, AMKEdges, AMAMKEdges, AMAMAMKEdges}; //AM = AtMost

    class CollisionDetection
    {

        static MainWindow window;

        public static void Init(MainWindow window)
        {
            CollisionDetection.window = window;
        } 

        static double epsilon = 0.5;

        public static Coordinates TwoLines(Line line1, Line line2)
        {
            var denominator = (line2.Y2 - line2.Y1) * (line1.X2 - line1.X1) - (line2.X2 - line2.X1) * (line1.Y2 - line1.Y1);

            var numT = -line1.X1 * (line2.Y2 - line2.Y1) + line1.Y1 * (line2.X2 - line2.X1) +
                 line2.X1 * (line2.Y2 - line2.Y1) - line2.Y1 * (line2.X2 - line2.X1);
            var numS = -line1.X1 * (line1.Y2 - line1.Y1) + line1.Y1 * (line1.X2 - line1.X1) +
                line2.X1 * (line1.Y2 - line1.Y1) - line2.Y1 * (line1.X2 - line1.X1);
            

            var s = numS / denominator;
            var t = numT / denominator;
            //MessageBox.Show(denominator.ToString() + " " +  numT.ToString() + " " + numS.ToString());
            if(denominator != 0 && (s >= 0 && s <= 1) && (t >= 0 && t <= 1))
                return new Coordinates { x = line1.X1 + (line1.X2 - line1.X1) * t, y = line1.Y1 + (line1.Y2 - line1.Y1) * t };
            return new Coordinates { x = -1, y = -1 };
        }

        public static bool LineAndEllipseAtEnd(Line line, Ellipse ellipse)
        {
            var intersection1 = (line.X1 - (ellipse.Margin.Left + window.sizeOfVertex / 2)) * (line.X1 - (ellipse.Margin.Left + window.sizeOfVertex / 2)) +
                (line.Y1 - (ellipse.Margin.Top + window.sizeOfVertex / 2)) * (line.Y1 - (ellipse.Margin.Top + window.sizeOfVertex / 2));

            var intersection2 = (line.X2 - (ellipse.Margin.Left + window.sizeOfVertex / 2)) * (line.X2 - (ellipse.Margin.Left + window.sizeOfVertex / 2)) +
                (line.Y2 - (ellipse.Margin.Top + window.sizeOfVertex / 2)) * (line.Y2 - (ellipse.Margin.Top + window.sizeOfVertex / 2));

            if (intersection1 <= window.sizeOfVertex * window.sizeOfVertex + epsilon || intersection2 <= window.sizeOfVertex * window.sizeOfVertex + epsilon)
                return true;

            return false;
        }

        public static bool LineAndEllipse(Line line, Ellipse ellipse)
        {
            return new Coordinates { x = -1, y = -1 } ==
                TwoLines(line,
                new Line
                {
                    X1 = ellipse.Margin.Left,
                    Y1 = ellipse.Margin.Top,
                    X2 = ellipse.Margin.Left + window.sizeOfVertex,
                    Y2 = ellipse.Margin.Top + window.sizeOfVertex
                }) ? false : true;
        }
    }

    public partial class MainWindow : Window
    {
        StateChanging stateChanging = StateChanging.None;
        Dictionary<StateCalculation, bool> statesCalculation = new Dictionary<StateCalculation, bool>();

        GraphGenerator graphGenerator;

        double cx, cy;
        public double sizeOfVertex;
        double scale;

        List<Coordinates> selectedVertices = new List<Coordinates>();

        public MainWindow()
        {
            foreach (StateCalculation state in Enum.GetValues(typeof(StateCalculation)))
                statesCalculation[state] = false;

            InitializeComponent();

            StateChanged += new EventHandler(ResizeWindowEvent);

            cx = 200; cy = 200;
            sizeOfVertex = 15;
            scale = 1.5;

            CollisionDetection.Init(this);

            graphGenerator = new GraphGenerator(5);
            var graphCoordinates = graphGenerator.GenerateNextDrawing();
            DrawGraph(graphCoordinates, 1);

        }

        private void ResizeWindowEvent(object sender, EventArgs e)
        {
            if(WindowState == WindowState.Normal)
            {
                cx = cx / scale; cy = cy / scale; sizeOfVertex = sizeOfVertex / scale;
                mainCanvas.Children.Clear();
                DrawGraph(graphGenerator.GetTopicalDrawing(), 1);
            }

            if(WindowState == WindowState.Maximized)
            {
                cx = cx * scale; cy = cy * scale; sizeOfVertex = sizeOfVertex * scale;
                mainCanvas.Children.Clear();
                DrawGraph(graphGenerator.GetTopicalDrawing(), scale);
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

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void KedgesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void NextDrawing_Click(object sender, RoutedEventArgs e)
        {
            var graphCoordinates = graphGenerator.GenerateNextDrawing();

            mainCanvas.Children.Clear();
            DrawGraph(graphCoordinates, WindowState ==  WindowState.Maximized ? scale : 1);
        }

        private void ellipse_MouseDown(object sender, RoutedEventArgs e)
        {
            Ellipse ellipse = sender as Ellipse;
            //MessageBox.Show(new { x = ellipse.Margin.Left, y= ellipse.Margin.Top }.ToString());


            if (stateChanging == StateChanging.Adding)
            {
                selectedVertices.Add(new Coordinates { x = ellipse.Margin.Left, y = ellipse.Margin.Top });
                if (selectedVertices.Count == 2)
                {
                    var line = new Line
                    {
                        X1 = selectedVertices[0].x + sizeOfVertex / 2,
                        Y1 = selectedVertices[0].y + sizeOfVertex / 2,
                        X2 = selectedVertices[1].x + sizeOfVertex / 2,
                        Y2 = selectedVertices[1].y + sizeOfVertex / 2,
                        Stroke = Brushes.Red,
                        StrokeThickness = sizeOfVertex / 3
                    };
                    line.MouseDown += line_MouseDown;
                    Panel.SetZIndex(line, 1);
                    mainCanvas.Children.Add(line);

                    selectedVertices.Clear();


                    List<Ellipse> intersections = new List<Ellipse>();
                    foreach (var l in mainCanvas.Children.OfType<Line>())
                    {
                        if (l == line) continue;

                        var intersection = CollisionDetection.TwoLines(line, l);
                        if(!(intersection.x == -1 && intersection.y == -1))
                        {
                            var ellipse2 = new Ellipse
                            {
                                Width = sizeOfVertex,
                                Height = sizeOfVertex,
                                Fill = Brushes.Green,
                                Margin = new Thickness(intersection.x - sizeOfVertex / 2, intersection.y - sizeOfVertex / 2, 0, 0),
                                Visibility = statesCalculation[StateCalculation.Intersections] ? Visibility.Visible : Visibility.Hidden
                            };
                            ellipse2.MouseDown += ellipse_MouseDown;
                            Panel.SetZIndex(ellipse2, 10);
                            intersections.Add(ellipse2);

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
                    if (CollisionDetection.LineAndEllipseAtEnd(line, ellipse))
                        intersectedLines.Add(line);
                }

                /* removing edges */
                foreach(var line in intersectedLines)
                    mainCanvas.Children.Remove(line);

                /* removing vertex */
                mainCanvas.Children.Remove(ellipse);

            }
        }

        private void line_MouseDown(object sender, RoutedEventArgs e)
        {
            Line line = sender as Line;
            if (stateChanging == StateChanging.Removing)
            {

                var removedIntersections = new List<Ellipse>();
                foreach(var ellipse in mainCanvas.Children.OfType<Ellipse>())
                {
                    if (CollisionDetection.LineAndEllipse(line, ellipse)
                        && ellipse.Fill == Brushes.Green)
                        removedIntersections.Add(ellipse);

                }

                foreach (var ellipse in removedIntersections)
                    mainCanvas.Children.Remove(ellipse);

                mainCanvas.Children.Remove(line);
            }
        }

        private void canvas_MouseDown(object sender, RoutedEventArgs e)
        {

            //MessageBox.Show(mainCanvas.ActualWidth.ToString());
            //MessageBox.Show(MainWindow.MaxHeightProperty.);

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
                    
            }

           
        }

        void DrawGraph(GraphCoordinates graphCoordinates, double scale)
        {
            foreach (var vertex in graphCoordinates.vertices)
            {
                var ellipse = new Ellipse
                {
                    Width = sizeOfVertex,
                    Height = sizeOfVertex,
                    Fill = vertex.Item2 == VertexState.Regular ? Brushes.Blue : Brushes.Green,
                    Margin = new Thickness(scale * vertex.Item1.x + cx, scale * vertex.Item1.y + cy, 0, 0)

                };
                ellipse.MouseDown += ellipse_MouseDown;
                mainCanvas.Children.Add(ellipse);

                Panel.SetZIndex(ellipse, vertex.Item2 == VertexState.Regular ? 100 : 10);
            }
            foreach (var edge in graphCoordinates.edges)
            {
                var line = new Line
                {
                    X1 = scale * edge.from.x + cx + sizeOfVertex / 2,
                    Y1 = scale * edge.from.y + cy + sizeOfVertex / 2,
                    X2 = scale * edge.to.x + cx + sizeOfVertex / 2,
                    Y2 = scale * edge.to.y + cy + sizeOfVertex / 2,
                    Stroke = Brushes.Red,
                    StrokeThickness = sizeOfVertex / 3
                };
                Panel.SetZIndex(line, 1);
                line.MouseDown += line_MouseDown;
                mainCanvas.Children.Add(line);
            }
          
        }
       
    }

    struct Coordinates
    {
        public double x;
        public double y;

        public static bool operator ==(Coordinates a, Coordinates b)
        {
            return a.x == b.x && a.y == b.y;
        }

        public static bool operator !=(Coordinates a, Coordinates b)
        {
            return !(a == b);
        }

    }

    struct Edge
    {
        public Coordinates from;
        public Coordinates to;

      
    }

    enum VertexState { Intersection, Regular};

    class GraphCoordinates
    {
        public List<Tuple<Coordinates, VertexState> > vertices = new List<Tuple<Coordinates, VertexState> >();
        public List<Edge> edges = new List<Edge>();
    }

    class GraphGenerator
    {
        StreamReader streamReader;
        int SizeOfGraph { get; set; }

        GraphCoordinates graphCoordinates = new GraphCoordinates();

        public GraphGenerator(int n)
        {
            SizeOfGraph = n;
            streamReader = new StreamReader(
                "C:/Users/filip/source/repos/generating-simple-drawings-of-graphs/VizualizerWPF/data/graph" + SizeOfGraph + ".txt");
        }

        GraphCoordinates ReadUntillNextRS()
        {
            graphCoordinates = new GraphCoordinates();
            HashSet<Tuple<Coordinates, VertexState> > vertices = new HashSet<Tuple<Coordinates, VertexState> >();

            string line;
            while(!String.Equals((line = streamReader.ReadLine()), "#") && line != null)
            {
                string[] temp = line.Split();

                for (int i = 0; i < temp.Length / 4; i++)
                {
                    var stateFrom = i == 0 ? VertexState.Regular : VertexState.Intersection;
                    var stateTo = i == temp.Length / 4 - 1 ? VertexState.Regular : VertexState.Intersection;
                    graphCoordinates.edges.Add(
                        new Edge
                        {
                            from = new Coordinates { x = Double.Parse(temp[4*i + 0]), y = Double.Parse(temp[4*i + 1]) },
                            to = new Coordinates { x = Double.Parse(temp[4*i + 2]), y = Double.Parse(temp[4*i + 3]) }
                        });
                    vertices.Add(Tuple.Create(graphCoordinates.edges[graphCoordinates.edges.Count - 1].from, stateFrom));
                    vertices.Add(Tuple.Create(graphCoordinates.edges[graphCoordinates.edges.Count - 1].to, stateTo));
                }
            }

            graphCoordinates.vertices = vertices.ToList();

            return graphCoordinates;
        }

        public GraphCoordinates GetTopicalDrawing()
        {
            return graphCoordinates;
        }

        public GraphCoordinates GenerateNextDrawing()
        {
            return ReadUntillNextRS();
        }
    }
}
