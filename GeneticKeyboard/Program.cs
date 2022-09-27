using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticKeyboard
{
    public class Program
    {
        public static void Main(string[] args)
        {
            FitnessEvaluator.PickAndProcessSourceFile();
            FitnessEvaluator.ResetFingers = true;
            Keyboard k = new Keyboard(qwerty: true);
            k.CalculateFitness();
            Console.WriteLine($"QWERTY Score: {k.Fitness}");
            new GeneticAlgorithm(population_size: 500, survivors: 225, random_per_gen: 25, gens: 300, KeyboardGenType.Default).Run();
        }
    }
}