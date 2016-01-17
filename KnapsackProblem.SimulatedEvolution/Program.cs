using System;
using System.Collections.Generic;
using System.Text;
using KnapsackProblem.Common;

namespace KnapsackProblem.SimulatedEvolution
{
    delegate TResult ParseDelegate<in T1, T2, out TResult>(T1 input, out T2 output);

    class Program
    {
        private const int OptionPadding = 14;

        /* args:
        [0] - a path to a test file
        [1] - a path to a solution file
        [2] - how many times repeat a test
        [3] - whether to save results        
        [4 ...] - GA arguments
        */
        static void Main(string[] args)
        {
            var genAlgArgs = ParseArguments(args);
            if (genAlgArgs == null)
            {
                Console.ReadKey();
                return;
            }

            var ga = new GeneticAlgorithm(genAlgArgs);
            var results = new List<KnapsackSolution>();
            foreach (var knapsack in KnapsackLoader.LoadKnapsacks(args[0], KnapsackLoader.KnapsackPerFile))
            {
                results.Add(ga.Solve(knapsack));
            }

            if (bool.Parse(args[3]))
                Common.Helpers.SaveSolutions(results);

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
                var popMngmnt = (PopulationManagement) ParseInt32Option(args, "pm", true, 0, 2);
                int elitesCount = ParseInt32Option(args, "e", true, 0, populationSize / 2);
                var mutateProb = ParseDoubleOption(args, "m", true, 0, 1);
                
                return new GeneticAlgorithmArgs(populationSize, maxGenerations, tournamentSize, popMngmnt, elitesCount, mutateProb);
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
            sb.AppendLine("Usage: -p size -g count -t size -pm method -e count -m probability");
            sb.AppendLine();
            sb.AppendLine("Options:");
            AppendOption("-p size", "Population size.", sb);
            AppendOption("-g count", "Total # of generations before stopping.", sb);
            AppendOption("-t size", "Tournament size.", sb);
            AppendOption("-pm method", "Population management method. 0 - replace all, 1 - replace all but elites, 2 - replace weakest", sb);
            AppendOption("-e count", "# of a population's fittest pass to the next generation.", sb);
            AppendOption("-m probability", "Probability that a single random offspring's gen is mutated.", sb);
            Console.WriteLine(sb.ToString());
        }

        static void AppendOption(string optionName, string description, StringBuilder sb)
        {
            sb.AppendLine($"    {optionName.PadRight(OptionPadding)}{description}");
        }

        #endregion
    }
}
