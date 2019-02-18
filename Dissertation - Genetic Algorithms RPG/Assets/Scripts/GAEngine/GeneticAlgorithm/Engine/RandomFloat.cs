using System;
using Assets.Scripts.Utilities;

namespace GeneticAlgorithmEngine
{
    public class RandomFloat : IRandomisable<float>
    {
        public float Value { get; set; }

        public RandomFloat(float min, float max)
        {
            Randomise(min,max);
        }

        public void Randomise(float min, float max)
        {
            RandomiseAndReturn(min, max);
        }

        public float RandomiseAndReturn(float min, float max)
        {
            var rng = RandomService.GetRandomNumberGenerator();
            var result = rng.NextDouble() * max;
            Value = (float)result;
            return (float)result;
        }

        public void Randomise(int min, int max)
        {
            RandomiseAndReturn(min, max);
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

        public float RandomiseAndReturn(int min, int max)
        {
            return RandomiseAndReturn((float)min, max);
        }
    }
}
