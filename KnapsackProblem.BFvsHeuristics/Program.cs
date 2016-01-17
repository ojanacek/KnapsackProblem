using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KnapsackProblem.Common;
using static KnapsackProblem.Common.Helpers;

namespace KnapsackProblem.BFvsHeuristics
{
    class Program
    {
        /* args:
        [0] - a path to directory with testing files
        [1] - how many files to use for testing
        [2] - how many times repeat a single file
        [3] - bf / sbf / heur
        [4] - whether to save results
        */
        static void Main(string[] args)
        {
            var testFiles = LoadTestFiles(args);
            if (testFiles == null)
            {
                Console.ReadLine();
                return;
            }

            int repeatFile = int.Parse(args[2]);
            var knapsackSets = testFiles.Select(f => KnapsackLoader.LoadKnapsacks(f, KnapsackLoader.KnapsackPerFile));
            
            Func<Knapsack, KnapsackSolution> solver;
            var method = args[3].Trim().ToLower();
            if (method == "bf")
                solver = KnapsackProblemSolver.BruteForce;
            else if (method == "sbf")
                solver = KnapsackProblemSolver.SmarterBruteForce;
            else
                solver = KnapsackProblemSolver.PriceToWeightRatioHeuristics;

            bool saveResults = bool.Parse(args[4]);
            if (saveResults)
            {
                var solutions = new List<KnapsackSolution>();
                Benchmark(solver, repeatFile, KnapsackLoader.KnapsackPerFile, knapsackSets, solutions);
                SaveSolutions(solutions);
            }
            else
            {
                Benchmark(solver, repeatFile, KnapsackLoader.KnapsackPerFile, knapsackSets);
            }

            Console.ReadLine();
        }

        static void MeasureRelativeErrors(Func<Knapsack, KnapsackSolution> solveKnapsack, string directoryPath)
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
                    var result = solveKnapsack(knapsacks[j]);
                    relativeErrors.Add(ComputeRelativeError(bestPrices[j], result.BestPrice));
                }
            }

            Console.WriteLine($"Average relative error is {relativeErrors.Average()}.");
            Console.WriteLine($"Max relative error is {relativeErrors.Max()}.");
        }
    }
}
