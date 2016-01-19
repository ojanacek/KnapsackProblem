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

        private Knapsack currentKnapsack;
        private int chromosomeLength;

        private readonly List<Chromosome> breedingPool;

        public GeneticAlgorithm(GeneticAlgorithmArgs args)
        {
            this.args = args;
            breedingPool = new List<Chromosome>(args.PopulationSize);
        }

        public KnapsackSolution Solve(Knapsack knapsack)
        {
            currentKnapsack = knapsack;
            chromosomeLength = knapsack.InstanceSize;
            
            var population = CreateInitialPopulation();
            int generation = 0, noFitnessChangeFor = 0, lastBestFitness = 0;

            while (true)
            {
                if (args.PrintStatus)
                    PrintStatus(generation, population);
                if (generation == args.MaxGenerations || noFitnessChangeFor == -args.MaxGenerations)
                    break;

                List<Chromosome> parents;
                switch (args.PopulationManagement)
                {
                    case PopulationManagement.ReplaceAll:
                        parents = SelectParents(population, args.PopulationSize);
                        for (int i = 0; i < args.PopulationSize - 1; i++)
                        {
                            population[i] = Cross(parents[i], parents[i + 1]);
                        }
                        population[args.PopulationSize - 1] = Cross(parents[args.PopulationSize - 1], parents[0]);
                        break;
                    case PopulationManagement.ReplaceAllButElites:
                        int elitesCount = 0;
                        var newPopulation = CreateNewGeneration(population, out elitesCount);
                        parents = SelectParents(population, args.PopulationSize - elitesCount);
                        
                        for (int i = elitesCount; i < args.PopulationSize - 1; i++)
                        {
                            newPopulation[i] = Cross(parents[i - elitesCount], parents[i + 1 - elitesCount]);
                        }
                        newPopulation[args.PopulationSize - 1] = Cross(parents[args.PopulationSize - 1 - elitesCount], parents[0]);
                        population = newPopulation;
                        break;
                    case PopulationManagement.ReplaceWeakest:
                        parents = SelectParents(population, args.PopulationSize);

                        for (int i = 0; i < parents.Count - 1; i += 2)
                        {
                            var offspring = Cross(parents[i], parents[i + 1]);
                            population[FindWeakest(population)] = offspring;
                        }
                        break;
                    default:
                        throw new ArgumentException("Invalid population management method.");
                }

                generation++;

                if (args.MaxGenerations < 0)
                {
                    int bestFitness = population.Max(ch => ch.Fitness);
                    if (bestFitness == lastBestFitness)
                        noFitnessChangeFor++;
                    else
                    {
                        lastBestFitness = bestFitness;
                        noFitnessChangeFor = 0;
                    }
                }
            }

            var fittest = population.OrderByDescending(ch => ch.Fitness).First();
            return new KnapsackSolution(fittest.Fitness, fittest.Gens, knapsack);
        }

        private Chromosome[] CreateInitialPopulation()
        {
            var population = new Chromosome[args.PopulationSize];

            for (int i = 0; i < args.PopulationSize; i++)
            {
                var gens = new BitArray(chromosomeLength);
                for (int j = 0; j < chromosomeLength; j++)
                {
                    if (random.NextDouble() > .5)
                        gens.Set(j, true);
                }
                population[i] = new Chromosome(gens);
                EvaluateFitness(population[i]);
            }

            return population;
        }

        public void EvaluateFitness(Chromosome chromosome)
        {
            chromosome.Weight = chromosome.Fitness = 0;
            for (int i = 0; i < chromosome.Gens.Count; i++)
            {
                if (chromosome.Gens[i])
                {
                    var matchingItem = currentKnapsack.Items[i];
                    chromosome.Weight += matchingItem.Weight;
                    chromosome.Fitness += matchingItem.Price;
                }
            }

            if (chromosome.Weight > currentKnapsack.Capacity)
                chromosome.Fitness = 0;
        }

        private Chromosome[] CreateNewGeneration(Chromosome[] currentPopulation, out int elitesCount)
        {
            var newGeneration = new Chromosome[args.PopulationSize];
            elitesCount = 0;
            if (args.ElitesCount > 0)
            {
                var elites = currentPopulation.Where(ch => ch.Fitness > 0)
                                              .OrderByDescending(ch => ch.Fitness)
                                              .Take(args.ElitesCount).ToArray();
                elitesCount = elites.Length;
                Array.Copy(elites, newGeneration, elites.Length);
            }
            return newGeneration;
        }

        private List<Chromosome> SelectParents(Chromosome[] population, int parentsCount)
        {
            int populationFitness = population.Sum(ch => ch.Fitness);
            breedingPool.Clear();

            for (int i = 0; i < parentsCount; i++)
            {
                if (args.ParentSelection == ParentSelection.Tournament)
                {
                    Chromosome fittest = null;
                    for (int j = 0; j < args.TournamentSize; j++)
                    {
                        int randomIndex = random.Next(0, args.PopulationSize);
                        var next = population[randomIndex];
                        if (fittest == null || fittest.Fitness < next.Fitness)
                            fittest = next;
                    }
                    breedingPool.Add(fittest);
                }
                else
                {
                    int randomPoint = random.Next(0, populationFitness + 1);
                    int runningFitness = 0;
                    for (int j = 0; j < args.PopulationSize; j++)
                    {
                        runningFitness += population[j].Fitness;
                        if (runningFitness >= randomPoint)
                        {
                            breedingPool.Add(population[j]);
                            break;
                        }
                    }
                }
            }

            return breedingPool;
        }

        private int FindWeakest(Chromosome[] population)
        {
            int minFitness = population[0].Fitness;
            int weakestIndex = 0;
            for (int j = 1; j < args.PopulationSize; j++)
            {
                if (population[j].Fitness < minFitness)
                {
                    minFitness = population[j].Fitness;
                    weakestIndex = j;
                }
            }
            return weakestIndex;
        }

        private Chromosome Cross(Chromosome chromosome, Chromosome other)
        {
            int randomCutIndex = random.Next(1, chromosomeLength);
            var newGens = new BitArray(chromosome.Gens);
            for (int i = randomCutIndex; i < chromosomeLength; i++)
            {
                newGens.Set(i, other.Gens[i]);
            }
            var offspring = new Chromosome(newGens);

            if (random.NextDouble() < args.MutationProbability)
                MutateRandomChromosome(offspring);

            EvaluateFitness(offspring);
            return offspring;
        }

        private void MutateRandomChromosome(Chromosome chromosome)
        {
            int randomGeneIndex = random.Next(0, chromosomeLength);
            chromosome.Gens[randomGeneIndex] = !chromosome.Gens[randomGeneIndex];
        }

        private static void PrintStatus(int generation, Chromosome[] population)
        {
            int minFitness = population.Min(ch => ch.Fitness);
            int avgFitness = (int)population.Average(ch => ch.Fitness);
            int maxFitness = population.Max(ch => ch.Fitness);
            int totalFitness = population.Sum(ch => ch.Fitness);
            Console.WriteLine($"Generation {generation.PadLeft(3)}: Fitness; {minFitness.PadLeft(6)}; {avgFitness.PadLeft(6)}; {maxFitness.PadLeft(6)}; {totalFitness.PadLeft(8)}");
        }
    }
}