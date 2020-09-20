using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Shapes;

namespace VizualizerWPF
{
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

        GraphCoordinates ReadUntillNextRS()
        {
            graphCoordinates = new GraphCoordinates();
            HashSet<Tuple<Point, VertexState> > vertices = new HashSet<Tuple<Point, VertexState> >();

            string line;
            while(!String.Equals((line = streamReader.ReadLine()), "#") && line != null)
            {
                string[] temp = line.Split();

                var edge = new Edge(new HashSet<Point>(), new List<Line>());
                for (int i = 0; i < temp.Length / 4; i++)
                {
                    var stateFrom = i == 0 ? VertexState.Regular : VertexState.Intersection;
                    var stateTo = i == temp.Length / 4 - 1 ? VertexState.Regular : VertexState.Intersection;

                    var from = new Point { X = Double.Parse(temp[4 * i + 0]), Y = Double.Parse(temp[4 * i + 1]) };
                    var to = new Point { X = Double.Parse(temp[4 * i + 2]), Y = Double.Parse(temp[4 * i + 3]) };
                        
                    vertices.Add(Tuple.Create(from, stateFrom));
                    vertices.Add(Tuple.Create(to, stateTo));

                    edge.lines.Add(new Line
                    {
                        X1 = from.X,
                        X2 = to.X,
                        Y1 = from.Y,
                        Y2 = to.Y,
                    }); ;

                    edge.points.Add(from); edge.points.Add(to);
                }
                graphCoordinates.edges.Add(edge);
                
            }

            graphCoordinates.vertices = vertices.ToList();

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
