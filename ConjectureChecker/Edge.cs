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

        public static double smallestCosOfAngle = 0.8; //cca 30 \degree we can take only smaller cos, maybe iterated one is needed

        public int kEdge = 0;
        public Edge() { }

        public Edge(List<Point> points, List<Line> lines)
        {
            this.points = points;
            this.lines = lines;
        }

        /// <summary>
        /// Function to detect, whether consectuive points <c>point1</c>, <c>point2</c>, <c>point3</c> are at angle less then cca 30°
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="point3"></param>
        /// <returns></returns>
        private bool IsSharp(Point point1, Point point2, Point point3)
        {
            Vector first = point1 - point2;
            Vector second = point3 - point2;

            if ((first * second) / Math.Sqrt((first * first) * (second * second)) < smallestCosOfAngle) //negative ones are alright
                return false;
            return true;
        }

        /// <summary>
        /// Function to connect points to create line segment between them.
        /// </summary>
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


        /// <summary>
        /// Function to go through the Edge and shorten if it is wanted.
        /// </summary>
        /// <param name="graphCoordinates"></param>
        public void Shorten(in GraphCoordinates graphCoordinates)
        {
            //var removed = new List<Vertex>();

            var shortenPoints = new List<Point> { points[0], points[1] };
            for (int i = 2; i < points.Count; i++)
            {
                while(
                    shortenPoints.Count - 2 >= 0
                    &&
                    //IsSharp(shortenPoints[shortenPoints.Count - 2], shortenPoints[shortenPoints.Count - 1], points[i])
                    //&&
                    graphCoordinates.FindVertex(shortenPoints[shortenPoints.Count - 1]).state == VertexState.Middle
                    &&
                    IsShortEnough(points[shortenPoints.Count - 2], points[shortenPoints.Count - 1], points[i])
                    &&
                    !CollisionDetection.IntersectsSomeLine(
                        new Line
                        {
                            X1 = shortenPoints[shortenPoints.Count - 2].X,
                            Y1 = shortenPoints[shortenPoints.Count - 2].Y,
                            X2 = points[i].X,
                            Y2 = points[i].Y
                        }, graphCoordinates)
                    )
                    
                {
                    shortenPoints.RemoveAt(shortenPoints.Count - 1);
                }

                shortenPoints.Add(points[i]);
            }

            points = shortenPoints;
            CreateLinesFromPoints();

            //return removed;
        }
        /// <summary>
        /// This function is called by <c>Shorten</c> to shorten only small enough lines
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="point3"></param>
        /// <returns></returns>

        private bool IsShortEnough(Point point1, Point point2, Point point3)
        {
            // 20 is delta in forces
            if((point1 - point2).Length < 100 && (point2 - point3).Length < 100)
                return true;
            return false;
        }
    }


}
