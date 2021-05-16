using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace VizualizerWPF
{
    class ConjectureChecker
    {
        /// <summary>
        /// Function to check if all faces satisfy our conjecture
        /// </summary>
        private static void TryAllReferenceFaces()
        {
            foreach (var vertex in GetVerticesAndIntersections())
            {
                List<Vector> firstLines = new List<Vector>();
                foreach (var l in GetLines(vertex))
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


                    facePoint = vertex.center + res;

                    TryFace();

                }

            }
        }

        private static void TryAllDrawings(GraphCoordinates graphCoordinates)
        {

            for (int i = 4; i <= 7; i++)
            {

                var graphGenerator = new GraphGenerator(i);

                graphCoordinates = graphGenerator.GenerateNextDrawing();
                Console.WriteLine(graphGenerator.counter);
                TryAllReferenceFaces();

                while (graphCoordinates.vertices.Count != 0)
                {
                    Console.WriteLine(graphGenerator.counter);
                    graphCoordinates = graphGenerator.GenerateNextDrawing();
                    TryAllReferenceFaces();
                }

            }
        }

        private static void TryFace()
        {
            ReCalculateKEdges();
        }



    }
}
