using System;
using System.Collections;
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

        private static void PrepareBenchmark(Func<Knapsack, Tuple<int, BitArray>> knapsackSolver, IEnumerable<Knapsack> testKnapsacks)
        {
            Console.WriteLine("Warming up ...");
            foreach (var knapsack in testKnapsacks)
            {
                var result = knapsackSolver(knapsack);
                Console.WriteLine(result.Item1); // so that it's not optimized away
            }
            Console.Clear();

            Process.GetCurrentProcess().ProcessorAffinity = new IntPtr(2); // Use only the second core 
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            Thread.CurrentThread.Priority = ThreadPriority.Highest;

            Console.WriteLine("Warm up finished. Starting test ...");
        }

        public static void Benchmark(Func<Knapsack, Tuple<int, BitArray>> knapsackSolver, int repeatEachSet, int loadKnapsacksAmount, IEnumerable<IEnumerable<Knapsack>> knapsackSets)
        {
            PrepareBenchmark(knapsackSolver, knapsackSets.First());

            var bfWatch = new Stopwatch();

            foreach (var knapsackSet in knapsackSets)
            {
                var knapsacks = knapsackSet.ToList();

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                for (int i = 0; i < repeatEachSet; i++)
                {
                    foreach (var knapsack in knapsacks)
                    {
                        bfWatch.Start();
                        knapsackSolver(knapsack);
                        bfWatch.Stop();
                    }
                }

                double instanceAverage = bfWatch.ElapsedMilliseconds / ((double)loadKnapsacksAmount * repeatEachSet);
                Console.WriteLine($"{knapsacks.First().InstanceSize}; {instanceAverage}ms");
                bfWatch.Reset();
            }

            Console.WriteLine("Testing finished.");
        }

        /// <summary>
        /// Returns a specific binary format of a ulong value.
        /// </summary>
        public static string ToReverseBinary(this ulong value, int binaryDigitsCount)
        {
            var sb = new StringBuilder();
            int binaryDigits = 0;
            while (value != 0)
            {
                sb.Append((value & 1) == 1 ? " 1" : " 0");
                value >>= 1;
                binaryDigits++;
            }
            
            while (binaryDigits < binaryDigitsCount)
            {
                sb.Append(" 0");
                binaryDigits++;
            }

            return sb.ToString();
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
    }
}