using System;
using System.Collections.Generic;

namespace KnapsackProblem.SimulatedEvolution
{
    public static class Helpers
    {
        public static IEnumerable<T> TakeRandom<T>(this T[] array, int count, Random random)
        {
            for (int i = 0; i < count; i++)
            {
                int randomIndex = random.Next(0, array.Length);
                yield return array[randomIndex];
            }
        }

        public static string PadRight(this int number, int totalWidth)
        {
            return number.ToString().PadLeft(totalWidth, ' ');
        }
    }
}