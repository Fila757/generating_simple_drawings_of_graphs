using System.Collections.Generic;
using System.Windows;
using System.Windows.Shapes;

namespace VizualizerWPF
{
    class Edge
    {
        public List<Point> points;
        public List<Line> lines;

        public Edge() { }

        public Edge(List<Point> points, List<Line> lines)
        {
            this.points = points;
            this.lines = lines;
        }
    }


}
