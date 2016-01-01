using System;
using System.Collections.Generic;
using System.Linq;
using KnapsackProblem.Common;

namespace KnapsackProblem.DynamicPrgVsFPTAS
{
    class Program
    {
        /* args:
        [0] - a path to directory with testing files
        [1] - how many files to use for testing
        [2] - how many times repeat a single file
        [3] - whether to save results
        [4] - fptas - dynamic programming with aproximation
        [5] - if fptas is used, what error to count with        
        */
        static void Main(string[] args)
        {
            var testFiles = Helpers.LoadTestFiles(args);
            if (testFiles == null)
            {
                Console.ReadLine();
                return;
            }

            var knapsackSets = testFiles.Select(f => KnapsackLoader.LoadKnapsacks(f, KnapsackLoader.KnapsackPerFile));

            bool useAprox = args.Length > 4 && args[4].Trim() == "fptas";
            if (useAprox)
            {
                double aproxError = double.Parse(args[5]);
                knapsackSets = knapsackSets.Select(set => set.Select(knapsack => knapsack.WithPriceFPTAS(aproxError)));
            }

            int repeatFile = int.Parse(args[2]);

            bool saveResults = bool.Parse(args[3]);
            if (saveResults)
            {
                var solutions = new List<KnapsackSolution>();
                Helpers.Benchmark(KnapsackProblemSolver.DynamicProgrammingByPrice, repeatFile, KnapsackLoader.KnapsackPerFile, knapsackSets, solutions);
                Helpers.SaveSolutions(solutions);
            }
            else
            {
                Helpers.Benchmark(KnapsackProblemSolver.DynamicProgrammingByPrice, repeatFile, KnapsackLoader.KnapsackPerFile, knapsackSets);
            }   

            Console.ReadLine();
        }
    }
}
