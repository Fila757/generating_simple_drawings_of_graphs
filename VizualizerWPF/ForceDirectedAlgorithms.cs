/// <summary>
/// In this project Syncfusion.WPF nuget is required because Syncfusion UpDown is used
/// Also english version of file system is assumed because decimal dots are used (25.4)
/// </summary>
/// 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Shapes;

namespace VizualizerWPF
{
    /// <summary>
    ///     Class to apply (iteratively) force-directed algorithm.
    /// </summary>
    internal static class ForceDirectedAlgorithms
    {
        private static readonly double gamma = 3;

        /// <summary>
        ///     The desider length of all edges
        /// </summary>
        private static readonly double delta = 20;

        /// <summary>
        ///     Maximal force we want to apply
        /// </summary>
        private static readonly int INF = 1000;

        private static readonly Point origin = new Point(0, 0);

        private static MainWindow mainWindow;

        /// <summary>
        ///     Vectors of borders of zones
        /// </summary>
        private static readonly Vector[] RegionVectors =
        {
            new Vector(10, 0),
            new Vector(10, 10),
            new Vector(0, 10),
            new Vector(-10, 10),
            new Vector(-10, 0),
            new Vector(-10, -10),
            new Vector(0, -10),
            new Vector(10, -10)
        };

        public static void Init(MainWindow mainWindow2)
        {
            mainWindow = mainWindow2;
        }


        private static Vertex FindVertex(GraphCoordinates graphCoordinates, Point center)
        {
            foreach (var vertex in graphCoordinates.vertices)
                if (vertex.center == center)
                    return vertex;

            throw new ArgumentException("There is no such a vertex with that center");
        }

        /// <summary>
        ///     Get all opposite point to <c>vertex</c> in <c>graphCoordinates</c>
        /// </summary>
        /// <param name="graphCoordinates"></param>
        /// <param name="vertex"></param>
        /// <returns></returns>
        private static HashSet<Point> FindNeighbors(GraphCoordinates graphCoordinates, Vertex vertex)
        {
            var neighbors = new HashSet<Point>();


            /* creating lines from graphCoordinates */

            var lines = new List<Line>();
            foreach (var edge in graphCoordinates.edges)
            foreach (var l in edge.lines)
                lines.Add(l);

            foreach (var line in lines)
                if (CollisionDetection.CenterOnLine(line, vertex.center))
                    //maybe compare absolute difference is < 0.5
                    neighbors.Add(new Point(line.X1, line.Y1) == vertex.center
                        ? new Point(line.X2, line.Y2)
                        : new Point(line.X1, line.Y1));
            return neighbors;
        }

        /// <summary>
        ///     Function to get smaller vector.
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <returns></returns>
        public static Vector Min(Vector vector1, Vector vector2)
        {
            return Distance(vector1.ToPoint(), origin) < Distance(vector2.ToPoint(), origin) ? vector1 : vector2;
        }

        private static double Determinant(Vector a, Vector b)
        {
            return a.X * b.Y - b.X * a.Y;
        }

        private static bool IsAcute(Vector first, Vector second)
        {
            return first.X * second.X + first.Y * second.Y > 0;
        }

        /// <summary>
        ///     Function to detect if the <c>midlle</c> vector is between (the smaller part) determined by <c>first</c> and
        ///     <c>second</c>.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="middle"></param>
        /// <returns></returns>
        private static bool IsBetween(Vector first, Vector second, Vector middle)
        {
            return Determinant(first, middle) * Determinant(second, middle) <= 0 && IsAcute(first, middle) &&
                   IsAcute(second, middle);
        }

        private static double Distance(Point a, Point b)
        {
            return Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
        }

        /// <summary>
        ///     Get attraction force of <c>b</c> when <c>a</c> is fixed.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vector CountAttractionForce(Point a, Point b)
        {
            return Distance(a, b) / delta * (a - b);
        }

        /// <summary>
        ///     Get repulsion force of <c>b</c> when <c>a</c> is fixed.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vector CountRepulsionForce(Point a, Point b)
        {
            return -delta * delta / (Distance(a, b) * Distance(a, b)) * (b - a);
        }

        /// <summary>
        ///     Get projection of <c>v</c> on the line determined by <c>a</c> and <c>b</c>.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private static Point Projection(Point v, Point a, Point b)
        {
            var av = v - a;
            var ab = b - a;
            var cosOfAngle = (av.X * ab.X + av.Y * ab.Y) /
                             (Distance(av.ToPoint(), origin) * Distance(ab.ToPoint(), origin));

            ab.Normalize();
            return Distance((cosOfAngle * av).ToPoint(), origin) * ab + a;
        }

        /// <summary>
        ///     Get repulsion force of <v>c</v> with edge determined by <c>a</c> and <c>b</c> fixed.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vector CountRepulsionEdgeForce(Point v, Point a, Point b)
        {
            var i_v = Projection(v, a, b);
            var difference = i_v - a;
            var vector = b - a;
            var coef = difference.Divide(vector);
            if (1 - 0.0001 > coef && coef > 0.0001 && Distance(v, i_v) < gamma)
                return -((gamma - Distance(v, i_v) * (gamma - Distance(v, i_v))) / Distance(v, i_v)) * (i_v - v);
            return new Vector(0, 0);
        }

        /// <summary>
        ///     Function to count and add forces to vertex <c>v</c>.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="neighbors"></param>
        /// <param name="vertices"></param>
        /// <param name="edges"></param>
        /// <param name="Rs"></param>
        /// <returns></returns>
        public static Point CountForceInnerVertex(Point v, HashSet<Point> neighbors, HashSet<Point> vertices,
            List<Edge> edges, Dictionary<Point, double[]> Rs)
        {
            var finalForce = origin.ToVector();

            foreach (var vertex in neighbors) finalForce += CountAttractionForce(vertex, v);
            foreach (var vertex in vertices)
                if (vertex != v)
                    finalForce += CountRepulsionForce(v, vertex);

            foreach (var edge in edges) finalForce += CountRepulsionEdgeForce(v, edge.points[0], edge.points[1]);

            foreach (var vertex in vertices)
            foreach (var neigh in neighbors)
                finalForce -= CountRepulsionEdgeForce(vertex, v, neigh);

            /* special case point on line, should be resolve */

            if (double.IsNaN(finalForce.X) && double.IsNaN(finalForce.Y))
                finalForce = new Vector(INF, INF);

            /* chechk regioun condition */

            for (var j = 0; j < 8; j++)
            {
                var j2 = (j + 1) % 8;
                if (IsBetween(RegionVectors[j], RegionVectors[j2], finalForce))
                {
                    var normalized = new Vector(finalForce.X, finalForce.Y);
                    normalized.Normalize();
                    normalized = normalized.Scale(Rs[v][j]);

                    finalForce = Min(finalForce, normalized);
                    return v + finalForce;
                }
            }

            throw new ArgumentException("It is not in any region");
        }

        /// <summary>
        ///     Apply force-directed algorithm to all vertices on canvas.
        /// </summary>
        /// <param name="graphCoordinates"></param>
        /// <returns></returns>
        public static GraphCoordinates CountAndMoveByForces(GraphCoordinates graphCoordinates)
        {
            mainWindow.mainCanvas.Children.Clear();

            var newGraphCoordinates = new GraphCoordinates();
            var edges = new List<Edge>();
            var vertices = new HashSet<Point>();

            var convertor = new Dictionary<Point, Point>();

            foreach (var edge in graphCoordinates.edges)
            foreach (var line in edge.lines)
                edges.Add(new Edge(new List<Point> {new Point(line.X1, line.Y1), new Point(line.X2, line.Y2)},
                    new List<Line> {line}));
            foreach (var vertex in graphCoordinates.vertices) vertices.Add(vertex.center);

            var Rs = new Dictionary<Point, double[]>();
            CountRadii(vertices, edges, Rs);

            foreach (var vertex in graphCoordinates.vertices)
            {
                var neighbors = FindNeighbors(graphCoordinates, vertex);
                convertor[vertex.center] = CountForceInnerVertex(vertex.center, neighbors, vertices, edges, Rs);
            }

            foreach (var vertex in graphCoordinates.vertices)
                newGraphCoordinates.vertices.Add(
                    new Vertex
                    {
                        center = convertor[vertex.center],
                        ellipse = new Ellipse(),
                        state = vertex.state
                    });


            /* create lines */

            foreach (var edge in graphCoordinates.edges)
            {
                var lines = new List<Line>();
                var points = new List<Point> {convertor[edge.points[0]]};
                for (var i = 1; i < edge.points.Count; i++)
                {
                    var line = new Line
                    {
                        X1 = convertor[edge.points[i - 1]].X,
                        Y1 = convertor[edge.points[i - 1]].Y,
                        X2 = convertor[edge.points[i]].X,
                        Y2 = convertor[edge.points[i]].Y
                    };

                    points.Add(convertor[edge.points[i]]);
                    lines.Add(line);
                }

                var newEdge = new Edge(points, lines);
                newGraphCoordinates.edges.Add(newEdge);

                var actualZero = FindVertex(newGraphCoordinates, convertor[edge.points[0]]);
                var actualLast = FindVertex(newGraphCoordinates, convertor[edge.points.Last()]);

                newGraphCoordinates.AddToDictionary(actualZero, newEdge);
                newGraphCoordinates.AddToDictionary(actualLast, newEdge);
            }

            return newGraphCoordinates;
        }

        /// <summary>
        ///     Function to count the radii of zones to preserve the crossing properties.
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="edges"></param>
        /// <param name="Rs"></param>
        private static void CountRadii(HashSet<Point> vertices, List<Edge> edges, Dictionary<Point, double[]> Rs)
        {
            foreach (var vertex in vertices) Rs[vertex] = new double[8] {INF, INF, INF, INF, INF, INF, INF, INF};

            foreach (var v in vertices)
            foreach (var edge in edges)
            {
                var a = edge.points[0];
                var b = edge.points[1];

                if (a == v || b == v) //is needed outherwise zero
                    continue;

                var i_v = Projection(v, a, b);
                var difference = i_v - a;
                var vector = b - a;
                var coef = difference.Divide(vector);
                if (1 - 0.0001 > coef && coef > 0.0001)
                {
                    var s = INF;
                    for (var i = 0; i < 8; i++)
                    {
                        var i2 = (i + 1) % 8;
                        if (IsBetween(RegionVectors[i], RegionVectors[i2], i_v - v)) s = i;
                    }

                    for (var i = -2; i <= 2; i++)
                        Rs[v][(s + i + 8) % 8] = Math.Min(Rs[v][(s + i + 8) % 8], Distance(v, i_v) / 3);
                    for (var i = 2; i <= 6; i++)
                    {
                        Rs[a][(s + i + 8) % 8] = Math.Min(Rs[a][(s + i + 8) % 8], Distance(v, i_v) / 3);
                        Rs[b][(s + i + 8) % 8] = Math.Min(Rs[b][(s + i + 8) % 8], Distance(v, i_v) / 3);
                    }
                }
                else
                {
                    for (var i = 0; i < 8; i++)
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