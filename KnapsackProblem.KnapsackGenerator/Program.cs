using System;
using System.IO;
using System.Linq;
using System.Text;

namespace KnapsackProblem.KnapsackGenerator
{
    delegate TResult ParseDelegate<in T1, T2, out TResult>(T1 input, out T2 output);

    class Program
    {
        private const int OptionPadding = 14;

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
                sb.Append($"{generatorArgs.InitId + i} {generatorArgs.ItemsCount} {generatorArgs.Ratio * totalWeight}");
                sb.AppendLine(string.Concat(weights.Zip(costs, (w, c) => $" {w} {c}")));
                Console.WriteLine("total weight " + totalWeight);
            }

            File.WriteAllText($"knap_{generatorArgs.ItemsCount.ToString().PadLeft(3, '0')}", sb.ToString());
            Console.WriteLine($"average total weight {overallTotalWeight / generatorArgs.InstancesCount}; first unused instance id {generatorArgs.InitId + generatorArgs.InstancesCount}");
            Console.ReadKey();
        }

        #region Generator arguments

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
                double exponent = ParseDoubleOption(args, "k", true, 0, 1); // TODO: not sure with limits here
                int balance = ParseInt32Option(args, "d", true, -1, 1);
                return new GeneratorArgs(initId, itemsCount, instancesCount, ratio, maxWeight, maxCost, exponent,
                    balance);
            }
            catch
            {
                return null;
            }
        }

        static int ParseInt32Option(string[] args, string option, bool compulsory, int lowerLimit, int upperLimit,
            int defaultValue = 0)
        {
            return ParseOption(args, option, compulsory, int.TryParse, lowerLimit, upperLimit, defaultValue);
        }

        static double ParseDoubleOption(string[] args, string option, bool compulsory, double lowerLimit,
            double upperLimit, double defaultValue = 0)
        {
            return ParseOption(args, option, compulsory, double.TryParse, lowerLimit, upperLimit, defaultValue);
        }

        static T ParseOption<T>(string[] args, string option, bool compulsory, ParseDelegate<string, T, bool> parse,
            T lowerLimit, T upperLimit, T defaultValue) where T : IComparable<T>
        {
            option = "-" + option;
            for (int i = 0; i < args.Length; i += 2)
            {
                if (string.Equals(args[i], option, StringComparison.InvariantCulture))
                {
                    T value;
                    if (!parse(args[i + 1], out value))
                    {
                        Console.WriteLine($"{option} option value is not a valid {typeof (T).Name} number.");
                        throw new ArgumentException();
                    }

                    if (value.CompareTo(lowerLimit) == -1 || value.CompareTo(upperLimit) == 1)
                    {
                        Console.WriteLine(
                            $"{option} option value is limited to range from {lowerLimit} to {upperLimit}.");
                        throw new ArgumentException();
                    }

                    return value;
                }
            }

            if (compulsory)
            {
                Console.WriteLine($"Option {option} is compulsory but it's missing.");
                throw new ArgumentException();
            }

            return defaultValue;
        }

        static void PrintHelp()
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("Usage: [-I id] -n count -N count -m ratio");
            sb.AppendLine("       -W weight -C cost -k exponent -d balance");
            sb.AppendLine();
            sb.AppendLine("Options:");
            AppendOption("[-I id]", "Initial instance ID, defaults to 0.", sb);
            AppendOption("-n count", "Total # of items per each instance.", sb);
            AppendOption("-N count", "Total # of instances per file.", sb);
            AppendOption("-m ratio", "The ratio of max knapsack capacity to total weight.", sb);
            AppendOption("-W weight", "Max item weight.", sb);
            AppendOption("-C cost", "Max item cost.", sb);
            AppendOption("-k exponent", "exponent k (real)", sb);
            AppendOption("-d balance", "-1 .. more small items, 1 .. more large items, 0 .. balance", sb);
            Console.WriteLine(sb.ToString());
        }

        static void AppendOption(string optionName, string description, StringBuilder sb)
        {
            sb.AppendLine($"    {optionName.PadRight(OptionPadding)}{description}");
        }

        #endregion
    }
}
