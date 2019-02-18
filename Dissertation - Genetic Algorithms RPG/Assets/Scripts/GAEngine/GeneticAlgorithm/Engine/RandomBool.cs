using System;
using Assets.Scripts.Utilities;
using GeneticAlgorithmEngine;

namespace Assets.Scripts.GAEngine.GeneticAlgorithm.Engine
{
    public class RandomBool : IRandomisable<bool>
    {
        public bool Value { get; set; }


        public RandomBool(float truePercentChance)
        {
            var rng = RandomService.GetRandomNumberGenerator();
            Randomise(truePercentChance);
        }

        public void Randomise(float min, float max)
        {
            var rng = RandomService.GetRandomNumberGenerator();
            var value = rng.Next(0, 100);
            Value = min < value && value < max;
        }

        public void Randomise(int min, int max)
        {
            var rng = RandomService.GetRandomNumberGenerator();
            var value = rng.Next(0, 100);
            Value = min < value && value < max;
        }

        public object GetValue()
        {
            return Value;
        }

        public void Randomise(int max)
        {
            var rng = RandomService.GetRandomNumberGenerator();
            var value = rng.Next(0, 100);
            Value = value < max;
        }

        public void Randomise(float max)
        {
            var rng = RandomService.GetRandomNumberGenerator();
            var value = rng.Next(0, 100);
            Value = value < max;
        }

        public bool RandomiseAndReturn(float min, float max)
        {
            var rng = RandomService.GetRandomNumberGenerator();
            var value = rng.Next(0, 100);
            Value = min < value && value < max;
            return Value;
        }

        public bool RandomiseAndReturn(int min, int max)
        {
            var rng = RandomService.GetRandomNumberGenerator();
            var value = rng.Next(0, 100);
            Value = min < value && value < max;
            return Value;
        }
    }
}