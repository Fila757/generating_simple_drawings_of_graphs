using System.Collections.Generic;

namespace VizualizerWPF
{
    public static class HashSetExtesions
    {
        /// <summary>
        /// Hashset extesion to remain order of added elements 
        /// (UnionWith could be used but I didnt found implemenation)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hashset"></param>
        /// <param name="list"></param>
        public static void AddList<T>(this HashSet<T> hashset, List<T> list)
        {
            foreach(var l in list)
            {
                if (!hashset.Contains(l))
                    hashset.Add(l);
            }
        }
    }

}
