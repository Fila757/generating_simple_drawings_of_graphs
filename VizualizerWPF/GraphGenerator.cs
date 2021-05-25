using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Shapes;

namespace VizualizerWPF
{
    /// <summary>
    ///     Class to read drawing from file, create
    ///     and store graphs
    /// </summary>
    internal class GraphGenerator
    {
        public int counter;

        private GraphCoordinates graphCoordinates = new GraphCoordinates();

        private readonly StreamReader streamReader;


        public GraphGenerator(int n)
        {
            SizeOfGraph = n;
            streamReader = new StreamReader(@"data/graph" + SizeOfGraph + ".txt");
        }

        public GraphGenerator()
        {
            streamReader = new StreamReader(@"../../../data/savedGraphs.txt");
        }

        public int SizeOfGraph { get; set; }


        public void CloseFile()
        {
            streamReader.Dispose(); //delete if we want to remain state of file
        }

        /// <summary>
        ///     Function to sort order of points to
        ///     create continous edge
        /// </summary>
        /// <param name="line">One line</param>
        /// <returns>Parsed points to line</returns>
        private List<double[]> SortFromStartToEnd(List<string[]> line)
        {
            var doubleLine = new List<double[]>();

            int FindNextLine(Point a, int cur)
            {
                for (var i = 0; i < line.Count; i++)
                {
                    //if (i != cur)
                    //{
                    var from = new Point
                        {X = double.Parse(line[i].ElementAt(0)), Y = double.Parse(line[i].ElementAt(1))};
                    if (from == a)
                        return i;
                    //}
                }

                return -1;
            }

            int FindPreviousLine(Point a)
            {
                for (var i = 0; i < line.Count; i++)
                {
                    var to = new Point
                    {
                        X = double.Parse(line[i].ElementAt(line[i].Length - 2)),
                        Y = double.Parse(line[i].ElementAt(line[i].Length - 1))
                    };
                    if (to == a)
                        return i;
                }

                return -1;
            }

            int FindFirst()
            {
                for (var i = 0; i < line.Count; i++)
                {
                    var from = new Point
                        {X = double.Parse(line[i].ElementAt(0)), Y = double.Parse(line[i].ElementAt(1))};

                    if (FindPreviousLine(from) == -1)
                        return i;
                }

                throw new ArgumentException("there is no first line");
            }

            int FindLast()
            {
                for (var i = 0; i < line.Count; i++)
                {
                    var to = new Point
                    {
                        X = double.Parse(line[i].ElementAt(line[i].Length - 2)),
                        Y = double.Parse(line[i].ElementAt(line[i].Length - 1))
                    };
                    if (FindNextLine(to, -1) == -1)
                        return i;
                }

                throw new ArgumentException("there is no last line");
            }

            var first = FindFirst();
            var last = FindLast();

            double[] temp;
            var cur = first;
            while (cur != last)
            {
                temp = new double[line[cur].Length];
                for (var i = 0; i < line[cur].Length; i++)
                    temp[i] = double.Parse(line[cur][i]);
                doubleLine.Add(temp);

                cur = FindNextLine(
                    new Point
                    {
                        X = doubleLine.Last().ElementAt(doubleLine.Last().Length - 2),
                        Y = doubleLine.Last().ElementAt(doubleLine.Last().Length - 1)
                    }, cur);
            }

            temp = new double[line[cur].Length];
            for (var i = 0; i < line[cur].Length; i++)
                temp[i] = double.Parse(line[cur][i]);
            doubleLine.Add(temp);

            return doubleLine;
        }

        /// <summary>
        ///     Read whole points and lines of cliques
        ///     and generate graph out of it
        /// </summary>
        /// <returns>New graph</returns>
        private GraphCoordinates ReadUntillNextRS(bool skip = false)
        {
            graphCoordinates = new GraphCoordinates();

            string line;
            while (!string.Equals(line = streamReader.ReadLine(), "#") && line != null)
            {
                if (line == "" || skip)
                    continue;

                var vertices = new List<Vertex>();

                var temp = line.Split(new[] {'(', ')'}, StringSplitOptions.RemoveEmptyEntries);

                temp = temp.Where(x => x != " ").ToArray();

                var tempList = new List<string[]>();
                foreach (var t in temp)
                    tempList.Add(t.Split(new char[0], StringSplitOptions.RemoveEmptyEntries));

                var edge = new Edge(new List<Point>(), new List<Line>());

                var sortedLine = SortFromStartToEnd(tempList);

                for (var i = 0; i < sortedLine.Count; i++)
                {
                    var stateFrom = i == 0 ? VertexState.Regular : VertexState.Intersection;
                    var stateTo = i == sortedLine.Count - 1 ? VertexState.Regular : VertexState.Intersection;

                    var points = new List<Point>();

                    for (var j = 0; j < sortedLine[i].Length; j += 2)
                        points.Add(new Point {X = sortedLine[i][j], Y = sortedLine[i][j + 1]});

                    //add just every second . . _ . _ . _ . _ ., . _ is always the same
                    if (i == 0)
                    {
                        vertices.Add(new Vertex(new Ellipse(), points.First(), stateFrom));
                        edge.points.Add(points.First());

                        for (var j = 1; j < points.Count - 1; j++)
                        {
                            vertices.Add(new Vertex(new Ellipse(), points[j], VertexState.Middle));
                            edge.points.Add(points[j]);
                        }

                        vertices.Add(new Vertex(new Ellipse(), points.Last(), stateTo));
                        edge.points.Add(points.Last());
                    }
                    else
                    {
                        for (var j = 1; j < points.Count - 1; j++)
                        {
                            vertices.Add(new Vertex(new Ellipse(), points[j], VertexState.Middle));
                            edge.points.Add(points[j]);
                        }

                        vertices.Add(new Vertex(new Ellipse(), points.Last(), stateTo));
                        edge.points.Add(points.Last());
                    }

                    //foreach (var point in points)
                    //    edge.points.Add(point);


                    for (var j = 0; j < points.Count - 1; j++)
                        edge.lines.Add(new Line
                        {
                            X1 = points[j].X,
                            X2 = points[j + 1].X,
                            Y1 = points[j].Y,
                            Y2 = points[j + 1].Y
                        });
                }

                graphCoordinates.edges.Add(edge);


                var actualZero = vertices[0];
                var actualLast = vertices[vertices.Count - 1];

                /*
                if (graphCoordinates.vertices.Contains(vertices[0]))
                    graphCoordinates.vertices.TryGetValue(vertices[0], out actualZero);
                if (graphCoordinates.vertices.Contains(vertices[vertices.Count - 1]))
                    graphCoordinates.vertices.TryGetValue(vertices[vertices.Count - 1], out actualLast);
                */

                graphCoordinates.AddToDictionary(actualZero, edge);
                graphCoordinates.AddToDictionary(actualLast, edge);

                graphCoordinates.vertices.AddList(vertices);
            }

            return graphCoordinates;
        }

        public GraphCoordinates GetTopicalDrawing()
        {
            return graphCoordinates;
        }


        /// <summary>
        ///     Get next drawing
        /// </summary>
        /// <returns>New graph</returns>
        public GraphCoordinates GenerateNextDrawing()
        {
            counter++;

            return ReadUntillNextRS();
        }

        /// <summary>
        ///     Get previous drawing by going from the beggining of the file (the reopeing of the file is done in the method
        ///     calling this one)
        /// </summary>
        /// <returns></returns>
        public GraphCoordinates GeneratePreviousDrawing()
        {
            counter--;

            var tmpCounter = 1;
            while (tmpCounter < counter)
            {
                ReadUntillNextRS(true);
                tmpCounter++;
            }

            return ReadUntillNextRS();
        }
    }
}