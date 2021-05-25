using System.Windows;

namespace VizualizerWPF
{
    /// <summary>
    ///     Class to extend methods as Scale Point
    ///     Add two Points, Multiply point with int
    ///     Substract two points and
    ///     Method point ToVector
    /// </summary>
    public static class PointExtensions
    {
        public static Point Scale(this Point point, double scale)
        {
            var tempPoint = new Point();
            tempPoint.X = point.X * scale;
            tempPoint.Y = point.Y * scale;
            return tempPoint;
        }

        public static Point Scale(this Point point, double scaleX, double scaleY)
        {
            var tempPoint = new Point();
            tempPoint.X = point.X * scaleX;
            tempPoint.Y = point.Y * scaleY;
            return tempPoint;
        }

        public static Point Add(this Point point, Point point1)
        {
            var tempPoint = new Point();
            tempPoint.X = point.X + point1.X;
            tempPoint.Y = point.Y + point1.Y;
            return tempPoint;
        }

        public static Point Mulitply(this Point point, int scale)
        {
            return new Point {X = point.X * scale, Y = point.Y * scale};
        }

        public static Point Substract(this Point point1, Point point2)
        {
            return point1.Add(point2.Mulitply(-1));
        }

        public static Vector ToVector(this Point point)
        {
            return (Vector) point;
        }

        public static Point ToPoint(this Vector vector)
        {
            return (Point) vector;
        }

        /// <summary>
        ///     Get the ratio of two parralel vectors
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <returns></returns>
        public static double Divide(this Vector vector1, Vector vector2)
        {
            if (vector2.X == 0 && vector1.X == 0)
                return vector1.Y / vector2.Y;
            return vector1.X / vector2.X;
        }

        public static Point Substract(this Point point, double a)
        {
            return new Point(point.X - a, point.Y - a);
        }

        public static Vector Scale(this Vector vector, double a)
        {
            return vector.ToPoint().Scale(a).ToVector();
        }
    }
}