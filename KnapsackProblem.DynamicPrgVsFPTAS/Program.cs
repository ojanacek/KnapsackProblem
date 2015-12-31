using System;
using System.Linq;
using KnapsackProblem.Common;

namespace KnapsackProblem.DynamicPrgVsFPTAS
{
    class Program
    {
        /* args:
        [0] - a path to directory with testing files
        [1] - how many files to use for testing
        [2] - how many times repeat a single file, default is 1
        [3] - fptas - dynamic programming with aproximation
        [4] - if fptas is used, what error to count with
        */
        static void Main(string[] args)
        {
            var testFiles = Helpers.LoadTestFiles(args);
            if (testFiles == null)
            {
                Console.ReadLine();
                return;
            }

            var knapsackSets = testFiles.Select(f => KnapsackLoader.LoadKnapsacks(f, 500));
            var maxPrice = knapsackSets.SelectMany(s => s).SelectMany(k => k.Items).Max(i => i.Price);

            bool useAprox = args.Length > 3 && args[3].Trim() == "fptas";
            if (useAprox)
            {
                double aproxError = double.Parse(args[4]);
                knapsackSets = knapsackSets.Select(set => set.Select(knapsack => knapsack.WithPriceFPTAS(aproxError)));
            }

            int repeatFile = int.Parse(args[2]);
            Helpers.Benchmark(KnapsackProblemSolver.DynamicProgrammingByPrice, repeatFile, 500, knapsackSets);

            Console.ReadLine();
        }
    }
}
