using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Shapes;

namespace VizualizerWPF
{
    /// <summary>
    /// Class to store graph to know what to draw on canvas
    /// <param name="vertices">Hashset to store vertices</param>
    /// <param name="edge">List to store edges</param>
    /// <param name="neigbors">Dictionary to store neighbors of all vertices</param>
    /// </summary>
    class GraphCoordinates
    {
        
        public HashSet<Vertex> vertices = new HashSet<Vertex>();
        public List<Edge> edges = new List<Edge>();
        public Dictionary<Vertex, List<Vertex>> neighbors = new Dictionary<Vertex, List<Vertex>>();

        /// <summary>
        /// Safe version of adding to dictionary 
        /// if value is not present, new list is created
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddToDictionary(Vertex key, Vertex value)
        {
            if (neighbors.ContainsKey(key))
            {
                neighbors[key].Add(value);
            }
            else
            {
                neighbors[key] = new List<Vertex>();
                neighbors[key].Add(value);
            }
        }
    }

    /// <summary>
    /// Class to read drawing from file, create
    /// and store graphs
    /// </summary>
    class GraphGenerator
    {

        StreamReader streamReader;
        public int SizeOfGraph {get; set;}

        GraphCoordinates graphCoordinates = new GraphCoordinates();

        /// <summary>
        /// it supposed to run from in it Visual Studio from bin/Debug(Release)/netcoreapp3.1/
        /// </summary>
        /// <param name="n">Size of wanted clique</param>
        public GraphGenerator(int n)
        {
            SizeOfGraph = n;
            streamReader = new StreamReader(
                @"data/graph" + SizeOfGraph + ".txt");
        }

        public void CloseFile()
        {
            streamReader.Dispose(); //delete if we want to remain state of file
        }
        /// <summary>
        /// Function to sort order of points to 
        /// create continous line
        /// </summary>
        /// <param name="line">One line</param>
        /// <returns>Parsed points to line</returns>
        double [] SortFromStartToEnd(string [] line)
        {
            List<double> doubleLine = new List<double>();

            int FindNextLine(Point a)
            {
                for(int i = 0; i < line.Length / 4; i++)
                {
                    var from = new Point { X = Double.Parse(line[4 * i + 0]), Y = Double.Parse(line[4 * i + 1]) };
                    if (from == a)
                        return i;
                }

                return -1;
            }

            int FindPreviousLine(Point a)
            {
                for (int i = 0; i < line.Length / 4; i++)
                {
                    var to = new Point { X = Double.Parse(line[4 * i + 2]), Y = Double.Parse(line[4 * i + 3]) };
                    if (to == a)
                        return i;
                }

                return -1;
            }

            int FindFirst()
            {
                for (int i = 0; i < line.Length / 4; i++)
                {
                    var from = new Point { X = Double.Parse(line[4 * i + 0]), Y = Double.Parse(line[4 * i + 1]) };

                    if (FindPreviousLine(from) == -1)
                        return i;
                }

                throw new ArgumentException("there is no first line");
            }

            int FindLast()
            {
                for (int i = 0; i < line.Length / 4; i++)
                {
                    var to = new Point { X = Double.Parse(line[4 * i + 2]), Y = Double.Parse(line[4 * i + 3]) };
                    if (FindNextLine(to) == -1)
                        return i;
                }

                throw new ArgumentException("there is no last line");
            }

            int first = FindFirst();
            int last = FindLast();

            int cur = first;
            while(cur != last)
            {
                for (int i = 0; i < 4; i++)
                    doubleLine.Add(Double.Parse(line[4 * cur + i]));

                cur = FindNextLine(new Point { X = doubleLine[doubleLine.Count - 2], Y = doubleLine[doubleLine.Count - 1] });
            }
            for (int i = 0; i < 4; i++)
                doubleLine.Add(Double.Parse(line[4 * cur + i]));

            return doubleLine.ToArray();
        }

        /// <summary>
        /// Read whole points and lines of cliques
        /// and generate graph out of it
        /// </summary>
        /// <returns>New graph</returns>
        GraphCoordinates ReadUntillNextRS()
        {
            graphCoordinates = new GraphCoordinates();

            string line;
            while (!String.Equals((line = streamReader.ReadLine()), "#") && line != null)
            {
                List<Vertex> vertices = new List<Vertex>();

                string[] temp = line.Split();

                var edge = new Edge(new List<Point>(), new List<Line>());

                double[] sortedLine = SortFromStartToEnd(temp);

                for (int i = 0; i < sortedLine.Length / 4; i++)
                {
                    var stateFrom = i == 0 ? VertexState.Regular : VertexState.Intersection;
                    var stateTo = i == temp.Length / 4 - 1 ? VertexState.Regular : VertexState.Intersection;

                    var from = new Point { X = sortedLine[4 * i + 0], Y = sortedLine[4 * i + 1] };
                    var to = new Point { X = sortedLine[4 * i + 2], Y = sortedLine[4 * i + 3] };

                    //add just every second . . _ . _ . _ . _ ., . _ is always the same
                    if (i == 0)
                    {
                        vertices.Add(new Vertex(new Ellipse(), from, stateFrom));
                        vertices.Add(new Vertex(new Ellipse(), to, stateTo));
                    }
                    else
                    {
                        vertices.Add(new Vertex(new Ellipse(), to, stateTo));
                    }


                    if (i == 0)
                    {
                        edge.points.Add(from);
                        edge.points.Add(to);
                    }
                    else
                    {
                        edge.points.Add(to);
                    }


                    edge.lines.Add(new Line
                    {
                        X1 = from.X,
                        X2 = to.X,
                        Y1 = from.Y,
                        Y2 = to.Y,
                    }); ;

                }

                graphCoordinates.edges.Add(edge);

                Vertex actualZero = vertices[0]; Vertex actualLast = vertices[vertices.Count - 1];
                if (graphCoordinates.vertices.Contains(vertices[0]))
                    graphCoordinates.vertices.TryGetValue(vertices[0], out actualZero);
                if (graphCoordinates.vertices.Contains(vertices[vertices.Count - 1]))
                    graphCoordinates.vertices.TryGetValue(vertices[vertices.Count - 1], out actualLast);

                graphCoordinates.AddToDictionary(actualZero, actualLast);
                graphCoordinates.AddToDictionary(actualLast, actualZero);

                graphCoordinates.vertices.AddList(vertices);
            }

            return graphCoordinates;
        }

        public GraphCoordinates GetTopicalDrawing()
        {
            return graphCoordinates;
        }

        /// <summary>
        /// Get next drawing
        /// </summary>
        /// <returns>New graph</returns>
        public GraphCoordinates GenerateNextDrawing()
        {
            return ReadUntillNextRS();
        }
    }
}
