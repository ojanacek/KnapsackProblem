using System.Text;

namespace KnapsackProblem.Common
{
    public static class Helpers
    {
        /// <summary>
        /// Returns a specific binary format of a ulong value.
        /// </summary>
        public static string ToReverseBinary(this ulong value, int binaryDigitsCount)
        {
            var sb = new StringBuilder();
            int binaryDigits = 0;
            while (value != 0)
            {
                sb.Append((value & 1) == 1 ? " 1" : " 0");
                value >>= 1;
                binaryDigits++;
            }
            
            while (binaryDigits < binaryDigitsCount)
            {
                sb.Append(" 0");
                binaryDigits++;
            }

            return sb.ToString();
        }
    }
}