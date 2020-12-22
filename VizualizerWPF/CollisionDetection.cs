using Syncfusion.Windows.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Path = System.Windows.Shapes.Path;

namespace VizualizerWPF
{

    class LineWithCoeffients
    {
        public double a;
        public double b;
        public double c;
    }

    class HalfLineWithCoeffients
    {
        public Line line;
        
        public LineWithCoeffients border;
        public int direction;

    }
    class CollisionDetection
    {
        /// <summary>
        /// Class to detect many types of collision in 2D 
        /// </summary>
        static MainWindow window;

        public static void Init(MainWindow window)
        {
            CollisionDetection.window = window;
        }

        /// <summary>
        /// Error
        /// </summary>
        static double epsilon = 0.5;



        public static Point? LineAndHalfLine (Line line1, HalfLineWithCoeffients halfLine)
        {
            var line2 = halfLine.line;

            var denominator = (line2.Y2 - line2.Y1) * (line1.X2 - line1.X1) - (line2.X2 - line2.X1) * (line1.Y2 - line1.Y1);

            if (denominator == 0)
            {
                //MessageBox.Show("Parralel");
                return null;
                //throw new ArgumentException("Line and Halfline are parralel"); 
                //first special case when parralel

            }
            var numT = -line1.X1 * (line2.Y2 - line2.Y1) + line1.Y1 * (line2.X2 - line2.X1) +
                 line2.X1 * (line2.Y2 - line2.Y1) - line2.Y1 * (line2.X2 - line2.X1);
            var numS = -line1.X1 * (line1.Y2 - line1.Y1) + line1.Y1 * (line1.X2 - line1.X1) +
                line2.X1 * (line1.Y2 - line1.Y1) - line2.Y1 * (line1.X2 - line1.X1);


            var s = numS / denominator;
            var t = numT / denominator;
            
            if(t >= -0.001 && t <= 1.001) //line1 is just segments so we need to check limits of s
                return new Point { X = line1.X1 + (line1.X2 - line1.X1) * t, Y = line1.Y1 + (line1.Y2 - line1.Y1) * t };
            return null;
        }
        /// <summary>
        /// Detect intersection of two line segment
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <returns></returns>
        public static Point TwoLines(Line line1, Line line2)
        {
            var denominator = (line2.Y2 - line2.Y1) * (line1.X2 - line1.X1) - (line2.X2 - line2.X1) * (line1.Y2 - line1.Y1);

            var numT = -line1.X1 * (line2.Y2 - line2.Y1) + line1.Y1 * (line2.X2 - line2.X1) +
                 line2.X1 * (line2.Y2 - line2.Y1) - line2.Y1 * (line2.X2 - line2.X1);
            var numS = -line1.X1 * (line1.Y2 - line1.Y1) + line1.Y1 * (line1.X2 - line1.X1) +
                line2.X1 * (line1.Y2 - line1.Y1) - line2.Y1 * (line1.X2 - line1.X1);


            var s = numS / denominator;
            var t = numT / denominator;
            //MessageBox.Show(denominator.ToString() + " " +  numT.ToString() + " " + numS.ToString());
            if (denominator != 0 && (s >= -0.001 && s <= 1.001) && (t >= -0.001 && t <= 1.001))
                return new Point { X = line1.X1 + (line1.X2 - line1.X1) * t, Y = line1.Y1 + (line1.Y2 - line1.Y1) * t };
            return new Point { X = -1, Y = -1 };
        }
        /// <summary>
        /// Function to detect if end of the <c>line</c> is in given <c>ellipse</c>(circle)
        /// </summary>
        /// <param name="line"></param>
        /// <param name="ellipse"></param>
        /// <returns></returns>
        public static bool LineAndEllipseAtEnd(Line line, Ellipse ellipse)
        {
            var intersection1 = (line.X1 - (ellipse.Margin.Left + window.sizeOfVertex / 2)) * (line.X1 - (ellipse.Margin.Left + window.sizeOfVertex / 2)) +
                (line.Y1 - (ellipse.Margin.Top + window.sizeOfVertex / 2)) * (line.Y1 - (ellipse.Margin.Top + window.sizeOfVertex / 2));

            var intersection2 = (line.X2 - (ellipse.Margin.Left + window.sizeOfVertex / 2)) * (line.X2 - (ellipse.Margin.Left + window.sizeOfVertex / 2)) +
                (line.Y2 - (ellipse.Margin.Top + window.sizeOfVertex / 2)) * (line.Y2 - (ellipse.Margin.Top + window.sizeOfVertex / 2));

            if (intersection1 <= window.sizeOfVertex * window.sizeOfVertex + epsilon || intersection2 <= window.sizeOfVertex * window.sizeOfVertex + epsilon)
                return true;

            return false;
        }

        /// <summary>
        /// Detect if center of <c>ellipse</c> lies on given <c>line</c>
        /// </summary>
        /// <param name="line"></param>
        /// <param name="ellipse"></param>
        /// <returns></returns>
        public static bool CenterOfEllipseOnLine(Line line, Ellipse ellipse)
        {
            var center = new Point { X = ellipse.Margin.Left + window.sizeOfVertex / 2, Y = ellipse.Margin.Top + window.sizeOfVertex / 2 };
            if (((line.Y1 - line.Y2) * center.X + (line.X2 - line.X1) * center.Y
                    + (line.X1 - line.X2) * line.Y1 + (line.Y2 - line.Y1) * line.X1 <= epsilon
                    &&
                    (line.Y1 - line.Y2) * center.X + (line.X2 - line.X1) * center.Y
                    + (line.X1 - line.X2) * line.Y1 + (line.Y2 - line.Y1) * line.X1 >= -epsilon)
                    &&
                   Math.Min(line.X1, line.X2) <= center.X
                   &&
                   center.X <= Math.Max(line.X1, line.X2)
                   &&
                   Math.Min(line.Y1, line.Y2) <= center.Y
                   &&
                   center.Y <= Math.Max(line.Y1, line.Y2))
                return true;
            return false;
        }


        public static bool CenterOnLine(Line line, Point center)
        { 
            if (((line.Y1 - line.Y2) * center.X + (line.X2 - line.X1) * center.Y
                    + (line.X1 - line.X2) * line.Y1 + (line.Y2 - line.Y1) * line.X1 <= epsilon
                    &&
                    (line.Y1 - line.Y2) * center.X + (line.X2 - line.X1) * center.Y
                    + (line.X1 - line.X2) * line.Y1 + (line.Y2 - line.Y1) * line.X1 >= -epsilon)
                    &&
                   Math.Min(line.X1, line.X2) <= center.X
                   &&
                   center.X <= Math.Max(line.X1, line.X2)
                   &&
                   Math.Min(line.Y1, line.Y2) <= center.Y
                   &&
                   center.Y <= Math.Max(line.Y1, line.Y2))
                return true;
            return false;
        }

        /// <summary>
        /// Detect if <c>point</c> is inside <c>ellipse</c> (circle)
        /// </summary>
        /// <param name="point"></param>
        /// <param name="ellipse"></param>
        /// <returns></returns>
        public static bool CenterInsideEllipse(Point point, Ellipse ellipse)
        {
            var intersection = (point.X - (ellipse.Margin.Left + window.sizeOfVertex / 2)) * (point.X - (ellipse.Margin.Left + window.sizeOfVertex / 2)) +
                (point.Y - (ellipse.Margin.Top + window.sizeOfVertex / 2)) * (point.Y - (ellipse.Margin.Top + window.sizeOfVertex / 2));

            if (intersection <= window.sizeOfVertex * window.sizeOfVertex + epsilon)
                return true;
            
            return false;
        }

        /// <summary>
        /// Detect if <c>ellipse</c> and <c>line</c> intersects 
        /// </summary>
        /// <param name="line"></param>
        /// <param name="ellipse"></param>
        /// <returns></returns>
        public static bool LineAndEllipse(Line line, Ellipse ellipse)
        {
            return new Point { X = -1, Y = -1 } ==
                TwoLines(line,
                new Line
                {
                    X1 = ellipse.Margin.Left,
                    Y1 = ellipse.Margin.Top,
                    X2 = ellipse.Margin.Left + window.sizeOfVertex,
                    Y2 = ellipse.Margin.Top + window.sizeOfVertex
                }) ? false : true;
        }

        /// <summary>
        /// General collision detection for further generalization 
        /// and bezier lines 
        /// </summary>
        /// <param name="path1">first object</param>
        /// <param name="path2">second object</param>
        /// <returns></returns>
        public static bool TwoPaths(Path path1, Path path2)
        {
            var intersectionDetail = path1.Data.FillContainsWithDetail(path2.Data);

            if (intersectionDetail != IntersectionDetail.NotCalculated && intersectionDetail != IntersectionDetail.Empty)
                return true;
            return false;
        }


        static Point GetAlmostMid(Line line)
        {
            return new Point((2 * line.X1 + line.X2) / 3, (2 * line.Y1 + line.Y2) / 3); 
        }


        static LineWithCoeffients GetLineWithCoefficients(Line line)
        {
            return new LineWithCoeffients {
                a = line.Y1 - line.Y2,
                b = line.X2 - line.X1,
                c = (line.X1 - line.X2) * line.Y1 + (line.Y2 - line.Y1) * line.X1 
            };
        }

        /*
        static LineWithCoeffients GetLineWithCoeffientsByPointAndVector(Point point, Vector v)
        {
            double c = -(v.X * point.X + v.Y * point.Y);

            return new LineWithCoeffients { a = v.X, b = v.Y, c = c };
        }
        */

        static (HalfLineWithCoeffients, HalfLineWithCoeffients) GetPerpendicularToAlmostMid(Line line) {
            var mid = GetAlmostMid(line);

            var border = GetLineWithCoefficients(line);
            var newLine = new Line {
                X1 = mid.X,
                Y1 = mid.Y,
                X2 = (mid + new Vector { X = border.a, Y = border.b }).X, //orthogonal vector is given in eqution of line
                Y2 = (mid + new Vector { X = border.a, Y = border.b }).Y 
            };

            var leftHalfLine = new HalfLineWithCoeffients {
                border = border,
                line = newLine,
                direction = 1
                };

            var rightHalfLine = new HalfLineWithCoeffients
            {
                border = border,
                line = newLine,
                direction = -1
            };

            return (leftHalfLine, rightHalfLine);
        }

        public static int GetOrientation(Line mainLine, List<Line> lines)
        {

            (HalfLineWithCoeffients, HalfLineWithCoeffients) halfLines = GetPerpendicularToAlmostMid(mainLine);

            var leftHalfLine = halfLines.Item1;

            int numberOfIntersections = 0;
            foreach(var line in lines)
            {
                var intersectionOrNull = LineAndHalfLine(line, leftHalfLine);

                if (intersectionOrNull.HasValue)
                {
                    Point intersection = intersectionOrNull.Value; 
                    if (leftHalfLine.border.a * intersection.X + leftHalfLine.border.b * intersection.Y + leftHalfLine.border.c > 0)
                        numberOfIntersections++;
                }
            }

            if (numberOfIntersections % 2 == 1)
                return 1;
            return -1;
        }

        static Line MakeReversedLine(Line line)
        {
            return new Line { X1 = line.X2, Y1 = line.Y2, X2 = line.X1, Y2 = line.Y1 };
        }

        public static Point ChooseOppositeOne(Edge edge, Point point)
        {
            return Vertex.Compare(edge.points[0], point) ? edge.points.Last() : edge.points[0];
        }

        public static (Line, List<Line>) PutLinesTogether(Edge e1, Edge e2, Edge e3)
        {
            var result = new List<Line>();

            for (int i = 1; i < e1.lines.Count; i++) //skip the first one and return as mainLine
                result.Add(e1.lines[i]);

            foreach (var line in e2.lines)
                result.Add(line);

            foreach (var line in e3.lines)
                result.Add(line);

            return (e1.lines[0], result);
        }

    }
}
