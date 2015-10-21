using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KnapsackProblem.Common
{
    public static class KnapsackLoader
    {
        public static IEnumerable<Knapsack> LoadKnapsacks(string path, int maxKnapsacks)
        {
            return File.ReadLines(path).Take(maxKnapsacks).Select(ParseKnapsack);
        }

        private static Knapsack ParseKnapsack(string line)
        {
            var parts = line.Split();
            return new Knapsack(short.Parse(parts[0]), short.Parse(parts[2]), ParseKnapsackItems(parts.Skip(3)));
        }

        private static IEnumerable<KnapsackItem> ParseKnapsackItems(IEnumerable<string> enumerable)
        {
            var enumerator = enumerable.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var weight = ushort.Parse(enumerator.Current);
                enumerator.MoveNext();
                var price = ushort.Parse(enumerator.Current);
                yield return new KnapsackItem(weight, price);
            }
        }
    }
}