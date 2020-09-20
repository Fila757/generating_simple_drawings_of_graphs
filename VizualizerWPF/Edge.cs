using System.Collections.Generic;
using System.Windows;
using System.Windows.Shapes;

namespace VizualizerWPF
{
    struct Edge
    {
        public HashSet<Point> points;
        public List<Line> lines;

        public Edge(HashSet<Point> points, List<Line> lines)
        {
            this.points = points;
            this.lines = lines;
        }
    }


}
