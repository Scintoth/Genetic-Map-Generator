using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GeneticMapGenerator.Domain.Evolution.Evaluation
{
    /// <summary>Strategy for scoring a batch of genomes (sequentially or in parallel).</summary>
    public interface IFitnessEvaluator
    {
        IReadOnlyList<Scored<TGenome>> Evaluate<TGenome>(
            IReadOnlyList<TGenome> genomes,
            IFitnessFunction<TGenome> fitnessFunction,
            CancellationToken cancellationToken);
    }

    public sealed class SequentialFitnessEvaluator : IFitnessEvaluator
    {
        public IReadOnlyList<Scored<TGenome>> Evaluate<TGenome>(
            IReadOnlyList<TGenome> genomes,
            IFitnessFunction<TGenome> fitnessFunction,
            CancellationToken cancellationToken)
        {
            var scored = new Scored<TGenome>[genomes.Count];
            for (var i = 0; i < genomes.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                scored[i] = new Scored<TGenome>(genomes[i], fitnessFunction.Evaluate(genomes[i]));
            }

            return scored;
        }
    }

    /// <summary>
    /// Evaluates genomes across cores. Results are written by index, so output order (and therefore
    /// the run) is independent of thread scheduling.
    /// </summary>
    public sealed class ParallelFitnessEvaluator : IFitnessEvaluator
    {
        public IReadOnlyList<Scored<TGenome>> Evaluate<TGenome>(
            IReadOnlyList<TGenome> genomes,
            IFitnessFunction<TGenome> fitnessFunction,
            CancellationToken cancellationToken)
        {
            var scored = new Scored<TGenome>[genomes.Count];
            var options = new ParallelOptions { CancellationToken = cancellationToken };
            Parallel.For(0, genomes.Count, options, i =>
                scored[i] = new Scored<TGenome>(genomes[i], fitnessFunction.Evaluate(genomes[i])));
            return scored;
        }
    }
}
