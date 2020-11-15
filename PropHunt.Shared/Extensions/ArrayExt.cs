using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropHunt.Shared.Extensions
{
    public static class ArrayExt
    {
        public static T Random<T>(this Array array)
        {
            if (array == null)
                return default;

            Random random = new Random(Guid.NewGuid().GetHashCode());
            return (T)array.GetValue(random.Next(0, array.Length));
        }
    }
}
