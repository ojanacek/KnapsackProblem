namespace KnapsackProblem.SimulatedEvolution
{
    public sealed class GeneticAlgorithmArgs
    {
        public int PopulationSize { get; }
        public int MaxGenerations { get; }
        public int TournamentSize { get; }
        public PopulationManagement PopulationManagement { get; set; }
        public int ElitesCount { get; }
        public double MutationProbability { get; set; }

        public GeneticAlgorithmArgs(int populationSize, int maxGenerations, int tournamentSize, PopulationManagement populationManagement, int elitesCount, double mutationProbability)
        {
            PopulationSize = populationSize;
            MaxGenerations = maxGenerations;
            TournamentSize = tournamentSize;
            PopulationManagement = populationManagement;
            ElitesCount = elitesCount;
            MutationProbability = mutationProbability;
        }
    }
}