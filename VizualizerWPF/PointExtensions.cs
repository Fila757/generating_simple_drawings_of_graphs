using System.Windows;

namespace VizualizerWPF
{
    public static class PointExtensions
    {
        public static Point Scale(this Point point, double scale)
        {
            Point tempPoint = new Point();
            tempPoint.X = point.X * scale;
            tempPoint.Y =  point.Y* scale;
            return tempPoint;
        }

        public static Point Add(this Point point, Point point1)
        {
            Point tempPoint = new Point();
            tempPoint.X = point.X + point1.X;
            tempPoint.Y = point.Y + point1.Y;
            return tempPoint;
        }
    }

}
