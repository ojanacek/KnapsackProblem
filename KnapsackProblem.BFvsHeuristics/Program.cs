using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using KnapsackProblem.Common;

namespace KnapsackProblem.BFvsHeuristics
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No path provided.");
                return;
            }

            MeasureRelativeErrors(args[0]);
        }

        /// <summary> Generate file with results to check against referece results. </summary>
        static void SaveResultsForCheck(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("File '{0}' does not exist.", filePath);
                return;
            }

            var sb = new StringBuilder();
            foreach (var knapsack in KnapsackLoader.LoadKnapsacks(filePath, 500))
            {
                //var result = KnapsackProblemSolver.BruteForce(knapsack);
                var result = KnapsackProblemSolver.PriceToWeightRatioHeuristics(knapsack);
                sb.AppendLine($"{knapsack.Id} {knapsack.InstanceSize} {result.Item1} {result.Item2.ToReverseBinary(knapsack.InstanceSize)}");
            }

            File.WriteAllText("result.txt", sb.ToString());
        }
        
        static void MeasureBFTimePerInstanceSize(string directoryPath, int knapsacksPerInstanceSize, TimeSpan maxTimePerInstance)
        {
            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine("Directory '{0}' does not exist.", directoryPath);
                return;
            }

            var knapsacksFiles = Directory.EnumerateFiles(directoryPath, "knap_*.dat").Where(f => !f.Contains("sol")).ToList();

            // warmup
            foreach (var knapsack in KnapsackLoader.LoadKnapsacks(knapsacksFiles.First(), 50))
            {
                var result = KnapsackProblemSolver.BruteForce(knapsack);
                Console.WriteLine(result); // so that it's not optimized away
            }
            GC.Collect();

            // measure
            var bfWatch = new Stopwatch();
            int originalKnapsacksPerInstanceSize = knapsacksPerInstanceSize;

            for (int i = 0; i < knapsacksFiles.Count; i++)
            {
                string file = knapsacksFiles[i];
                int instanceSize = 0;

                Console.WriteLine("Starting {0}", file);
                int j = 0;

                foreach (var knapsack in KnapsackLoader.LoadKnapsacks(file, knapsacksPerInstanceSize))
                {
                    instanceSize = knapsack.InstanceSize;
                    bfWatch.Start();
                    KnapsackProblemSolver.BruteForce(knapsack);
                    bfWatch.Stop();
                    Console.WriteLine("--finished {0}: {1}", j, bfWatch.ElapsedMilliseconds);
                }

                double instanceAverage = bfWatch.ElapsedMilliseconds / (double)knapsacksPerInstanceSize;
                if (Math.Abs(instanceAverage) < 0.0000001)
                {
                    i--;
                    knapsacksPerInstanceSize = 500;
                    continue;
                }

                File.Create($"{instanceSize};{instanceAverage}.txt");

                knapsacksPerInstanceSize = originalKnapsacksPerInstanceSize;
                bfWatch.Reset();
                GC.Collect();
                if (instanceAverage > maxTimePerInstance.TotalMilliseconds)
                    break;
            }
        }

        static void MeasureHeuristicsTimePerInstanceSize(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine("Directory '{0}' does not exist.", directoryPath);
                return;
            }

            var knapsacksFiles = Directory.EnumerateFiles(directoryPath, "knap_*.dat").Where(f => !f.Contains("sol")).ToList();

            // warmup
            foreach (var knapsack in KnapsackLoader.LoadKnapsacks(knapsacksFiles.First(), 500))
            {
                var result = KnapsackProblemSolver.PriceToWeightRatioHeuristics(knapsack);
                Console.WriteLine(result); // so that it's not optimized away
            }
            GC.Collect();

            // measure
            var bfWatch = new Stopwatch();
            var repeat = 50;

            foreach (string file in knapsacksFiles)
            {
                int instanceSize = 0;
                for (int i = 0; i < repeat; i++)
                {
                    foreach (var knapsack in KnapsackLoader.LoadKnapsacks(file, 500))
                    {
                        instanceSize = knapsack.InstanceSize;
                        bfWatch.Start();
                        KnapsackProblemSolver.PriceToWeightRatioHeuristics(knapsack);
                        bfWatch.Stop();
                    }
                }

                double instanceAverage = bfWatch.ElapsedMilliseconds / (500d * repeat);

                File.Create($"{instanceSize};{instanceAverage}.txt");
                
                bfWatch.Reset();
                GC.Collect();
            }
        }

        static void MeasureRelativeErrors(string directoryPath)
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
                    var result = KnapsackProblemSolver.PriceToWeightRatioHeuristics(knapsacks[j]);
                    relativeErrors.Add((bestPrices[j] - result.Item1) / bestPrices[j]);
                }
            }

            Console.WriteLine($"Average relative error from {relativeErrors.Count} different errors is {relativeErrors.Average()}.");
            Console.WriteLine("Max relative error is {0}.", relativeErrors.Max());
        }
    }
}
