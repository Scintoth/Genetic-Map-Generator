using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Utilities;

namespace GeneticAlgorithmEngine
{
    public class RandomList<T> : IRandomisable<List<T>> where T : IRandomisable, new()
    {
        public List<T> Value { get; set; } = new List<T>();

        public RandomList(int numberOfItems)
        {
            Randomise(numberOfItems);
        }

        public void Randomise(float min, float max)
        {
            RandomiseAndReturn((int) min, max);
        }

        public List<T> RandomiseAndReturn(float min, float max)
        {
            return RandomiseAndReturn((int)min, (int)max);
        }

        public void Randomise(int min, int max)
        {
            RandomiseAndReturn(min, max);
        }

        public List<T> RandomiseAndReturn(int min, int max)
        {
            var rng = RandomService.GetRandomNumberGenerator();
            var result = rng.Next(min, max);

            for (var i = 0; i < result; i++)
            {
                Value.Add(new T());
                Value.Last().Randomise(min,max);
            }

            return Value;
        }

        public object GetValue()
        {
            return Value;
        }

        public void Randomise(int max)
        {
            RandomiseAndReturn(0, max);
        }

        public void Randomise(float max)
        {
            RandomiseAndReturn(0, max);
        }
    }
}
