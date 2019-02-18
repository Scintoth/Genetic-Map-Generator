using System;

namespace Assets.Scripts.Utilities
{
    public interface IRandomService
    {
        Random GetRandomNumberGenerator();
    }

    public static class RandomService// : IRandomService
    {
        private static Random _random;


        public static Random GetRandomNumberGenerator()
        {
            if(_random == null)
                _random = new Random(DateTime.Now.Millisecond);
            return _random;
        }
    }
}