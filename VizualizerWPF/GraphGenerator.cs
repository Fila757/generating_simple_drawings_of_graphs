using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Shapes;

namespace VizualizerWPF
{
    class GraphCoordinates
    {
        public List<Vertex> vertices = new List<Vertex>();
        public List<Edge> edges = new List<Edge>();
        public Dictionary<Vertex, List<Edge>> neighbors = new Dictionary<Vertex, List<Edge>>();

        public void AddToDictionary(Vertex key, Edge value)
        {
            if (neighbors.ContainsKey(key))
            {
                key.state = VertexState.Regular; //little bit force idk why there is no always regulars
                neighbors[key].Add(value);
            }
            else
            {
                neighbors[key] = new List<Edge>();
                neighbors[key].Add(value);
            }
        }
    }
    class GraphGenerator
    {
        StreamReader streamReader;
        public int SizeOfGraph {get; set; }

        GraphCoordinates graphCoordinates = new GraphCoordinates();

        public GraphGenerator(int n)
        {
            SizeOfGraph = n;
            streamReader = new StreamReader(
                "C:/Users/filip/source/repos/generating-simple-drawings-of-graphs/VizualizerWPF/data/graph" + SizeOfGraph + ".txt");
        }

        public void CloseFile()
        {
            streamReader.Dispose(); //delete if we want to remain state of file
        }

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

        GraphCoordinates ReadUntillNextRS()
        {
            graphCoordinates = new GraphCoordinates();
            List<Vertex> vertices = new List<Vertex>();

            string line;
            while(!String.Equals((line = streamReader.ReadLine()), "#") && line != null)
            {
                string[] temp = line.Split();

                var edge = new Edge(new HashSet<Point>(), new List<Line>());

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

                graphCoordinates.AddToDictionary(vertices[0], edge);
                graphCoordinates.AddToDictionary(vertices[vertices.Count - 1], edge);
                
            }

            graphCoordinates.vertices = vertices;

            return graphCoordinates;
        }
        public GraphCoordinates GetTopicalDrawing()
        {
            return graphCoordinates;
        }

        public GraphCoordinates GenerateNextDrawing()
        {
            return ReadUntillNextRS();
        }
    }
}
