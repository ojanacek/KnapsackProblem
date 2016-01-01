using System.Collections;
using System.Linq;

namespace KnapsackProblem.Common
{
    public sealed class KnapsackSolution
    {
        public int BestPrice { get; }
        public BitArray Vector { get; }
        public Knapsack Knapsack { get; }

        public KnapsackSolution(int bestPrice, BitArray vector, Knapsack knapsack)
        {
            BestPrice = bestPrice;
            Vector = vector;
            Knapsack = knapsack;
        }

        public KnapsackSolution(int bestPrice, ulong vector, Knapsack knapsack)
        {
            BestPrice = bestPrice;
            Knapsack = knapsack;

            Vector = new BitArray(knapsack.InstanceSize);
            for (int i = 0; i < knapsack.InstanceSize; i++)
            {
                var bit = (ulong)(1 << i);
                Vector.Set(i, (vector & bit) == bit);
            }
        }

        public override string ToString()
        {
            return $"{Knapsack.Id} {Knapsack.InstanceSize} {BestPrice} {string.Join(" ", Vector.Cast<bool>().Select(b => b ? "1" : "0"))}";
        }
    }
}