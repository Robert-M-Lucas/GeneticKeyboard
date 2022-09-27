using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticKeyboard
{
    internal class GeneticAlgorithm
    {
        int populationSize;
        int survivors;
        int randomPerGen;
        int gens;
        KeyboardGenType keyboardGenType;

        public GeneticAlgorithm(int population_size, int survivors, int random_per_gen, int gens, KeyboardGenType keyboardGenType)
        {
            populationSize = population_size;
            this.survivors = survivors;
            randomPerGen = random_per_gen;
            this.gens = gens;
            this.keyboardGenType = keyboardGenType;
        }

        public void Run()
        {
            Console.WriteLine("Starting genetic algorithm");

            Random r = new Random();

            List<Keyboard> population = new List<Keyboard>();

            for (int _ = 0; _ < populationSize; _++) population.Add(new Keyboard());

            Stopwatch s = new Stopwatch();

            for (int gen = 0; gen < gens; gen++)
            {
                s.Start();

                ThreadPool.Pool(12, 25, population, (Keyboard k) => k.CalculateFitness);

                s.Stop();
                long average_fitness_calculate_time = s.ElapsedMilliseconds / populationSize;
                s.Start();

                float avg = population.Sum(o => o.Fitness) / population.Count();

                population = population.OrderByDescending(p => p.Fitness).ToList();

                while (population.Count > survivors)
                {
                    population.RemoveAt(0);
                }

                Keyboard best_keyboard = population[population.Count - 1];
                float best = best_keyboard.Fitness;
                

                for (int _ = 0; _ < randomPerGen; _++) population.Add(new Keyboard());

                List<Keyboard> new_population = new List<Keyboard>();

                for (int _ = 0; _ < populationSize; _++)
                {
                    new_population.Add(new Keyboard(population[r.Next(population.Count)], population[r.Next(population.Count)], keyboardGenType));
                }

                population = new_population;

                s.Stop();
                Console.WriteLine($"Generation: {gen}; Avg fitness calc time: {average_fitness_calculate_time}ms; Time: {s.ElapsedMilliseconds}ms; Avg: {avg}; Best: {best}");
                s.Reset();

                if (gen == gens - 1)
                {
                    Console.WriteLine("Best keyboard:");
                    int x = 0;
                    foreach(int i in best_keyboard.KeyboardLayout)
                    {
                        if (x == 10) Console.Write("\n ");
                        if (x == 21) Console.Write("\n  ");
                        Console.Write(Keyboard.AllKeys[i]);
                        Console.Write(" ");
                        x++;
                    }
                }
            }
        }
    }
}
