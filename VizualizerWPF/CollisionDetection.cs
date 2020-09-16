using System.Windows.Media;
using System.Windows.Shapes;
using Path = System.Windows.Shapes.Path;

namespace VizualizerWPF
{
    class CollisionDetection
    {

        static MainWindow window;

        public static void Init(MainWindow window)
        {
            CollisionDetection.window = window;
        } 

        static double epsilon = 0.5;

        public static Coordinates TwoLines(Line line1, Line line2)
        {
            var denominator = (line2.Y2 - line2.Y1) * (line1.X2 - line1.X1) - (line2.X2 - line2.X1) * (line1.Y2 - line1.Y1);

            var numT = -line1.X1 * (line2.Y2 - line2.Y1) + line1.Y1 * (line2.X2 - line2.X1) +
                 line2.X1 * (line2.Y2 - line2.Y1) - line2.Y1 * (line2.X2 - line2.X1);
            var numS = -line1.X1 * (line1.Y2 - line1.Y1) + line1.Y1 * (line1.X2 - line1.X1) +
                line2.X1 * (line1.Y2 - line1.Y1) - line2.Y1 * (line1.X2 - line1.X1);
            

            var s = numS / denominator;
            var t = numT / denominator;
            //MessageBox.Show(denominator.ToString() + " " +  numT.ToString() + " " + numS.ToString());
            if(denominator != 0 && (s >= 0 && s <= 1) && (t >= 0 && t <= 1))
                return new Coordinates { x = line1.X1 + (line1.X2 - line1.X1) * t, y = line1.Y1 + (line1.Y2 - line1.Y1) * t };
            return new Coordinates { x = -1, y = -1 };
        }

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

        public static bool LineAndEllipse(Line line, Ellipse ellipse)
        {
            return new Coordinates { x = -1, y = -1 } ==
                TwoLines(line,
                new Line
                {
                    X1 = ellipse.Margin.Left,
                    Y1 = ellipse.Margin.Top,
                    X2 = ellipse.Margin.Left + window.sizeOfVertex,
                    Y2 = ellipse.Margin.Top + window.sizeOfVertex
                }) ? false : true;
        }

        public static bool TwoPaths(Path path1, Path path2)
        {
            var intersectionDetail = path1.Data.FillContainsWithDetail(path2.Data);

            if (intersectionDetail != IntersectionDetail.NotCalculated && intersectionDetail != IntersectionDetail.Empty)
                return true;
            return false;
        }
    }
}
