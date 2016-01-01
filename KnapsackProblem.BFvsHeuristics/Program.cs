using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KnapsackProblem.Common;

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
            var testFiles = Helpers.LoadTestFiles(args);
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
                Helpers.Benchmark(solver, repeatFile, KnapsackLoader.KnapsackPerFile, knapsackSets, solutions);
                Helpers.SaveSolutions(solutions);
            }
            else
            {
                Helpers.Benchmark(solver, repeatFile, KnapsackLoader.KnapsackPerFile, knapsackSets);
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
                var file = knapsacksFiles[i];
                var solutionFile = knapsacksFiles[i + 1];
                var bestPrices = File.ReadAllLines(solutionFile).Select(line => double.Parse(line.Split()[2])).ToList();

                var knapsacks = KnapsackLoader.LoadKnapsacks(file, 500).ToList();
                for (int j = 0; j < knapsacks.Count; j++)
                {
                    var result = solveKnapsack(knapsacks[j]);
                    relativeErrors.Add((bestPrices[j] - result.BestPrice) / bestPrices[j]);
                }
            }

            Console.WriteLine($"Average relative error from {relativeErrors.Count} different errors is {relativeErrors.Average()}.");
            Console.WriteLine("Max relative error is {0}.", relativeErrors.Max());
        }
    }
}
