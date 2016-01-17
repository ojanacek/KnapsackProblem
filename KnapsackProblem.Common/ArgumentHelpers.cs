using System;
using System.Text;

namespace KnapsackProblem.Common
{
    delegate TResult ParseDelegate<in T1, T2, out TResult>(T1 input, out T2 output);

    public static class ArgumentHelpers
    {
        public static int OptionPadding = 14;

        public static void AppendOption(this StringBuilder sb, string optionName, string description)
        {
            sb.AppendLine($"    {optionName.PadRight(OptionPadding)}{description}");
        }

        public static int ParseInt32Option(string[] args, string option, bool compulsory, int lowerLimit, int upperLimit, int defaultValue = 0)
        {
            return ParseOption(args, option, compulsory, int.TryParse, lowerLimit, upperLimit, defaultValue);
        }

        public static double ParseDoubleOption(string[] args, string option, bool compulsory, double lowerLimit, double upperLimit, double defaultValue = 0)
        {
            return ParseOption(args, option, compulsory, double.TryParse, lowerLimit, upperLimit, defaultValue);
        }

        static T ParseOption<T>(string[] args, string option, bool compulsory, ParseDelegate<string, T, bool> parse, T lowerLimit, T upperLimit, T defaultValue) where T : IComparable<T>
        {
            option = "-" + option;
            for (int i = 0; i < args.Length; i += 2)
            {
                if (string.Equals(args[i], option, StringComparison.InvariantCulture))
                {
                    T value;
                    if (!parse(args[i + 1], out value))
                    {
                        Console.WriteLine($"{option} option value is not a valid {typeof(T).Name} number.");
                        throw new ArgumentException();
                    }

                    if (value.CompareTo(lowerLimit) == -1 || value.CompareTo(upperLimit) == 1)
                    {
                        Console.WriteLine($"{option} option value is limited to range from {lowerLimit} to {upperLimit}.");
                        throw new ArgumentException();
                    }

                    return value;
                }
            }

            if (compulsory)
            {
                Console.WriteLine($"Option {option} is compulsory but it's missing.");
                throw new ArgumentException();
            }

            return defaultValue;
        }
    }
}