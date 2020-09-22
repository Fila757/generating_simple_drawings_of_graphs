using System.Collections.Generic;

namespace VizualizerWPF
{
    public static class HashSetExtesions
    {
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
