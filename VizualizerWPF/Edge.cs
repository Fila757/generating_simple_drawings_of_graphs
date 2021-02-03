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
    }


}
