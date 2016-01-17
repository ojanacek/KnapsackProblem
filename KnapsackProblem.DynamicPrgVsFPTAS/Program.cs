using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KnapsackProblem.Common;
using static KnapsackProblem.Common.Helpers;

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
            var testFiles = LoadTestFiles(args);
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
                Benchmark(KnapsackProblemSolver.DynamicProgrammingByPrice, repeatFile, KnapsackLoader.KnapsackPerFile, knapsackSets, solutions);
                SaveSolutions(solutions);
            }
            else
            {
                Benchmark(KnapsackProblemSolver.DynamicProgrammingByPrice, repeatFile, KnapsackLoader.KnapsackPerFile, knapsackSets);
            }
            
            Console.ReadLine();
        }

        static void CheckAproximationErrors(string directoryPath, double error)
        {
            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine("Directory '{0}' does not exist.", directoryPath);
                return;
            }

            var knapsacksFiles = Directory.EnumerateFiles(directoryPath, "knap_*.dat").ToList();
            var relativeErrors = new List<double>();

            for (int i = 0; i < knapsacksFiles.Count; i += 2)
            {
                var bestPrices = LoadBestPricesFromSolutionFile(knapsacksFiles[i + 1]);
                var knapsacks = KnapsackLoader.LoadKnapsacks(knapsacksFiles[i], KnapsackLoader.KnapsackPerFile).ToList();

                for (int j = 0; j < knapsacks.Count; j++)
                {
                    var result = KnapsackProblemSolver.DynamicProgrammingByPrice(knapsacks[j].WithPriceFPTAS(error));
                    int realBestPrice = 0;
                    for (int k = 0; k < result.Vector.Length; k++)
                    {
                        if (result.Vector[k])
                            realBestPrice += knapsacks[j].Items[k].Price;
                    }
                    
                    relativeErrors.Add(ComputeRelativeError(bestPrices[j], realBestPrice));
                }
            }

            Console.WriteLine();
            Console.WriteLine($"For chosen aproximation error {error}: ");
            Console.WriteLine($"  - average relative error is {relativeErrors.Average()}.");
            Console.WriteLine($"  - max relative error is {relativeErrors.Max()}.");
        }
    }
}
