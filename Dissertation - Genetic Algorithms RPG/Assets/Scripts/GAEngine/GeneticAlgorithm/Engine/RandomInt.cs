using System;
using Assets.Scripts.Utilities;

namespace GeneticAlgorithmEngine
{
    public class RandomInt : IRandomisable<int>
    {
        public int Value { get; set; }

        public RandomInt()
        {
            Randomise(0, 100);
        }

        public RandomInt(int min, int max)
        {
            Randomise(min, max);
        }

        public void Randomise(float min, float max)
        {
            RandomiseAndReturn(min, max);
        }

        public int RandomiseAndReturn(float min, float max)
        {
            return RandomiseAndReturn((int)min, (int)max);
        }

        public void Randomise(int min, int max)
        {
            RandomiseAndReturn(min, max);
        }

        public int RandomiseAndReturn(int min, int max)
        {
            var rng = RandomService.GetRandomNumberGenerator();
            var result = rng.Next(min, max);
            Value = result;
            return result;
        }

        public object GetValue()
        {
            return Value;
        }

        public void Randomise(int max)
        {
            Randomise(0, max);
        }

        public void Randomise(float max)
        {
            Randomise(0, max);
        }
    }
}
