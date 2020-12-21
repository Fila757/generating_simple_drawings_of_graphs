using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace VizualizerWPF
{
    public static class EdgeListExtensions
    {
        public static Edge ContainsEnd(this List<Edge> list, Point point)
        {
            foreach (var el in list)
            {
                var last = CollisionDetection.ChooseOppositeOne(el, point);
                if (Vertex.Compare(last, point))
                    return el;
            }
            return null;
        }

        public static List<Point> GetEnds(this List<Edge> list, Point point){

            var result = new List<Point>();
            foreach(var el in list)
            {
                result.Add(CollisionDetection.ChooseOppositeOne(el, point));
            }
            return result;
        } 
    }

}
