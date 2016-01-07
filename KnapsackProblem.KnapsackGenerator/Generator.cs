using System;

namespace KnapsackProblem.KnapsackGenerator
{
    internal static class Generator
    {
        private static readonly Random random = new Random();

        public static int GenerateInstance(GeneratorArgs args, int[] weights, int[] costs)
        {
            var issuedWeights = new int[args.MaxWeight + 1];
            int i = 0, totalWeight = 0;

            while (i < args.ItemsCount)
            {
                int weight = random.Next(1, args.MaxWeight + 1);
                if (issuedWeights[weight] > 0)
                    continue; // already have an item with this weight

                if (AllowWeight(weight, args))
                {
                    costs[i] = issuedWeights[weight] = random.Next(1, args.MaxCost + 1);
                    weights[i] = weight;
                    totalWeight += weight;
                    i++;
                }
            }

            return totalWeight;
        }

        private static bool AllowWeight(int weight, GeneratorArgs args)
        {
            double threshold = 0;
            switch (args.Balance)
            {
                case 0: return true;
                case -1:
                    threshold = int.MaxValue / Math.Pow(weight, args.ExponentK);
                    break;
                case 1:
                    threshold = int.MaxValue / Math.Pow(args.MaxWeight - weight + 1, args.ExponentK);
                    break;
                default:
                    throw new ArgumentException("This should never happen.");
            }

            return threshold >= random.Next();
        }
    }
}