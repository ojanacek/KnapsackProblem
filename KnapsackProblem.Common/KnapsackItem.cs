namespace KnapsackProblem.Common
{
    public struct KnapsackItem
    {
        public ushort Weight { get; }
        public ushort Price { get; }

        public KnapsackItem(ushort weight, ushort price)
        {
            Weight = weight;
            Price = price;
        }

        public override string ToString() => $"weight: {Weight}, price: {Price}";
    }
}