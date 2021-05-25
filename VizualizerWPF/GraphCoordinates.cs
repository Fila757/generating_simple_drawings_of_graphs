using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace VizualizerWPF
{
    /// <summary>
    ///     Class to store graph to know what to draw on canvas
    ///     <param name="vertices">Hashset to store vertices</param>
    ///     <param name="edge">List to store edges</param>
    ///     <param name="neigbors">Dictionary to store neighbors of all vertices</param>
    /// </summary>
    public class GraphCoordinates
    {
        public static Point farFarAway = new Point {X = 10000, Y = 10000};

        public static Point facePoint = farFarAway;

        private static readonly Brush[] colors =
        {
            Brushes.Red, Brushes.Orange, Brushes.Yellow, Brushes.LightGreen, Brushes.ForestGreen,
            Brushes.LightSkyBlue, Brushes.Blue, Brushes.DarkBlue, Brushes.Purple, Brushes.Pink
        };

        /// <summary>
        ///     Function to recalculate number of k edges
        ///     and AM, AMAM, AMAMAM k edges
        ///     It is done by finding all triangles upon all edges
        ///     and counting <c>k</c> for every edge (also invariant counting is done)
        /// </summary>
        public static int maximalKEdges = 8;

        public List<Edge> edges = new List<Edge>();
        public Dictionary<Vertex, List<Edge>> neighbors = new Dictionary<Vertex, List<Edge>>();

        public HashSet<Vertex> vertices = new HashSet<Vertex>();


        /// <summary>
        ///     Safe version of adding to dictionary
        ///     if value is not present, new list is created
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddToDictionary(Vertex key, Edge value)
        {
            if (neighbors.ContainsKey(key))
            {
                neighbors[key].Add(value);
            }
            else
            {
                neighbors[key] = new List<Edge>();
                neighbors[key].Add(value);
            }
        }

        /// <summary>
        ///     Function to save coordinates to file.
        /// </summary>
        public void SaveCoordinates()
        {
            var streamWriter = new StreamWriter(
                @"../../../data/savedGraphsBackUp.txt",
                true);

            var visited = new Dictionary<(Vertex, Vertex), bool>();
            foreach (var from in neighbors.Keys)
            foreach (var to in neighbors.Keys)
                visited[(@from, to)] = false;

            foreach (var v in neighbors.Keys)
            foreach (var e in neighbors[v])
            {
                var oppositeVertex = FindVertex(CollisionDetection.ChooseOppositeOne(e, v.center));
                if (visited[(v, oppositeVertex)])
                    continue;
                visited[(v, oppositeVertex)] = true;
                visited[(oppositeVertex, v)] = true;

                PrintLine(streamWriter, e);
            }

            streamWriter.WriteLine("#"); // end
            streamWriter.Close();
        }

        /// <summary>
        ///     Function to print given <c>edge</c>, meaning all its line segment
        /// </summary>
        /// <param name="streamWriter"></param>
        /// <param name="edge"></param>
        private void PrintLine(StreamWriter streamWriter, Edge edge)
        {
            streamWriter.Write("( ");
            for (var i = 0; i < edge.points.Count; i++)
            {
                streamWriter.Write($"{edge.points[i].X} {edge.points[i].Y} ");
                if (FindVertex(edge.points[i]).state == VertexState.Intersection)
                {
                    streamWriter.Write(") ( ");
                    streamWriter.Write($"{edge.points[i].X} {edge.points[i].Y} ");
                }
            }

            streamWriter.WriteLine(")");
        }

        /// <summary>
        ///     Check if the conjecture holds for given values of <c>AMKEdgesArray</c>
        /// </summary>
        /// <param name="AMKEdgesArray"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public bool Chech3AMKConjecture(int[,] AMKEdgesArray, int size)
        {
            for (var i = 0; i <= size; i++)
                if (edges.Count >= (2 * i + 3) * (2 * i + 2) / 2)
                    if (AMKEdgesArray[2, i] < 3 * ((i + 4) * (i + 3) * (i + 2) * (i + 1) / 24))
                        return false;
            return true;
        }


        /// <summary>
        ///     Check if the conjecture holds for given values of <c>AMKEdgesArray</c>
        /// </summary>
        /// <param name="AMKEdgesArray"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public bool Chech2AMKConjecture(int[,] AMKEdgesArray, int size)
        {
            for (var i = 0; i <= size; i++)
                if (edges.Count >= (2 * i + 3) * (2 * i + 2) / 2)
                    if (AMKEdgesArray[1, i] < 3 * ((i + 3) * (i + 2) * (i + 1) / 6))
                        return false;
            return true;
        }


        public void DeleteEdgeFromDictionary(Vertex from, Edge to)
        {
            neighbors[from].Remove(to);
        }


        /// <summary>
        ///     Get all lines incident to <c>vertex</c> so that first end is <c>vertex</c>
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public IEnumerable<Line> GetLines(Vertex vertex)
        {
            if (vertex.state == VertexState.Regular)
            {
                foreach (var edge in neighbors[vertex])
                    yield return
                        CollisionDetection.ChooseTheLineBy(vertex,
                            edge); //choose the line by the vertex v (of this edge)
                yield break;
            }

            if (vertex.state == VertexState.Intersection)
            {
                foreach (var line in CollisionDetection.GetEdges(vertex, this))
                    yield return
                        CollisionDetection.OrientLineProperly(vertex,
                            line); //orient the edge if the intersection is on the opposite side of the line
                yield break;
            }

            throw new ArgumentException("Vertex must be regular or intersection.");
        }

        /// <summary>
        ///     Get all vertices and intersection contained in <c>graphCoordinates</c>
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Vertex> GetVerticesAndIntersections()
        {
            foreach (var vertex in vertices)
                if (vertex.state == VertexState.Regular || vertex.state == VertexState.Intersection)
                    yield return vertex;
        }

        public (int[], int[]) ReCalculateKEdges(List<Vertex> withouts = null, Edge withoutEdge = null)
        {
            //int kEdgesPicked = 0;//(int)KhranyUpDown.Value;

            var kEdgesValues = Enumerable.Repeat(0, maximalKEdges + 1).ToArray();
            var invariantKEdges = Enumerable.Repeat(0, maximalKEdges + 1).ToArray();

            var visited = new Dictionary<(Vertex, Vertex), bool>();

            foreach (var from in neighbors.Keys)
            foreach (var to in neighbors.Keys)
                visited[(@from, to)] = false;

            if (withouts != null)
                foreach (var vertex in withouts)
                foreach (var v in neighbors.Keys)
                {
                    visited[(vertex, v)] = true;
                    visited[(v, vertex)] = true;
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

            foreach (var (from, value) in neighbors)
            foreach (var e1 in value)
            {
                var to = FindVertex(CollisionDetection.ChooseOppositeOne(e1, @from.center));

                if (visited[(@from, to)]) continue;
                visited[(@from, to)] = true;
                visited[(to, @from)] = true;

                var sumRight = 0;
                var sumLeft = 0;
                foreach (var e2 in neighbors[to])
                {
                    var third = FindVertex(CollisionDetection.ChooseOppositeOne(e2, to.center));
                    var e3 = neighbors[@from].ContainsEnd(@from.center, third.center);
                    if (e3 != null)
                    {
                        if (third == @from || third == to
                                           ||
                                           withouts != null && withouts.Contains(third)
                                           ||
                                           firstEdgeVertex.HasValue
                                           &&
                                           secondEdgeVertex.HasValue
                                           &&
                                           CollisionDetection.CheckIfEdgeIsInTriangle(@from, to, third,
                                               firstEdgeVertex.Value, secondEdgeVertex.Value))
                            continue;

                        var allLines = CollisionDetection.PutLinesTogether(e1, e2, e3);

                        if (CollisionDetection.GetOrientation(allLines.Item1, allLines.Item2, facePoint) > 0)
                            sumRight++;
                        else
                            sumLeft++;
                    }
                }

                var sum = sumRight < sumLeft ? sumRight : sumLeft;


                kEdgesValues[sum]++;

                var edge = FindEdgeFromVertices(@from, to);

                //if (edge == null) 
                //    continue;

                var invariant = Difference.One;

                if (withouts != null || withoutEdge != null)
                {
                    if (edge.kEdge == sum)
                    {
                        invariant = Difference.Zero;
                        invariantKEdges[sum]++;
                    }
                    else if (edge.kEdge - sum == 2)
                    {
                        invariant = Difference.Two;
                    }
                }
                else
                {
                    edge.kEdge = sum;
                }

                foreach (var line in edge.lines)
                    if (withouts != null || withoutEdge != null)
                    {
                        if (invariant == Difference.Zero)
                            line.StrokeDashArray = DoubleCollection.Parse("4 1 1 1 1 1");
                        else if (invariant == Difference.Two)
                            line.StrokeDashArray = DoubleCollection.Parse("1 1");
                        else
                            line.StrokeDashArray = DoubleCollection.Parse("");
                    }

                    else
                    {
                        line.Stroke = colors[sum];
                    }
            }

            return (kEdgesValues, invariantKEdges);
        }

        /// <summary>
        ///     Function to find edge between two vertices
        /// </summary>
        /// <param name="a">First vertex</param>
        /// <param name="b">Second vertex</param>
        /// <returns>Found edge</returns>
        public Edge FindEdgeFromVertices(Vertex a, Vertex b)
        {
            foreach (var edge in edges)
                if ((CollisionDetection.CenterOfVertexOnLine(edge.lines[0], a) ||
                     CollisionDetection.CenterOfVertexOnLine(edge.lines.Last(), a))
                    &&
                    (CollisionDetection.CenterOfVertexOnLine(edge.lines[0], b) ||
                     CollisionDetection.CenterOfVertexOnLine(edge.lines.Last(), b)))
                    return edge;

            return null; // so such edge
        }

        /// <summary>
        ///     Function to find vertex with center equal to <c>center</c>.
        /// </summary>
        /// <param name="center"></param>
        /// <returns></returns>
        public Vertex FindVertex(Point center)
        {
            foreach (var vertex in vertices)
                if (Vertex.Compare(vertex.center, center))
                    return vertex;

            //return new Vertex();
            throw new ArgumentException("There is no such a vertex with that center");
        }

        /// <summary>
        ///     Function to find vertex containg <c>ellipse</c>
        /// </summary>
        /// <param name="ellipse"></param>
        /// <returns></returns>
        public Vertex FindVertex(Ellipse ellipse, double sizeOfVertex)
        {
            return FindVertex(new Point(ellipse.Margin.Left + sizeOfVertex / 2, ellipse.Margin.Top + sizeOfVertex / 2));

            throw new ArgumentException("There is no such a vertex, containing ellipse");
        }


        /// <summary>
        ///     Function to get all points from vertices
        /// </summary>
        /// <param name="graphCoordinates"></param>
        /// <returns></returns>
        public HashSet<Point> GetAllPoints()
        {
            var result = new HashSet<Point>();
            foreach (var vertex in vertices)
                result.Add(vertex.center);

            return result;
        }

        public IEnumerable<Line> LinesIterator()
        {
            foreach (var edge in edges)
            foreach (var line in edge.lines)
                yield return line;
        }

        public IEnumerable<Point> PointsIterator()
        {
            foreach (var edge in edges)
            foreach (var point in edge.points)
                yield return point;
        }


        public List<object> PrintingLines()
        {
            var lines = new List<object>();
            foreach (var edge in edges)
            foreach (var line in edge.lines)
                lines.Add(new {line.X1, line.Y1, line.X2, line.Y2});
            return lines;
        }

        public List<object> PrintingLines(List<Line> listLines)
        {
            var lines = new List<object>();
            foreach (var line in listLines) lines.Add(new {line.X1, line.Y1, line.X2, line.Y2});
            return lines;
        }

        public List<object> PrintingVertices()
        {
            var newVertices = new List<object>();
            foreach (var vertex in vertices) newVertices.Add(new {vertex.center.X, vertex.center.Y});
            return newVertices;
        }

        private enum Difference
        {
            Zero,
            One,
            Two
        }
    }
}