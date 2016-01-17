using System;
using System.Collections.Generic;
using System.Text;
using KnapsackProblem.Common;
using static KnapsackProblem.Common.ArgumentHelpers;

namespace KnapsackProblem.SimulatedEvolution
{
    class Program
    {
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
                Console.ReadLine();
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
                var popMngmnt = ParseInt32Option(args, "pm", true, 0, 2);
                int elitesCount = ParseInt32Option(args, "e", true, 0, populationSize / 2);
                var mutateProb = ParseDoubleOption(args, "m", true, 0, 1);
                
                return new GeneticAlgorithmArgs(populationSize, maxGenerations, tournamentSize, (PopulationManagement)popMngmnt, elitesCount, mutateProb);
            }
            catch { return null; }
        }

        static void PrintHelp()
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("Usage: -p size -g count -t size -pm method -e count -m probability");
            sb.AppendLine();
            sb.AppendLine("Options:");
            sb.AppendOption("-p size", "Population size.");
            sb.AppendOption("-g count", "Total # of generations before stopping.");
            sb.AppendOption("-t size", "Tournament size.");
            sb.AppendOption("-pm method", "Population management method. 0 - replace all, 1 - replace all but elites, 2 - replace weakest");
            sb.AppendOption("-e count", "# of a population's fittest pass to the next generation.");
            sb.AppendOption("-m probability", "Probability that a single random offspring's gen is mutated.");
            Console.WriteLine(sb.ToString());
        }
    }
}
