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
            var cosOfAngle = (av.X * ab.X + av.Y * ab.Y) / ((Distance(av.ToPoint(), origin) * Distance(ab.ToPoint(), origin)));

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

        public Edge CountWholeForceForEdge(Edge e, List<Vertex> vertices, List<Edge> edges, Dictionary<Point, double[]> Rs)
        {
            Edge newEdge = new Edge();
            newEdge.points.Add(e.points[0]);
            for (int i = 1; i < e.points.Count - 1; i++)
            {

                var v = e.points[i];

                /* count forces */

                Vector finalForce = origin.ToVector();

                finalForce += CountAttractionForce(new Point(e.lines[i - 1].X1, e.lines[i - 1].Y1), v);
                finalForce += CountAttractionForce(new Point(e.lines[i + 1].X2, e.lines[i + 1].Y2), v);

                foreach (var vertex in vertices)
                {
                    finalForce += CountRepulsionForce(v, vertex.center);
                }

                foreach (var edge in edges)
                {
                    finalForce += CountRepulsionEdgeForce(v, edge.points[0], edge.points[1]);
                }

                foreach (var vertex in vertices)
                {
                    finalForce -= CountRepulsionEdgeForce(vertex.center, v, new Point(e.lines[i - 1].X1, e.lines[i - 1].Y1));
                    finalForce -= CountRepulsionEdgeForce(vertex.center, v, new Point(e.lines[i + 1].X2, e.lines[i + 1].Y2));
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
                        newEdge.points.Add(v + finalForce);
                        break;

                    }
                }
            }
            newEdge.points.Add(e.points.Last());
            return newEdge;
        }
        
    }

}
