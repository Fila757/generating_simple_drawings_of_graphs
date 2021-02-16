using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Shapes;

namespace VizualizerWPF
{
    /// <summary>
    /// Class to store edges in graph
    /// meaming all lines and points
    /// </summary>
    public class Edge //: IEquatable<Edge>
    {
        public List<Point> points;
        public List<Line> lines;
        //public int direction = 1;

        public static double smallestCosOfAngle = 0.5; //cca 20 \degree we can take only smaller cos, maybe iterated one is needed

        public int kEdge = 0;
        public Edge() { }

        public Edge(List<Point> points, List<Line> lines)
        {
            this.points = points;
            this.lines = lines;
        }

        /*
        public bool Equals(Edge other)
        {
            return lines == other.lines && points == other.points;
        }
        */

        private bool IsSharp(Point point1, Point point2, Point point3)
        {
            Vector first = point1 - point2;
            Vector second = point3 - point2;

            if ((first * second) / Math.Sqrt((first * first) * (second * second)) < smallestCosOfAngle)
                return false;
            return true;
        }

        private void CreateLinesFromPoints()
        {
            lines = new List<Line>();
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
        }

        public List<Vertex> Shorten()
        {
            var removed = new List<Vertex>();

            var shortenPoints = new List<Point> { points[0], points[1] };
            for (int i = 2; i < points.Count; i++)
            {
                while(
                    shortenPoints.Count - 2 >= 0
                    &&
                    IsSharp(shortenPoints[shortenPoints.Count - 2], shortenPoints[shortenPoints.Count - 1], points[i])
                    &&
                    !CollisionDetection.IntersectsSomeLine(
                        new Line
                        {
                            X1 = shortenPoints[shortenPoints.Count - 2].X,
                            Y1 = shortenPoints[shortenPoints.Count - 2].Y,
                            X2 = points[i].X,
                            Y2 = points[i].Y
                        }))
                {
                    shortenPoints.RemoveAt(shortenPoints.Count - 1);
                    removed.Add(new Vertex { ellipse = new Ellipse(), center = shortenPoints[shortenPoints.Count - 1], state = VertexState.Middle });
                }

                shortenPoints.Add(points[i]);
            }

            CreateLinesFromPoints();

            return removed;
        }
    }


}
