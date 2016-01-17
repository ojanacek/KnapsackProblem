using System;
using System.IO;
using System.Linq;
using System.Text;
using KnapsackProblem.Common;
using static KnapsackProblem.Common.ArgumentHelpers;

namespace KnapsackProblem.KnapsackGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var generatorArgs = ParseArguments(args);
            if (generatorArgs == null)
            {
                Console.ReadKey();
                return;
            }

            var weights = new int[generatorArgs.ItemsCount];
            var costs = new int[generatorArgs.ItemsCount];

            double overallTotalWeight = 0;
            var sb = new StringBuilder();
            for (int i = 0; i < generatorArgs.InstancesCount; i++)
            {
                int totalWeight = Generator.GenerateInstance(generatorArgs, weights, costs);
                overallTotalWeight += totalWeight;
                sb.Append($"{generatorArgs.InitId + i} {generatorArgs.ItemsCount} {(int)(generatorArgs.Ratio * totalWeight)}");
                sb.AppendLine(string.Concat(weights.Zip(costs, (w, c) => $" {w} {c}")));
                Console.WriteLine("total weight " + totalWeight);
            }

            File.WriteAllText($"knap_{generatorArgs.ItemsCount.ToString().PadLeft(3, '0')}.dat", sb.ToString());
            Console.WriteLine($"average total weight {overallTotalWeight / generatorArgs.InstancesCount}; first unused instance id {generatorArgs.InitId + generatorArgs.InstancesCount}");
            Console.ReadKey();
        }

        static GeneratorArgs ParseArguments(string[] args)
        {
            if (args.Length == 0 || args[0] == "-?" || args[0] == "/?")
            {
                PrintHelp();
                return null;
            }

            try
            {
                int initId = ParseInt32Option(args, "I", false, short.MinValue, short.MaxValue, 0);
                int itemsCount = ParseInt32Option(args, "n", true, 0, ushort.MaxValue);
                int instancesCount = ParseInt32Option(args, "N", true, 0, ushort.MaxValue);
                double ratio = ParseDoubleOption(args, "m", true, 0, 1);
                int maxWeight = ParseInt32Option(args, "W", true, 0, ushort.MaxValue);
                int maxCost = ParseInt32Option(args, "C", true, 0, ushort.MaxValue);
                double exponent = ParseDoubleOption(args, "k", true, 0, 1);
                int balance = ParseInt32Option(args, "d", true, -1, 1);
                return new GeneratorArgs(initId, itemsCount, instancesCount, ratio, maxWeight, maxCost, exponent, balance);
            }
            catch { return null; }
        }

        static void PrintHelp()
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("Usage: [-I id] -n count -N count -m ratio");
            sb.AppendLine("       -W weight -C cost -k exponent -d balance");
            sb.AppendLine();
            sb.AppendLine("Options:");
            sb.AppendOption("[-I id]", "Initial instance ID, defaults to 0.");
            sb.AppendOption("-n count", "Total # of items per each instance.");
            sb.AppendOption("-N count", "Total # of instances per file.");
            sb.AppendOption("-m ratio", "The ratio of max knapsack capacity to total weight.");
            sb.AppendOption("-W weight", "Max item weight.");
            sb.AppendOption("-C cost", "Max item cost.");
            sb.AppendOption("-k exponent", "exponent k (real)");
            sb.AppendOption("-d balance", "-1 .. more small items, 1 .. more large items, 0 .. balance");
            Console.WriteLine(sb.ToString());
        }
    }
}
