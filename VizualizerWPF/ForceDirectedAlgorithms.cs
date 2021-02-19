﻿/// <summary>
/// In this project Syncfusion.WPF nuget is required because Syncfusion UpDown is used
/// Also english version of file system is assumed because decimal dots are used (25.4)
/// </summary>
/// 
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;



namespace VizualizerWPF
{

    static class ForceDirectedAlgorithms
    {
        static double gamma = 1;
        static double delta = 1000;

        static int INF = 1000;

        static Point origin = new Point(0, 0);

        static MainWindow mainWindow;

        public static void Init(MainWindow mainWindow2)
        {
            mainWindow = mainWindow2;
        }

        static Vector[] RegionVectors = new Vector[] {
            new Vector(10, 0),
            new Vector(10, 10),
            new Vector(0, 10),
            new Vector(-10, 10),
            new Vector(-10, 0),
            new Vector(-10, -10),
            new Vector(0, -10),
            new Vector(10, -10)
         };

        static Vertex FindVertex(GraphCoordinates graphCoordinates, Point center)
        {
            foreach (var vertex in graphCoordinates.vertices)
            {
                if (vertex.center == center)
                    return vertex;
            }

            throw new ArgumentException("There is no such a vertex with that center");
        }

        static HashSet<Point> FindNeighbors(GraphCoordinates graphCoordinates, Vertex vertex)
        {
            HashSet<Point> neighbors = new HashSet<Point>();


            /* creating lines from graphCoordinates */

            List<Line> lines = new List<Line>();
            foreach (var edge in graphCoordinates.edges) {
                foreach(var l in edge.lines)
                {
                    lines.Add(l);
                }
            }

            foreach (var line in lines)
            {
                if(CollisionDetection.CenterOnLine(line, vertex.center))
                {
                    //maybe compare absolute difference is < 0.5
                    neighbors.Add(new Point(line.X1, line.Y1) == vertex.center ? new Point(line.X2, line.Y2) : new Point(line.X1, line.Y1));
                }

            }
            return neighbors;
        }

        static public Vector Min(Vector vector1, Vector vector2)
        {
            return Distance(vector1.ToPoint(), origin) < Distance(vector2.ToPoint(), origin) ? vector1 : vector2;
        }

        static double Determinant(Vector a, Vector b)
        {
            return a.X * b.Y - b.X * a.Y;
        }

        static bool IsAcute(Vector first, Vector second)
        {
            return (first.X * second.X + first.Y * second.Y) > 0;
        }

        static bool IsBetween(Vector first, Vector second, Vector middle)
        {
            return (Determinant(first, middle) * Determinant(second, middle) <= 0) && (IsAcute(first, middle) && IsAcute(second, middle));
        }

        static double Distance(Point a, Point b)
        {
            return Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
        }

        static public Vector CountAttractionForce(Point a, Point b)
        {
            return (Distance(a, b) / delta) * (b - a);
        }

        static public Vector CountRepulsionForce(Point a, Point b)
        {
            return ((-delta * delta) / (Distance(a, b) * Distance(a, b))) * (b - a);
        }

        static Point Projection(Point v, Point a, Point b)
        {
            Vector av = v - a;
            Vector ab = b - a;
            var cosOfAngle = (av.X * ab.X + av.Y * ab.Y) / (Distance(av.ToPoint(), origin) * Distance(ab.ToPoint(), origin));

            ab.Normalize();
            return Distance((cosOfAngle * av).ToPoint(), origin) * ab + a;
        }

        static public Vector CountRepulsionEdgeForce(Point v, Point a, Point b)
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

        static public Point CountForceInnerVertex(Point v, HashSet<Point> neighbors, HashSet<Point> vertices, List<Edge> edges, Dictionary<Point, double[]> Rs)
        {
            if(v.X == 393 && v.Y == 74)
            {
                Console.WriteLine("rr");
            }

            Vector finalForce = origin.ToVector();

            foreach (var vertex in neighbors)
            {
                 finalForce += CountAttractionForce(vertex, v);
            }
            foreach (var vertex in vertices)
            {
                if(vertex != v)
                    finalForce += CountRepulsionForce(v, vertex);
            }

            foreach (var edge in edges)
            {
                finalForce += CountRepulsionEdgeForce(v, edge.points[0], edge.points[1]);
                if (Double.IsNaN(finalForce.X))
                {
                    Console.WriteLine("NaN");
                    CountRepulsionEdgeForce(v, edge.points[0], edge.points[1]);
                }
            }

            foreach (var vertex in vertices)
            {
                foreach (var neigh in neighbors)
                {
                    finalForce += CountRepulsionEdgeForce(vertex, v, neigh);
                    if (Double.IsNaN(finalForce.X))
                    {
                        Console.WriteLine("NaN");
                        CountRepulsionEdgeForce(vertex, v, neigh);
                    }
                }
            }

            /* special case point on line, should be resolve */

            if (Double.IsNaN(finalForce.X) && Double.IsNaN(finalForce.Y))
                finalForce = new Vector(INF, INF);

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

            throw new ArgumentException("It is not in any region");
        }

        static public GraphCoordinates CountAndMoveByForces(GraphCoordinates graphCoordinates)
        {
            mainWindow.mainCanvas.Children.Clear();

            var newGraphCoordinates = new GraphCoordinates();
            List<Edge> edges = new List<Edge>();
            HashSet<Point> vertices = new HashSet<Point>();

            Dictionary<Point, Point> convertor = new Dictionary<Point, Point>();

            foreach(var edge in graphCoordinates.edges)
            {
                foreach(var line in edge.lines)
                {
                    edges.Add(new Edge(new List<Point> { new Point(line.X1, line.Y1), new Point(line.X2, line.Y2)  }, new List<Line> { line }));
                }
            }
            foreach(var vertex in graphCoordinates.vertices)
            {
                vertices.Add(vertex.center);
            }

            Dictionary<Point, double[]> Rs = new Dictionary<Point, double[]>();
            CountRadiuses(vertices, edges, Rs);

            /*
            foreach(var (point, arr) in Rs)
            {
                //Rs[point] = new double[] { 1, 1, 1, 1, 1, 1, 1, 1 };

                string debugger = point.X + " " + point.Y;
                for (int i = 0; i < 8; i++)
                    debugger += " " + arr[i];


                MessageBox.Show(debugger);
            }
            */

            foreach(var vertex in graphCoordinates.vertices)
            {
                var neighbors = FindNeighbors(graphCoordinates, vertex);
                convertor[vertex.center] = CountForceInnerVertex(vertex.center, neighbors, vertices, edges, Rs);
            }

            foreach(var vertex in graphCoordinates.vertices)
            {
                newGraphCoordinates.vertices.Add(
                    new Vertex
                    {
                        center = convertor[vertex.center],
                        ellipse = new Ellipse(),
                        state = vertex.state
                    }) ;
            }


            /* create lines */

            foreach(var edge in graphCoordinates.edges)
            {
                var lines = new List<Line>();
                var points = new List<Point>() { convertor[edge.points[0]] };
                for(int i = 1; i < edge.points.Count; i++)
                {
                    var line = new Line
                    {
                        X1 = convertor[edge.points[i - 1]].X,
                        Y1 = convertor[edge.points[i - 1]].Y,
                        X2 = convertor[edge.points[i]].X,
                        Y2 = convertor[edge.points[i]].Y,
                    };

                    points.Add(convertor[edge.points[i]]);
                    lines.Add(line);
                }

                var newEdge = new Edge(points, lines);
                newGraphCoordinates.edges.Add(newEdge);

                Vertex actualZero = FindVertex(newGraphCoordinates, convertor[edge.points[0]]);
                Vertex actualLast = FindVertex(newGraphCoordinates, convertor[edge.points.Last()]);
                
                /*
                if (newGraphCoordinates.vertices.Contains(actualZero))
                    newGraphCoordinates.vertices.TryGetValue(actualZero, out actualZero);
                if (newGraphCoordinates.vertices.Contains(actualLast))
                    newGraphCoordinates.vertices.TryGetValue(actualLast, out actualLast);
                */

                newGraphCoordinates.AddToDictionary(actualZero, newEdge);
                newGraphCoordinates.AddToDictionary(actualLast, newEdge);
            }

            return newGraphCoordinates;
        }

        static void CountRadiuses(HashSet<Point> vertices, List<Edge> edges, Dictionary<Point, double[]> Rs)
        {
            foreach(var vertex in vertices)
            {
                Rs[vertex] = new double[8] { INF, INF, INF, INF, INF, INF, INF, INF };
            }

            foreach(Point v in vertices)
            {
                
                foreach(var edge in edges)
                {
                    Point a = edge.points[0];
                    Point b = edge.points[1];

                    if(a == v || b == v) //is needed outherwise zero
                        continue;

                    Point i_v = Projection(v, a, b);
                    Vector difference = i_v - a;
                    Vector vector = b - a;
                    var coef = difference.Divide(vector);
                    if (1 - 0.01 > coef && coef > 0.01)
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

    }

}
