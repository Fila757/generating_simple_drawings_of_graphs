using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace VizualizerWPF
{
    /// <summary>
    ///     <code>
    ///  Point[] points1 = {
    ///     new Point(60, 30),
    ///     new Point(200, 130),
    ///     new Point(100, 150),
    ///     new Point(200, 50),
    /// };
    ///  Path path1 = PolybezierPathMaker.MakeCurve(points1, 0);
    /// 
    ///  path1.Stroke = Brushes.LightGreen;
    ///  path1.StrokeThickness = 5;
    ///  mainCanvas.Children.Add(path1);
    /// 
    ///  Path path2 = PolybezierPathMaker.MakeCurve(new Point[2] { new Point(50, 50), new Point(122, 122) }, 0);
    ///  path2.Stroke = Brushes.LightCoral;
    ///  path2.StrokeThickness = 5;
    ///  mainCanvas.Children.Add(path2);
    /// </code>
    /// </summary>
    internal class PolybezierPathMaker
    {
        private static Point[] MakeCurvePoints(Point[] points, double tension)
        {
            if (points.Length < 2) return null;
            var control_scale = tension / 0.5 * 0.175;

            // Make a list containing the points and
            // appropriate control points.
            var result_points = new List<Point>();
            result_points.Add(points[0]);

            for (var i = 0; i < points.Length - 1; i++)
            {
                // Get the point and its neighbors.
                var pt_before = points[Math.Max(i - 1, 0)];
                var pt = points[i];
                var pt_after = points[i + 1];
                var pt_after2 = points[Math.Min(i + 2, points.Length - 1)];

                var dx1 = pt_after.X - pt_before.X;
                var dy1 = pt_after.Y - pt_before.Y;

                var p1 = points[i];
                var p4 = pt_after;

                var dx = pt_after.X - pt_before.X;
                var dy = pt_after.Y - pt_before.Y;
                var p2 = new Point(
                    pt.X + control_scale * dx,
                    pt.Y + control_scale * dy);

                dx = pt_after2.X - pt.X;
                dy = pt_after2.Y - pt.Y;
                var p3 = new Point(
                    pt_after.X - control_scale * dx,
                    pt_after.Y - control_scale * dy);

                // Save points p2, p3, and p4.
                result_points.Add(p2);
                result_points.Add(p3);
                result_points.Add(p4);
            }

            // Return the points.
            return result_points.ToArray();
        }

        // Make a Path holding a series of Bezier curves.
        // The points parameter includes the points to visit
        // and the control points.
        private static Path MakeBezierPath(Point[] points)
        {
            // Create a Path to hold the geometry.
            var path = new Path();

            // Add a PathGeometry.
            var path_geometry = new PathGeometry();
            path.Data = path_geometry;

            // Create a PathFigure.
            var path_figure = new PathFigure();
            path_geometry.Figures.Add(path_figure);

            // Start at the first point.
            path_figure.StartPoint = points[0];

            // Create a PathSegmentCollection.
            var path_segment_collection =
                new PathSegmentCollection();
            path_figure.Segments = path_segment_collection;

            // Add the rest of the points to a PointCollection.
            var point_collection =
                new PointCollection(points.Length - 1);
            for (var i = 1; i < points.Length; i++)
                point_collection.Add(points[i]);

            // Make a PolyBezierSegment from the points.
            var bezier_segment = new PolyBezierSegment();
            bezier_segment.Points = point_collection;

            // Add the PolyBezierSegment to othe segment collection.
            path_segment_collection.Add(bezier_segment);

            return path;
        }

        // Make a Bezier curve connecting these points.
        public static Path MakeCurve(Point[] points, double tension)
        {
            if (points.Length < 2) return null;
            var result_points = MakeCurvePoints(points, tension);

            // Use the points to create the path.
            return MakeBezierPath(result_points.ToArray());
        }
    }
}