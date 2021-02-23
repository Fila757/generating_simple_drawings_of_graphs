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
    public class GraphCoordinates
    {
        
        public HashSet<Vertex> vertices = new HashSet<Vertex>();
        public List<Edge> edges = new List<Edge>();
        public Dictionary<Vertex, List<Edge>> neighbors = new Dictionary<Vertex, List<Edge>>();

        /// <summary>
        /// Safe version of adding to dictionary 
        /// if value is not present, new list is created
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddToDictionary(Vertex key, Edge value)
        {
            if (neighbors.ContainsKey(key))
            {
                neighbors[key].Add(value);
            }
            else
            {
                neighbors[key] = new List<Edge>();
                neighbors[key].Add(value);
            }
        }

        public Vertex FindVertex(Point center)
        {
            foreach (var vertex in vertices)
            {
                if (Vertex.Compare(vertex.center, center))
                    return vertex;
            }


            MessageBox.Show("BUG");
            return new Vertex();
            throw new ArgumentException("There is no such a vertex with that center");
        }

        public void SaveCoordinates()
        {
            var streamWriter = new StreamWriter(
                "C:/Users/filip/source/repos/generating-simple-drawings-of-graphs/VizualizerWPF/data/savedGraphs.txt",
                append: true);

            Dictionary<(Vertex, Vertex), bool> visited = new Dictionary<(Vertex, Vertex), bool>();
            foreach (var from in neighbors.Keys)
            {
                foreach (var to in neighbors.Keys)
                {
                    visited[(from, to)] = false;
                }
            }

            foreach (var v in neighbors.Keys)
            {
                foreach(var e in neighbors[v])
                {
                    var oppositeVertex = FindVertex(CollisionDetection.ChooseOppositeOne(e, v.center));
                    if (visited[(v, oppositeVertex)])
                        continue;
                    visited[(v, oppositeVertex)] = true; visited[(oppositeVertex, v)] = true;

                    PrintLine(streamWriter, e);

                }
            }

            streamWriter.WriteLine("#"); // end
            streamWriter.Close();
        }

        void PrintLine(StreamWriter streamWriter, Edge edge)
        {
            streamWriter.Write("( ");
            for(int i = 0; i < edge.points.Count; i++)
            {
                streamWriter.Write($"{edge.points[i].X} {edge.points[i].Y} ");
                if (FindVertex(edge.points[i]).state == VertexState.Intersection)
                {
                    streamWriter.Write(") ( ");
                    streamWriter.Write($"{edge.points[i].X} {edge.points[i].Y} ");
                }
            }
            streamWriter.WriteLine(")");
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

        public int counter = 0;

        /// <summary>
        /// it supposed to run from in it Visual Studio from bin/Debug(Release)/netcoreapp3.1/
        /// </summary>
        /// <param name="n">Size of wanted clique</param>
        public GraphGenerator(int n)
        {
            SizeOfGraph = n;
            streamReader = new StreamReader(@"data/graph" + SizeOfGraph + ".txt");
        }

        public GraphGenerator()
        {
            streamReader = new StreamReader("C:/Users/filip/source/repos/generating-simple-drawings-of-graphs/VizualizerWPF/data/savedGraphs.txt");
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
        List<double []> SortFromStartToEnd(List<string []> line)
        {
            List<double[] > doubleLine = new List<double[]>();

            int FindNextLine(Point a, int cur)
            {
                for(int i = 0; i < line.Count; i++)
                {
                    //if (i != cur)
                    //{
                    var from = new Point { X = Double.Parse(line[i].ElementAt(0)), Y = Double.Parse(line[i].ElementAt(1)) };
                    if (from == a)
                        return i;
                    //}
                }

                return -1;
            }

            int FindPreviousLine(Point a)
            {
                for (int i = 0; i < line.Count; i++)
                {
                    var to = new Point { X = Double.Parse(line[i].ElementAt(line[i].Length - 2)), Y = Double.Parse(line[i].ElementAt(line[i].Length - 1)) };
                    if (to == a)
                        return i;
                }

                return -1;
            }

            int FindFirst()
            {
                for (int i = 0; i < line.Count; i++)
                {
                    var from = new Point { X = Double.Parse(line[i].ElementAt(0)), Y = Double.Parse(line[i].ElementAt(1))};

                    if (FindPreviousLine(from) == -1)
                        return i;
                }

                throw new ArgumentException("there is no first line");
            }

            int FindLast()
            {
                for (int i = 0; i < line.Count; i++)
                {
                    var to = new Point { X = Double.Parse(line[i].ElementAt(line[i].Length - 2)), Y = Double.Parse(line[i].ElementAt(line[i].Length - 1)) };
                    if (FindNextLine(to, -1) == -1)
                        return i;
                }

                throw new ArgumentException("there is no last line");
            }

            int first = FindFirst();
            int last = FindLast();

            double[] temp;
            int cur = first;
            while(cur != last)
            {
                temp = new double[line[cur].Length];
                for (int i = 0; i < line[cur].Length; i++)
                    temp[i] = Double.Parse(line[cur][i]);
                doubleLine.Add(temp);

                cur = FindNextLine(new Point { X = doubleLine.Last().ElementAt(doubleLine.Last().Length - 2), Y = doubleLine.Last().ElementAt(doubleLine.Last().Length - 1)}, cur);
            }
            temp = new double[line[cur].Length];
            for (int i = 0; i < line[cur].Length; i++)
                temp[i] = Double.Parse(line[cur][i]);
            doubleLine.Add(temp);

            return doubleLine;
        }

        /// <summary>
        /// Read whole points and lines of cliques
        /// and generate graph out of it
        /// </summary>
        /// <returns>New graph</returns>
        GraphCoordinates ReadUntillNextRS(bool skip = false)
        {
            graphCoordinates = new GraphCoordinates();

            string line;
            while (!String.Equals((line = streamReader.ReadLine()), "#") && line != null)
            {
                if (line == "" || skip)
                    continue;

                List<Vertex> vertices = new List<Vertex>();

                string[] temp = line.Split(new char[] {'(', ')'}, StringSplitOptions.RemoveEmptyEntries);

                temp = temp.Where(x => x != " ").ToArray();

                List<string[]> tempList = new List<string[]>();
                foreach (var t in temp)
                    tempList.Add(t.Split(new char[0], options : StringSplitOptions.RemoveEmptyEntries));

                var edge = new Edge(new List<Point>(), new List<Line>());

                List<double[]> sortedLine = SortFromStartToEnd(tempList);

                for (int i = 0; i < sortedLine.Count; i++)
                {
                    var stateFrom = i == 0 ? VertexState.Regular : VertexState.Intersection;
                    var stateTo = i == sortedLine.Count - 1 ? VertexState.Regular : VertexState.Intersection;

                    List<Point> points = new List<Point>();

                    for (int j = 0; j < sortedLine[i].Length; j += 2)
                    {
                        points.Add(new Point { X = sortedLine[i][j], Y = sortedLine[i][j + 1] });
                    }

                    //add just every second . . _ . _ . _ . _ ., . _ is always the same
                    if (i == 0)
                    {
                        vertices.Add(new Vertex(new Ellipse(), points.First(), stateFrom));
                        edge.points.Add(points.First());

                        for (int j = 1; j < points.Count - 1; j++)
                        {
                            vertices.Add(new Vertex(new Ellipse(), points[j], VertexState.Middle));
                            edge.points.Add(points[j]);
                        }

                        vertices.Add(new Vertex(new Ellipse(), points.Last(), stateTo));
                        edge.points.Add(points.Last());

                    }
                    else
                    {
                        for (int j = 1; j < points.Count - 1; j++)
                        {
                            vertices.Add(new Vertex(new Ellipse(), points[j], VertexState.Middle));
                            edge.points.Add(points[j]);
                        }

                        vertices.Add(new Vertex(new Ellipse(), points.Last(), stateTo));
                        edge.points.Add(points.Last());
                    }

                    //foreach (var point in points)
                    //    edge.points.Add(point);


                    for (int j = 0; j < points.Count - 1; j++)
                    {
                        edge.lines.Add(new Line
                        {
                            X1 = points[j].X,
                            X2 = points[j + 1].X,
                            Y1 = points[j].Y,
                            Y2 = points[j + 1].Y,
                        });
                    }
                }

                graphCoordinates.edges.Add(edge);

                
                Vertex actualZero = vertices[0]; Vertex actualLast = vertices[vertices.Count - 1];
                
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

        /*
        private GraphCoordinates ReadUntillPreviousRS()
        {
            int counterOfSharps = 0;
            while(counterOfSharps < 2 && streamReader.BaseStream.Position > 0)
            {
                streamReader.BaseStream.Position = streamReader.BaseStream.Seek(-1, SeekOrigin.Current);
                if (streamReader.Peek() >= 0 && Convert.ToChar(streamReader.Peek()) == '#')
                    counterOfSharps++;
            }
            
            return ReadUntillNextRS();
        }
        */

        /// <summary>
        /// Get next drawing
        /// </summary>
        /// <returns>New graph</returns>
        public GraphCoordinates GenerateNextDrawing()
        {
            counter++; 

            return ReadUntillNextRS();
        }

        
        public GraphCoordinates GeneratePreviousDrawing()
        {
            counter--;

            int tmpCounter = 1;
            while (tmpCounter < counter)
            {
                ReadUntillNextRS(true);
                tmpCounter++;
            }
            return ReadUntillNextRS();
        }
        
    }
}
