using System;
using System.Xml;

namespace GeneticAlgorithmEngine
{
    public interface IRandomisable<T> : IRandomisable
    {
        T Value { get; set; }

        T RandomiseAndReturn(float min, float max);
        T RandomiseAndReturn(int min, int max);
    }

    public interface IRandomisable
    {
        void Randomise(float min, float max);
        void Randomise(int min, int max);
        object GetValue();
        void Randomise(int max);
        void Randomise(float max);
    }
}
