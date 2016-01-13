using System.Collections;
using System.Linq;

namespace KnapsackProblem.SimulatedEvolution
{
    public sealed class Chromosome
    {
        public BitArray Gens { get; set; }

        public int Weight { get; set; }
        public int Fitness { get; set; }

        public Chromosome(BitArray gens)
        {
            Gens = gens;
        }

        public override string ToString()
        {
            return $"Weight: {Weight}, Fitness: {Fitness}, Gens: {string.Join(" ", Gens.Cast<bool>().Select(b => b ? "1" : "0"))}";
        }
    }
}