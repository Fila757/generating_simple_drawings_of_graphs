/// <summary>
/// In this project Syncfusion.WPF nuget is required because Syncfusion UpDown is used
/// Also english version of file system is assumed because decimal dots are used (25.4)
/// </summary>
/// 
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;



namespace VizualizerWPF
{

    class ForceDirectedAlgorithms
    {
        int gamma = 10;
        int delta = 10;

        int INF = 1000000;

        Point origin = new Point(0, 0);

        Vector[] RegionVectors = new Vector[] {
            new Vector(10, 0),
            new Vector(10, 10),
            new Vector(0, 10),
            new Vector(-10, 10),
            new Vector(-10, 0),
            new Vector(-10, -10),
            new Vector(0, -10),
         };

        Vertex FindVertex(GraphCoordinates graphCoordinates, Point center)
        {
            foreach (var vertex in graphCoordinates.vertices)
            {
                if (vertex.center == center)
                    return vertex;
            }

            throw new ArgumentException("There is no such a vertex with that center");
        }

        public Vector Min(Vector vector1, Vector vector2)
        {
            return Distance(vector1.ToPoint(), origin) < Distance(vector2.ToPoint(), origin) ? vector1 : vector2;
        }

        double Determinant(Vector a, Vector b)
        {
            return a.X * b.Y - b.X * a.Y;
        }

        bool IsBetween(Vector first, Vector second, Vector middle)
        {
            return Determinant(first, middle) * Determinant(second, middle) <= 0;
        }

        double Distance(Point a, Point b)
        {
            return Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
        }

        public Vector CountAttractionForce(Point a, Point b)
        {
            return (Distance(a, b) / delta) * (b - a);
        }

        public Vector CountRepulsionForce(Point a, Point b)
        {
            return ((-delta * delta) / (Distance(a, b) * Distance(a, b))) * (b - a);
        }

        Point Projection(Point v, Point a, Point b)
        {
            Vector av = v - a;
            Vector ab = b - a;
            var cosOfAngle = (av.X * ab.X + av.Y * ab.Y) / (Distance(av.ToPoint(), origin) * Distance(ab.ToPoint(), origin));

            return (cosOfAngle * av).ToPoint();
        }

        public Vector CountRepulsionEdgeForce(Point v, Point a, Point b)
        {
            Point i_v = Projection(v, a, b);
            Vector difference = i_v - a;
            Vector vector = b - a;
            var coef = difference.Divide(vector);
            if ((1 > coef && coef > 0) && Distance(v, i_v) < gamma)
                return -((gamma - Distance(v, i_v) * (gamma - Distance(v, i_v))) / (Distance(v, i_v))) * (i_v - v);
            else
                return new Vector(0, 0);
        }

        public Point CountForceInnerVertex(Point v, List<Vertex> neighbors, List<Point> vertices, List<Edge> edges, Dictionary<Point, double[]> Rs)
        {
            Vector finalForce = origin.ToVector();

            foreach (var vertex in neighbors)
            {
                finalForce += CountAttractionForce(vertex.center, v);
            }
            foreach (var vertex in vertices)
            {
                finalForce += CountRepulsionForce(v, vertex);
            }

            foreach (var edge in edges)
            {
                finalForce += CountRepulsionEdgeForce(v, edge.points[0], edge.points[1]);
            }

            foreach (var vertex in vertices)
            {
                foreach (var neigh in neighbors)
                {
                    finalForce -= CountRepulsionEdgeForce(vertex, v, neigh.center);
                }
            }

            /* chechk regioun condition */

            for (int j = 0; j < 8; j++)
            {
                int j2 = (j + 1) % 8;
                if (IsBetween(RegionVectors[j], RegionVectors[j2], finalForce))
                {

                    Vector normalized = new Vector(finalForce.X, finalForce.Y);
                    normalized.Normalize();
                    normalized = normalized.Scale(Rs[v][j]);

                    finalForce = Min(finalForce, normalized);
                    return v + finalForce; 
                }
            }
            return origin;
        }

        public GraphCoordinates CountAndMoveByForces(GraphCoordinates graphCoordinates)
        {
            var newGraphCoordinates = new GraphCoordinates();
            List<Edge> edges = new List<Edge>();
            List<Point> vertices = new List<Point>();
            foreach(var edge in graphCoordinates.edges)
            {
                foreach(var line in edge.lines)
                {
                    edges.Add(new Edge(new List<Point> { new Point(line.X1, line.Y1), new Point(line.X2, line.Y2)  }, new List<Line> { line }));
                }

                for(int i = 1; i < edge.points.Count - 1; i++)
                {
                    vertices.Add(edge.points[i]);
                }
            }
            foreach(var vertex in graphCoordinates.vertices)
            {
                vertices.Add(vertex.center);
            }

            Dictionary<Point, double[]> Rs = new Dictionary<Point, double[]>();
            CountRadiuses(vertices, edges, Rs);

            foreach (var edge in graphCoordinates.edges)
            {
                newGraphCoordinates.edges.Add(CountWholeForceForEdge(edge, vertices, edges, Rs));
            }

            foreach (var edge in newGraphCoordinates.edges)
            {
                var vertex1 = FindVertex(graphCoordinates, edge.points[0]);
                ellipse = new Ellipse
                {
                    Width = MainWindow.sizeOfVertex,
                    Height = MainWindowsizeOfVertex,
                    Fill = vertex1.state == VertexState.Regular ? Brushes.Blue : Brushes.Green,
                    Margin = new Thickness(vertexTemp.center.X - MainWindowsizeOfVertex / 2, vertexTemp.center.Y - sizeOfVertex / 2, 0, 0),
                    Visibility = ((vertexTemp.state == VertexState.Intersection && !statesCalculation[StateCalculation.Intersections]) ||
                    (vertexTemp.state == VertexState.Middle)) ? Visibility.Hidden : Visibility.Visible

                newGraphCoordinates.vertices.Add(
                    new Vertex { 
                        center = CountForceInnerVertex(vertex1.center, graphCoordinates.neighbors[vertex1], vertices, edges, Rs),
                        state = vertex1.state
                        
                        };

                ellipse.MouseDown += ellipse_MouseDown;
                mainCanvas.Children.Add(ellipse);

                Panel.SetZIndex(ellipse, vertexTemp.state == VertexState.Regular ? 100 : 10);

                vertexTemp.ellipse = ellipse;
            });

                var vertex2 = FindVertex(graphCoordinates, edge.points.Last());
                newGraphCoordinates.vertices.Add(
                    new Vertex
                    {
                        center = CountForceInnerVertex(vertex2.center, graphCoordinates.neighbors[vertex2], vertices, edges, Rs),
                        state = vertex2.state
                    });

                newGraphCoordinates.vertices.Add(vertex1);
                newGraphCoordinates.vertices.Add(vertex2);

                newGraphCoordinates.AddToDictionary(vertex1, vertex2);
                newGraphCoordinates.AddToDictionary(vertex2, vertex1);
            }

            return newGraphCoordinates;
        }

        void CountRadiuses(List<Point> vertices, List<Edge> edges, Dictionary<Point, double[]> Rs)
        {
            foreach(var vertex in Rs.Keys)
            {
                Rs[vertex] = new double[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
            }

            foreach(Point v in vertices)
            {
                
                foreach(var edge in edges)
                {
                    Point a = edge.points[0];
                    Point b = edge.points[1];

                    Point i_v = Projection(v, a, b);
                    Vector difference = i_v - a;
                    Vector vector = b - a;
                    var coef = difference.Divide(vector);
                    if (1 > coef && coef > 0)
                    {
                        int s = INF;
                        for (int i = 0; i < 8; i++)
                        {
                            int i2 = (i + 1) % 8;
                            if (IsBetween(RegionVectors[i], RegionVectors[i2], i_v - v))
                            {
                                s = i;
                            }
                        }
                        for (int i = -2; i <= 2; i++)
                        {
                            Rs[v][(s + i + 8) % 8] = Math.Min(Rs[v][(s + i + 8) % 8], Distance(v, i_v) / 3);
                        }
                        for (int i = 2; i <= 6; i++)
                        {
                            Rs[a][(s + i + 8) % 8] = Math.Min(Rs[a][(s + i + 8) % 8], Distance(v, i_v) / 3);
                            Rs[b][(s + i + 8) % 8] = Math.Min(Rs[b][(s + i + 8) % 8], Distance(v, i_v) / 3);
                        }
                    }
                    else
                    {
                        for(int i = 0; i < 8; i++)
                        {
                            Rs[v][(i + 8) % 8] = Math.Min(Rs[v][(i + 8) % 8], Math.Min(Distance(a, v), Distance(b, v)) / 3);

                            Rs[a][(i + 8) % 8] = Math.Min(Rs[a][(i + 8) % 8], Distance(a, v) / 3);
                            Rs[b][(i + 8) % 8] = Math.Min(Rs[b][(i + 8) % 8], Distance(b, v) / 3);
                        }
                    }

                }
            }
        }

        public Edge CountWholeForceForEdge(Edge e, List<Point> vertices, List<Edge> edges, Dictionary<Point, double[]> Rs)
        {
            Edge newEdge = new Edge(e.points, e.lines);
            //newEdge.points.Add(e.points[0]);
            for (int i = 1; i < e.points.Count - 1; i++)
            {
                var v = e.points[i];

                /* count forces */

                Vector finalForce = origin.ToVector();

                finalForce += CountAttractionForce(new Point(e.lines[i - 1].X1, e.lines[i - 1].Y1), v);
                finalForce += CountAttractionForce(new Point(e.lines[i + 1].X2, e.lines[i + 1].Y2), v);

                foreach (var vertex in vertices)
                {
                    finalForce += CountRepulsionForce(v, vertex);
                }

                foreach (var edge in edges)
                {
                    finalForce += CountRepulsionEdgeForce(v, edge.points[0], edge.points[1]);
                }

                foreach (var vertex in vertices)
                {
                    finalForce -= CountRepulsionEdgeForce(vertex, v, new Point(e.lines[i - 1].X1, e.lines[i - 1].Y1));
                    finalForce -= CountRepulsionEdgeForce(vertex, v, new Point(e.lines[i + 1].X2, e.lines[i + 1].Y2));
                }

                /* chechk regioun condition */

                for (int j = 0; j < 8; j++)
                {
                    int j2 = (j + 1) % 8;
                    if (IsBetween(RegionVectors[j], RegionVectors[j2], finalForce))
                    {

                        Vector normalized = new Vector(finalForce.X, finalForce.Y);
                        normalized.Normalize();
                        normalized = normalized.Scale(Rs[v][j]);

                        finalForce = Min(finalForce, normalized);
                        newEdge.points[i] = v + finalForce;
                        break;

                    }
                }
            }
            //newEdge.points.Add(e.points.Last());
            return newEdge;
        }
        
    }

}
