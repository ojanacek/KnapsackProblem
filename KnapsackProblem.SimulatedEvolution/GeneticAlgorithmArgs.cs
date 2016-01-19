namespace KnapsackProblem.SimulatedEvolution
{
    public sealed class GeneticAlgorithmArgs
    {
        public int PopulationSize { get; }
        public int MaxGenerations { get; }
        public ParentSelection ParentSelection { get; set; }
        public int TournamentSize { get; }
        public PopulationManagement PopulationManagement { get; set; }
        public int ElitesCount { get; }
        public double MutationProbability { get; set; }

        public bool PrintStatus { get; set; }

        public GeneticAlgorithmArgs(int populationSize, int maxGenerations, ParentSelection parentSelection, int tournamentSize, PopulationManagement populationManagement, 
            int elitesCount, double mutationProbability, bool printStatus)
        {
            PopulationSize = populationSize;
            MaxGenerations = maxGenerations;
            ParentSelection = parentSelection;
            TournamentSize = tournamentSize;
            PopulationManagement = populationManagement;
            ElitesCount = elitesCount;
            MutationProbability = mutationProbability;
            PrintStatus = printStatus;
        }
    }
}