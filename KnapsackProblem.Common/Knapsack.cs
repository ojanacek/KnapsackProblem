using System.Collections.Generic;
using System.Linq;

namespace KnapsackProblem.Common
{
    public sealed class Knapsack
    {
        public short Id { get; }
        public short Capacity { get; }
        public int InstanceSize => Items.Count;

        public List<KnapsackItem> Items { get; }

        public Knapsack(short id, short capacity, IEnumerable<KnapsackItem> items)
        {
            Id = id;
            Capacity = capacity;
            Items = items.ToList();
        }

        public override string ToString() => $"knapsack {Id}, capacity: {Capacity}";
    }
}