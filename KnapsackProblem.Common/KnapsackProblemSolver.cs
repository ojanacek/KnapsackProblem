﻿using System;
using System.Collections;
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

        public static Tuple<int, BitArray> DynamicProgrammingByPrice(Knapsack knapsack)
        {
            int totalPrice = knapsack.Items.Sum(i => i.Price);
            int rows = totalPrice + 1;
            int columns = knapsack.InstanceSize + 1;

            var weights = new uint[rows, columns];

            for (int n = 0; n < columns; n++)
            {
                for (int c = 0; c < rows; c++)
                {
                    // cost 0
                    if (c == 0) 
                        continue; // already 0

                    // no items
                    if (n == 0) 
                    {
                        weights[c, n] = uint.MaxValue;
                        continue;
                    }

                    var currentItem = knapsack.Items[n - 1];
                    if (n == 1)
                    {
                        weights[c, n] = c == currentItem.Price ? currentItem.Weight : uint.MaxValue;
                        continue;
                    }

                    // all other cells
                    var previousWeightForSamePrice = weights[c, n - 1];
                    int cIndex = c - currentItem.Price;
                    
                    if (cIndex < 0)
                        weights[c, n] = previousWeightForSamePrice == uint.MaxValue ? uint.MaxValue : previousWeightForSamePrice;
                    else
                    {
                        // the following code could be simplified when working with infinity instead of uint.MaxValue
                        if (weights[cIndex, n - 1] == uint.MaxValue)
                            weights[c, n] = Math.Min(previousWeightForSamePrice, weights[cIndex, n - 1]);
                        else
                            weights[c, n] = Math.Min(previousWeightForSamePrice, weights[cIndex, n - 1] + currentItem.Weight);
                    }
                }
            }

            // find the highest weight allowed
            int highestPrice = 0;
            int lastColumn = columns - 1;
            int row;
            for (row = totalPrice; row >= 0; row--)
            {
                if (weights[row, lastColumn] < knapsack.Capacity)
                {
                    highestPrice = row;
                    break;
                }
            }

            // create solution vector
            var vector = new BitArray(knapsack.InstanceSize);
            int column = knapsack.InstanceSize;
            while (row > 0)
            {
                if (weights[row, column] == weights[row, column - 1])
                {
                    column--;
                    continue;
                }

                row -= knapsack.Items[column - 1].Price;
                vector[--column] = true;
            }

            return Tuple.Create(highestPrice, vector);
        }
    }
}