using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticKeyboard
{
    internal static class FitnessEvaluator
    {
        private static string source = "";
        public static bool ResetFingers = false;

        public static void PickAndProcessSourceFile()
        {
            while (true)
            {
                Console.Write("Enter path to source file: ");
                string path = Console.ReadLine() ?? throw new Exception("IO Error");

                try
                {
                    string source_unprocessed = File.ReadAllText(path);
                    Console.WriteLine($"File found - Length: {source_unprocessed.Length}");
                    source = ProcessSource(source_unprocessed);
                    break;
                }
                catch (FileNotFoundException)
                {
                    Console.WriteLine("File not found");
                    continue;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing file: {ex})");
                    continue;
                }
            }
        }

        private static string ProcessSource(string unprocessed)
        {
            Console.WriteLine("Processing source");

            unprocessed = unprocessed.ToLower();

            string new_source = "";

            foreach (char c in unprocessed)
            {
                if (c == '"')
                {
                    new_source += '\'';
                    continue;
                }

                if (Keyboard.ResetKeys.Contains(c))
                {
                    new_source += ' ';
                    continue;
                }

                if (Keyboard.AllKeys.Contains(c)) new_source += c;
            }

            Console.WriteLine("Processing complete");
            return new_source;
        }

        public static float GetFitness(Keyboard keyboard)
        {
            float fitness = 0.0f;

            foreach (char c in source)
            {
                if (c == ' ')
                {
                    if (ResetFingers) keyboard.ResetFingers();
                    continue;
                }

                fitness += keyboard.GetTravelTime(c);
            }

            return fitness;
        }
    }
}
