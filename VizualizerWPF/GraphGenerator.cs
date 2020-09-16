using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

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

                for (int i = 0; i < temp.Length / 4; i++)
                {
                    var stateFrom = i == 0 ? VertexState.Regular : VertexState.Intersection;
                    var stateTo = i == temp.Length / 4 - 1 ? VertexState.Regular : VertexState.Intersection;
                    graphCoordinates.edges.Add(
                        new Edge
                        {
                            from = new Coordinates { x = Double.Parse(temp[4*i + 0]), y = Double.Parse(temp[4*i + 1]) },
                            to = new Coordinates { x = Double.Parse(temp[4*i + 2]), y = Double.Parse(temp[4*i + 3]) }
                        });
                    vertices.Add(Tuple.Create(graphCoordinates.edges[graphCoordinates.edges.Count - 1].from, stateFrom));
                    vertices.Add(Tuple.Create(graphCoordinates.edges[graphCoordinates.edges.Count - 1].to, stateTo));
                }
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
