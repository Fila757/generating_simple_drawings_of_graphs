using System.Windows;
using System.Windows.Shapes;

namespace VizualizerWPF
{
    enum VertexState { Intersection, Regular };

    class Vertex
    {
        public Ellipse ellipse;
        public Point center;
        public VertexState state;


        public Vertex() {}

        public Vertex(Ellipse ellipse, Point point, VertexState state)
        {
            this.ellipse = ellipse;
            center = point;
            this.state = state;
        }

        public static bool operator ==(Vertex a, Vertex b)
        {
            return a.center == b.center && a.state == b.state;
        }

        public static bool operator !=(Vertex a, Vertex b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (obj is null || !(obj is Vertex))
                return false;

            var vertex = (Vertex)obj;
            return center == vertex.center;
        }

        public override int GetHashCode()
        {
            return center.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }


}
