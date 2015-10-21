using System;
using System.Linq;

namespace KnapsackProblem.Common
{
    public static class KnapsackProblemSolver
    {
        public static Tuple<uint, ulong> BruteForce(Knapsack knapsack)
        {
            var caseCount = Math.Pow(2, knapsack.InstanceSize); // total number of possible permutations
            ulong bestCase = 0;
            uint bestPrice = 0;

            // try all permutations
            for (ulong currentCase = 1; currentCase <= caseCount; currentCase++)
            {
                uint permutationPrice = 0, permutationWeight = 0;
                for (int i = 0; i < knapsack.Items.Count; i++)
                {
                    if ((currentCase & (ulong)(1 << i)) == 0)
                        continue;

                    var currentItem = knapsack.Items[i];
                    permutationPrice += currentItem.Price;
                    permutationWeight += currentItem.Weight;

                    if (permutationWeight > knapsack.Capacity)
                        break;
                }

                if (permutationWeight > knapsack.Capacity)
                    continue;

                if (permutationPrice > bestPrice)
                {
                    bestPrice = permutationPrice;
                    bestCase = currentCase;
                }
            }

            return Tuple.Create(bestPrice, bestCase);
        }

        public static Tuple<uint, ulong> PriceToWeightRatioHeuristics(Knapsack knapsack)
        {
            var itemsWithRatios = knapsack.Items.Select((item, index) => new { Item = item, Index = index, Ratio = item.Price / item.Weight })
                                                .OrderByDescending(i => i.Ratio);

            ulong currentCase = 0;
            uint currentPrice = 0, currentWeight = 0;

            foreach (var pack in itemsWithRatios)
            {
                if (currentWeight + pack.Item.Weight > knapsack.Capacity)
                    continue;

                currentPrice += pack.Item.Price;
                currentWeight += pack.Item.Weight;
                currentCase |= (ulong)(1 << pack.Index);

                if (currentWeight == knapsack.Capacity)
                    break;
            }

            return Tuple.Create(currentPrice, currentCase);
        }
    }
}