using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartHomeCore.Utils
{
    public static class FunctionalExtensions
    {
        public static IEnumerable<T> Unpack<T>(this IEnumerable<(bool, T)> collection) =>
            collection.Where(p => p.Item1).Select(p => p.Item2);
    }
}
