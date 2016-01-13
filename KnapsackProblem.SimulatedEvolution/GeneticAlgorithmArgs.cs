namespace KnapsackProblem.SimulatedEvolution
{
    public sealed class GeneticAlgorithmArgs
    {
        public int PopulationSize { get; }
        public int MaxGenerations { get; }
        public int TournamentSize { get; }
        public double ElitismDegree { get; }

        public GeneticAlgorithmArgs(int populationSize, int maxGenerations, int tournamentSize, double elitismDegree)
        {
            PopulationSize = populationSize;
            MaxGenerations = maxGenerations;
            TournamentSize = tournamentSize;
            ElitismDegree = elitismDegree;
        }
    }
}