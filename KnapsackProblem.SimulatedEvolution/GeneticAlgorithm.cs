using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KnapsackProblem.Common;

namespace KnapsackProblem.SimulatedEvolution
{
    public sealed class GeneticAlgorithm
    {
        private static readonly Random random = new Random();
        private readonly GeneticAlgorithmArgs args;
        private int chromosomeLength;

        public GeneticAlgorithm(GeneticAlgorithmArgs args)
        {
            this.args = args;
        }

        public KnapsackSolution Solve(Knapsack knapsack)
        {
            chromosomeLength = knapsack.InstanceSize;
            var population = CreatePopulation(args.PopulationSize);
            int generation = 0;

            while (true)
            {
                foreach (var chromosome in population)
                {
                    EvaluateFitness(chromosome, knapsack);
                }

                PrintStatus(generation, population);
                if (generation == args.MaxGenerations) // won't trigger if max generations is not set
                    break;

                var newPopulation = new Chromosome[args.PopulationSize];
                var breedingPool = new List<Chromosome>(args.PopulationSize);
                int takeElites = 0;
                if (args.ElitismDegree > 0)
                {
                    takeElites = (int) (args.PopulationSize * args.ElitismDegree);
                    var elites = population.OrderByDescending(ch => ch.Fitness).Take(takeElites).ToArray();
                    Array.Copy(elites, newPopulation, elites.Length);
                }

                for (int i = takeElites; i < args.PopulationSize; i++)
                {
                    breedingPool.Add(population.TakeRandom(args.TournamentSize, random).OrderByDescending(ch => ch.Fitness).First());
                }

                for (int i = takeElites; i < args.PopulationSize - 1; i++)
                {
                    newPopulation[i] = Cross(breedingPool[i - takeElites], breedingPool[i + 1 - takeElites]);
                }
                newPopulation[args.PopulationSize - 1] = Cross(breedingPool[args.PopulationSize - 1 - takeElites], breedingPool[0]);
                MutateRandomChromosome(newPopulation); // TODO: could be parametrized

                population = newPopulation;
                generation++;
            }

            var fittest = population.OrderBy(ch => ch.Fitness).First();
            return new KnapsackSolution(fittest.Fitness, fittest.Gens, knapsack);
        }

        private Chromosome[] CreatePopulation(int populationSize)
        {
            var population = new Chromosome[populationSize];
            var possibleChromosomesCount = (int)Math.Pow(2, chromosomeLength);
            for (int i = 0; i < populationSize; i++)
            {
                int randomNumber = random.Next(1, possibleChromosomesCount);
                var gens = new BitArray(chromosomeLength);
                for (int j = 0; j < chromosomeLength; j++)
                {
                    if ((randomNumber & (1 << j)) != 0)
                        gens.Set(j, true);
                }
                population[i] = new Chromosome(gens);
            }
            return population;
        }

        public static void EvaluateFitness(Chromosome chromosome, Knapsack knapsack)
        {
            for (int i = 0; i < chromosome.Gens.Count; i++)
            {
                if (chromosome.Gens.Get(i))
                {
                    var matchingItem = knapsack.Items[i];
                    chromosome.Weight += matchingItem.Weight;
                    chromosome.Fitness += matchingItem.Price;
                }

                if (chromosome.Weight > knapsack.Capacity)
                    chromosome.Fitness = 0;
            }
        }

        private static Chromosome Cross(Chromosome chromosome, Chromosome other)
        {
            // TODO: consider creating two offsprings instead of one
            var gensCount = chromosome.Gens.Length;
            int randomCutIndex = random.Next(1, gensCount);
            var newGens = new BitArray(chromosome.Gens);
            for (int i = randomCutIndex; i < gensCount; i++)
            {
                newGens.Set(i, other.Gens.Get(i));
            }
            return new Chromosome(newGens);
        }

        private void MutateRandomChromosome(Chromosome[] population)
        {
            int randomChromosomeIndex = random.Next(0, population.Length);
            int randomGeneIndex = random.Next(0, chromosomeLength);
            population[randomChromosomeIndex].Gens[randomGeneIndex] = !population[randomChromosomeIndex].Gens[randomGeneIndex];
        }

        private static void PrintStatus(int generation, Chromosome[] population)
        {
            int maxFitness = population.Max(ch => ch.Fitness);
            int totalFitness = population.Sum(ch => ch.Fitness);
            Console.WriteLine($"Generation {generation.PadRight(2)}: max fitness - {maxFitness.PadRight(8)}, total fitness - {totalFitness.PadRight(8)}");
        }
    }
}