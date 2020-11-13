using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropHunt.Shared.Extensions
{
    public static class IEnumerableExt
    {
        public static T GetRandom<T>(this IEnumerable<T> list)
        {
            if (list == null)
                return default;

            Random random = new Random(Guid.NewGuid().GetHashCode());
            return list.ElementAt(random.Next(0, list.Count()));
        }
    }
}
