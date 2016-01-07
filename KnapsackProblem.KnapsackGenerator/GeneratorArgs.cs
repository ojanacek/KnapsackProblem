namespace KnapsackProblem.KnapsackGenerator
{
    internal sealed class GeneratorArgs
    {
        public int InitId { get; }
        public int ItemsCount { get; }
        public int InstancesCount { get; }
        public double Ratio { get; }
        public int MaxWeight { get; }
        public int MaxCost { get; }
        public double ExponentK { get; }
        public int Balance { get; }

        public GeneratorArgs(int initId, int itemsCount, int instancesCount, double ratio, int maxWeight, int maxCost, double exponentK, int balance)
        {
            InitId = initId;
            ItemsCount = itemsCount;
            InstancesCount = instancesCount;
            Ratio = ratio;
            MaxWeight = maxWeight;
            MaxCost = maxCost;
            ExponentK = exponentK;
            Balance = balance;
        }
    }
}