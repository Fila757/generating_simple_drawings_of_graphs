using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Shapes;
using System;

namespace VizualizerWPF
{
    /// <summary>
    /// Enum to recongize state of vertex
    /// </summary>
    public enum VertexState { Intersection, Regular , Middle};

    /// <summary>
    /// Struct to store vertices
    /// <param name="ellipse">Drawing of vertex</param>
    /// <param name="center">Center of vertex</param>
    /// <param name="state">State of vertex</param>
    /// Implementing  IEqualityComparer<Vertex>, IEquatable<Vertex> 
    /// to index HashSet and Dictionary
    /// </summary>
    public struct Vertex : IEqualityComparer<Vertex>, IEquatable<Vertex>
    {
        public Ellipse ellipse;
        public Point center ;
        public VertexState state;

        public static bool Compare(Point a, Point b)
        {
            if (Math.Abs(a.X - b.X) < 0.01 && Math.Abs(a.Y - b.Y) < 0.01)
                return true;
            return false;
        }

        public Vertex(Ellipse ellipse, Point point, VertexState state)
        {
            this.ellipse = ellipse;
            center = point;
            this.state = state;
        }

        public static bool operator ==(Vertex a, Vertex b)
        {
            return Compare(a.center, b.center);
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
            return Compare(center, vertex.center);
        }

        public override int GetHashCode()
        {
            return center.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public bool Equals(Vertex x, Vertex y)
        {
            return x == y;
        }

        public int GetHashCode(Vertex obj)
        {
            return obj.GetHashCode();
        }

        public bool Equals(Vertex other)
        {
            return this == other;
        }
    }


}
