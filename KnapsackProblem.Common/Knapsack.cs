using System.Collections.Generic;
using System.Linq;

namespace KnapsackProblem.Common
{
    public sealed class Knapsack
    {
        public int Id { get; }
        public int Capacity { get; }
        public int InstanceSize => Items.Count;

        public List<KnapsackItem> Items { get; }

        public Knapsack(int id, int capacity, IEnumerable<KnapsackItem> items)
        {
            Id = id;
            Capacity = capacity;
            Items = items.ToList();
        }

        public override string ToString() => $"knapsack {Id}, capacity: {Capacity}";
    }
}