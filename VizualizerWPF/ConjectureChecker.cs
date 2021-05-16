using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Shapes;

namespace VizualizerWPF
{
    class ConjectureChecker
    {

        public static ConjectureChecker Instance
        {
            get { return instance; }
        }

        private static ConjectureChecker instance = new ConjectureChecker();

        int maximalKEdges = 8;
        /// <summary>
        /// Function to check if all faces satisfy our conjecture
        /// </summary>
        private static IEnumerable<Point> GetAllReferenceFaces(GraphCoordinates graphCoordinates)
        {
            foreach (var vertex in graphCoordinates.GetVerticesAndIntersections())
            {
                List<Vector> firstLines = new List<Vector>();
                foreach (var l in graphCoordinates.GetLines(vertex))
                {
                    firstLines.Add(new Vector(l.X2 - l.X1, l.Y2 - l.Y1));
                }
                firstLines.Sort(delegate (Vector l1, Vector l2) { return CollisionDetection.CompareLinesByAngle(l1, l2); });

                for (int i = 0; i < firstLines.Count; i++)
                {

                    if (firstLines[i] / firstLines[i].Length == -firstLines[(i + 1) % firstLines.Count] / firstLines[(i + 1) % firstLines.Count].Length) //when there are opposite ones, the res can be NaN after normalizarion, because res can be zero in that case and we do not care, because there is another vertex on this face (angle 180 can be considered as this vertex is not on the face)
                        continue;

                    if (firstLines[i] / firstLines[i].Length == firstLines[(i + 1) % firstLines.Count] / firstLines[(i + 1) % firstLines.Count].Length) //when two lines are same direction, the face between them is empty and it is bad corner case
                        continue;

                    double minLength = Math.Min(firstLines[i].Length, firstLines[(i + 1) % firstLines.Count].Length);
                    Vector res = firstLines[i] + firstLines[(i + 1) % firstLines.Count];
                    res.Normalize();
                    res *= (minLength / 4);

                    if (CollisionDetection.Determinant(firstLines[i], firstLines[(i + 1) % firstLines.Count]) > 0)
                    {
                        res *= -1; res.Normalize(); res *= 10;
                    }


                    yield return (vertex.center + res);
                }

            }
        }


        private bool TryFace3AMK(GraphCoordinates graphCoordinates)
        {
            (var kEdgesArray, _) = graphCoordinates.ReCalculateKEdges();
            var AMKEdgesArray = CustomMath.CalculateAmEdges(maximalKEdges, kEdgesArray);
            return graphCoordinates.Chech3AMKConjecture(AMKEdgesArray, maximalKEdges);
        }

        private bool TryFace2AMK(GraphCoordinates graphCoordinates)
        {
            (var kEdgesArray, _) = graphCoordinates.ReCalculateKEdges();
            var AMKEdgesArray = CustomMath.CalculateAmEdges(maximalKEdges, kEdgesArray);
            return graphCoordinates.Chech2AMKConjecture(AMKEdgesArray, maximalKEdges);
        }

        void Check3AMKConjecture() {
            GraphCoordinates graphCoordinates;

            for (int i = 4; i <= 8; i++)
            {
                var graphGenerator = new GraphGenerator(i);


                while ((graphCoordinates = graphGenerator.GenerateNextDrawing()).vertices.Count != 0)
                {
                    Console.WriteLine(graphGenerator.counter);
                    foreach(Point facePoint in GetAllReferenceFaces(graphCoordinates))
                    {
                        GraphCoordinates.facePoint = facePoint;
                        if (!TryFace3AMK(graphCoordinates))
                        {
                            Console.WriteLine("HEUREKA WRONG 3AMK", i, graphGenerator.counter);
                        }
                    }
                }

            }
        }

        void Check2AMKConjecture()
        {
            GraphCoordinates graphCoordinates;

            for (int i = 4; i <= 8; i++)
            {
                var graphGenerator = new GraphGenerator(i);


                while ((graphCoordinates = graphGenerator.GenerateNextDrawing()).vertices.Count != 0)
                {
                    bool goodFace = false;
                    Console.WriteLine(graphGenerator.counter);
                    foreach (Point facePoint in GetAllReferenceFaces(graphCoordinates))
                    {
                        GraphCoordinates.facePoint = facePoint;
                        if (TryFace2AMK(graphCoordinates))
                        {
                            goodFace = true;
                        }
                    }
                    if (!goodFace)
                    {
                        Console.WriteLine("HEUREKA WRONG 2AMK", i, graphGenerator.counter);
                    }
                }

            }
        }

        static void Main(string[] args)
        {

            Instance.Check3AMKConjecture();
            Instance.Check2AMKConjecture();
        }




    }
}
