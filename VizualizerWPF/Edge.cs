using System.Collections.Generic;
using System.Windows;
using System.Windows.Shapes;

namespace VizualizerWPF
{
    /// <summary>
    /// Class to store edges in graph
    /// meaming all lines and points
    /// </summary>
    class Edge
    {
        public List<Point> points;
        public List<Line> lines;
        public int direction = 1;
        public Edge() { }

        public Edge(List<Point> points, List<Line> lines)
        {
            this.points = points;
            this.lines = lines;
        }
    }


}
