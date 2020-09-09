using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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


    public partial class MainWindow : Window
    {
        StateChanging stateChanging = StateChanging.None;
        Dictionary<StateCalculation, bool> statesCalculation = new Dictionary<StateCalculation, bool>();

        GraphGenerator graphGenerator;

        int cx, cy;
        int sizeOfVertex;

        List<Coordinates> selectedVertices = new List<Coordinates>();

        public MainWindow()
        {
            foreach (StateCalculation state in Enum.GetValues(typeof(StateCalculation)))
                statesCalculation[state] = false;

            InitializeComponent();

            cx = 200; cy = 200;
            sizeOfVertex = 15;

            graphGenerator = new GraphGenerator(4);
            var graphCoordinates = graphGenerator.GenerateNextDrawing();
            DrawGraph(graphCoordinates);
            //Measure();
            //Arrange();

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
            DrawGraph(graphCoordinates);
        }

        private void ellipse_MouseDown(object sender, RoutedEventArgs e)
        {
            Ellipse ellipse = sender as Ellipse;
            if (stateChanging == StateChanging.Adding)
            {
                selectedVertices.Add(new Coordinates { x = ellipse.Margin.Left, y = ellipse.Margin.Top });
                if(selectedVertices.Count == 2)
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
                    mainCanvas.Children.Add(line);

                    selectedVertices.Clear();
                }
            }
        }

        private void line_MouseDown(object sender, RoutedEventArgs e)
        {
            Line line = sender as Line;
            if (stateChanging == StateChanging.Removing)
            {
                mainCanvas.Children.Remove(line);
            }
        }

        void DrawGraph(GraphCoordinates graphCoordinates)
        {
            foreach (var vertex in graphCoordinates.vertices)
            {
                var ellipse = new Ellipse
                {
                    Width = sizeOfVertex,
                    Height = sizeOfVertex,
                    Fill = Brushes.Blue,
                    Margin = new Thickness(vertex.x + cx, vertex.y + cy, 0, 0)
                };
                ellipse.MouseDown += ellipse_MouseDown;
                mainCanvas.Children.Add(ellipse);

                Panel.SetZIndex(ellipse, 1);
            }
            foreach (var edge in graphCoordinates.edges)
            {
                var line = new Line
                {
                    X1 = edge.from.x + cx + sizeOfVertex / 2,
                    Y1 = edge.from.y + cy + sizeOfVertex / 2,
                    X2 = edge.to.x + cx + sizeOfVertex / 2,
                    Y2 = edge.to.y + cy + sizeOfVertex / 2,
                    Stroke = Brushes.Red,
                    StrokeThickness = sizeOfVertex / 3
                };
                line.MouseDown += line_MouseDown;
                mainCanvas.Children.Add(line);
            }
          
        }
       
    }

    struct Coordinates
    {
        public double x;
        public double y;
    }

    struct Edge
    {
        public Coordinates from;
        public Coordinates to;
    }

    class GraphCoordinates
    {
        public List<Coordinates> vertices = new List<Coordinates>();
        public List<Edge> edges = new List<Edge>();
    }

    class GraphGenerator
    {
        StreamReader streamReader;
        int SizeOfGraph { get; set; }

        public GraphGenerator(int n)
        {
            SizeOfGraph = n;
            streamReader = new StreamReader("C:/Users/filip/source/repos/generating-simple-drawings-of-graphs/VizualizerWPF/data/graph" + SizeOfGraph + ".txt");
        }

        GraphCoordinates ReadUntillNextRS()
        {
            var graphCoordinates = new GraphCoordinates();
            HashSet<Coordinates> vertices = new HashSet<Coordinates>();

            string line;
            while(!String.Equals((line = streamReader.ReadLine()), "#") && line != null)
            {
                string[] temp = line.Split();

                graphCoordinates.edges.Add(
                    new Edge
                    {
                        from = new Coordinates { x = Double.Parse(temp[0]), y = Double.Parse(temp[1]) },
                        to = new Coordinates { x = Double.Parse(temp[2]), y = Double.Parse(temp[3]) }
                    });

                vertices.Add(graphCoordinates.edges[graphCoordinates.edges.Count - 1].from);
                vertices.Add(graphCoordinates.edges[graphCoordinates.edges.Count - 1].to);
            }

            graphCoordinates.vertices = vertices.ToList();
            return graphCoordinates;
        }

        public GraphCoordinates GenerateNextDrawing()
        {
            return ReadUntillNextRS();
        }
    }
}
