using System.Collections.Generic;
using System.Windows;

namespace VizualizerWPF
{
    /// <summary>
    ///     Extension class on list of Edges
    /// </summary>
    public static class EdgeListExtensions
    {
        /// <summary>
        ///     Function to get the edge contained in <c>list</c> which is incident to both <c>oppositeTo</c> and <c>point</c>.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="oppositeTo"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Edge ContainsEnd(this List<Edge> list, Point oppositeTo, Point point)
        {
            foreach (var el in list)
            {
                var last = CollisionDetection.ChooseOppositeOne(el, oppositeTo);
                if (Vertex.Compare(last, point))
                    return el;
            }

            return null;
        }

        /// <summary>
        ///     Function to get all other ends, compare to <c>point</c>, of edges in <c>list</c>.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static List<Point> GetEnds(this List<Edge> list, Point point)
        {
            var result = new List<Point>();
            foreach (var el in list) result.Add(CollisionDetection.ChooseOppositeOne(el, point));
            return result;
        }
    }
}