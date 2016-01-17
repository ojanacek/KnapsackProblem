using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace KnapsackProblem.Common
{
    public static class Helpers
    {
        public static List<string> LoadTestFiles(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Specify path to test files and how many files to use for testing.");
                return null;
            }

            var directoryPath = args[0];
            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine("Directory '{0}' does not exist.", directoryPath);
                return null;
            }

            return Directory.EnumerateFiles(directoryPath, "knap_*.dat")
                            .Where(f => !f.Contains("sol"))
                            .Take(int.Parse(args[1]))
                            .ToList();
        } 

        private static void PrepareBenchmark(Func<Knapsack, KnapsackSolution> solveKnapsack, IEnumerable<Knapsack> testKnapsacks)
        {
            Console.WriteLine("Warming up ...");
            foreach (var knapsack in testKnapsacks)
            {
                var result = solveKnapsack(knapsack);
                Console.WriteLine(result.BestPrice); // so that it's not optimized away
            }
            Console.Clear();

            Process.GetCurrentProcess().ProcessorAffinity = new IntPtr(2); // Use only the second core 
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            Thread.CurrentThread.Priority = ThreadPriority.Highest;

            Console.WriteLine($"Warm up finished. Starting {solveKnapsack.Method.Name} test ...");
        }

        public static void Benchmark(Func<Knapsack, KnapsackSolution> solveKnapsack, int repeatEachSet, int testAmount, IEnumerable<IEnumerable<Knapsack>> knapsackSets, List<KnapsackSolution> solutions = null)
        {
            var sets = knapsackSets.ToList();
            if (sets.Count == 0)
            {
                Console.WriteLine("No test file was found. Check correct naming and extension.");
                return;
            }

            PrepareBenchmark(solveKnapsack, sets.First());

            var bfWatch = new Stopwatch();

            foreach (var knapsackSet in sets)
            {
                var knapsacks = knapsackSet.Take(testAmount).ToList();

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                for (int i = 0; i < repeatEachSet; i++)
                {
                    foreach (var knapsack in knapsacks)
                    {
                        bfWatch.Start();
                        var solution = solveKnapsack(knapsack);
                        bfWatch.Stop();
                        solutions?.Add(solution);
                    }
                }

                double instanceAverage = bfWatch.ElapsedMilliseconds / ((double)testAmount * repeatEachSet);
                Console.WriteLine($"{knapsacks.First().InstanceSize}; {instanceAverage}ms");
                bfWatch.Reset();
            }

            Console.WriteLine("Testing finished.");
        }

        public static Knapsack WithPriceFPTAS(this Knapsack knapsack, double error)
        {
            int totalPrice = knapsack.Items.Sum(i => i.Price);
            int shiftBits = (int) Math.Log((error * totalPrice) / Math.Pow(knapsack.InstanceSize, 2), 2);
            if (shiftBits < 1)
                return knapsack;

            return new Knapsack(knapsack.Id, knapsack.Capacity, 
                knapsack.Items.Select(i => new KnapsackItem(i.Weight, (ushort)(i.Price >> shiftBits))));
        }

        public static void SaveSolutions(List<KnapsackSolution> solutions)
        {
            for (int i = 0; i < solutions.Count; i += KnapsackLoader.KnapsackPerFile)
            {
                File.WriteAllLines($"solutions{solutions[i].Knapsack.InstanceSize}.txt", solutions.Skip(i).Take(KnapsackLoader.KnapsackPerFile).Select(s => s.ToString()));
            }
        }

        public static int[] LoadBestPricesFromSolutionFile(string filePath)
        {
            return File.ReadAllLines(filePath)
                       .Select(line => int.Parse(line.Split()[2]))
                       .ToArray();
        }

        public static double ComputeRelativeError(int bestPrice, int price) => (double)(bestPrice - price) / bestPrice;
    }
}