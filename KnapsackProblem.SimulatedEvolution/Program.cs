using System;
using System.Linq;
using System.Text;
using KnapsackProblem.Common;

namespace KnapsackProblem.SimulatedEvolution
{
    delegate TResult ParseDelegate<in T1, T2, out TResult>(T1 input, out T2 output);

    class Program
    {
        private const int OptionPadding = 14;

        /* args:
        [0] - a path to directory with testing files
        [1] - how many files to use for testing
        [2] - how many times repeat a single file
        [3] - whether to save results
        [4 ...] - GA arguments
        */
        static void Main(string[] args)
        {
            var testFiles = Common.Helpers.LoadTestFiles(args);
            if (testFiles == null)
            {
                Console.ReadLine();
                return;
            }

            var genAlgArgs = ParseArguments(args);
            if (genAlgArgs == null)
            {
                Console.ReadKey();
                return;
            }

            var knapsacks = KnapsackLoader.LoadKnapsacks(testFiles.First(), 1);
            var ga = new GeneticAlgorithm(genAlgArgs);
            var result = ga.Solve(knapsacks.First());
            Console.WriteLine("BEST PRICE FOUND " + result.BestPrice);

            Console.ReadLine();
        }

        #region Generator arguments

        static GeneticAlgorithmArgs ParseArguments(string[] args)
        {
            if (args.Length == 0 || args[0] == "-?" || args[0] == "/?")
            {
                PrintHelp();
                return null;
            }

            try
            {
                int populationSize = ParseInt32Option(args, "p", true, 0, int.MaxValue);
                int maxGenerations = ParseInt32Option(args, "g", true, 0, int.MaxValue);
                int tournamentSize = ParseInt32Option(args, "t", true, 0, populationSize);
                double elitismDegree = ParseDoubleOption(args, "e", true, 0, 1);
                
                return new GeneticAlgorithmArgs(populationSize, maxGenerations, tournamentSize, elitismDegree);
            }
            catch { return null; }
        }

        static int ParseInt32Option(string[] args, string option, bool compulsory, int lowerLimit, int upperLimit, int defaultValue = 0)
        {
            return ParseOption(args, option, compulsory, int.TryParse, lowerLimit, upperLimit, defaultValue);
        }

        static double ParseDoubleOption(string[] args, string option, bool compulsory, double lowerLimit, double upperLimit, double defaultValue = 0)
        {
            return ParseOption(args, option, compulsory, double.TryParse, lowerLimit, upperLimit, defaultValue);
        }

        static T ParseOption<T>(string[] args, string option, bool compulsory, ParseDelegate<string, T, bool> parse, T lowerLimit, T upperLimit, T defaultValue) where T : IComparable<T>
        {
            option = "-" + option;
            for (int i = 0; i < args.Length; i += 2)
            {
                if (string.Equals(args[i], option, StringComparison.InvariantCulture))
                {
                    T value;
                    if (!parse(args[i + 1], out value))
                    {
                        Console.WriteLine($"{option} option value is not a valid {typeof(T).Name} number.");
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
            sb.AppendLine("Usage: -p size -g count -t size -e degree");
            sb.AppendLine();
            sb.AppendLine("Options:");
            AppendOption("-p size", "Population size.", sb);
            AppendOption("-g count", "Total # of generations before stopping.", sb);
            AppendOption("-t size", "Tournament size.", sb);
            AppendOption("-e degree", "Percents of a population's fittest pass to the next generation.", sb);
            Console.WriteLine(sb.ToString());
        }

        static void AppendOption(string optionName, string description, StringBuilder sb)
        {
            sb.AppendLine($"    {optionName.PadRight(OptionPadding)}{description}");
        }

        #endregion
    }
}
