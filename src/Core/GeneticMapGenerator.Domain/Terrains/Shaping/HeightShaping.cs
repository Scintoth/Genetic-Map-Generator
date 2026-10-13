using System;
using System.Collections.Generic;

namespace GeneticMapGenerator.Domain.Terrains.Shaping
{
    /// <summary>One stage of the height pipeline.</summary>
    public interface IHeightShaper
    {
        /// <param name="raw">The unshaped noise elevation.</param>
        /// <param name="height">The output of the previous stage.</param>
        double Shape(double raw, double height);
    }

    /// <summary>Sharpens peaks and flattens lowlands: <c>raw ^ exponent</c>.</summary>
    public sealed class PowerCurveShaper : IHeightShaper
    {
        private readonly double _exponent;

        public PowerCurveShaper(double exponent) => _exponent = exponent;

        public double Shape(double raw, double height) => Math.Pow(raw, _exponent);
    }

    /// <summary>Caps height then rescales by raw elevation, carving mesas and basins.</summary>
    public sealed class MesaShaper : IHeightShaper
    {
        private const double Flattening = 0.3;

        private readonly double _maxHeight;

        public MesaShaper(double maxHeight) => _maxHeight = maxHeight;

        public double Shape(double raw, double height) => Math.Min(height, _maxHeight) * raw * Flattening;
    }

    /// <summary>Raises secondary hills on mesa tops where the unflattened terrain would have been very tall.</summary>
    public sealed class MesaHillsShaper : IHeightShaper
    {
        private const double TriggerFactor = 3.0;

        private readonly double _maxHeight;
        private readonly double _exponent;
        private readonly double _amplitude;

        public MesaHillsShaper(double maxHeight, double exponent, double amplitude)
        {
            _maxHeight = maxHeight;
            _exponent = exponent;
            _amplitude = amplitude;
        }

        public double Shape(double raw, double height)
        {
            var unflattened = Math.Pow(raw, _exponent);
            if (unflattened <= _maxHeight * TriggerFactor)
            {
                return height;
            }

            var baseline = _maxHeight * ((_exponent + _amplitude) / 2) - TriggerFactor;
            return height + (unflattened - baseline) / _maxHeight;
        }
    }

    /// <summary>Hard ceiling on terrain height.</summary>
    public sealed class CeilingShaper : IHeightShaper
    {
        private readonly double _ceiling;

        public CeilingShaper(double ceiling) => _ceiling = ceiling;

        public double Shape(double raw, double height) => Math.Min(height, _ceiling);
    }

    /// <summary>Ordered chain of shapers, assembled from a genome's switches.</summary>
    public sealed class HeightShapingPipeline : IHeightShaper
    {
        private const double CeilingFactor = 5.0;

        private readonly IReadOnlyList<IHeightShaper> _stages;

        public HeightShapingPipeline(IReadOnlyList<IHeightShaper> stages) => _stages = stages;

        public static HeightShapingPipeline For(TerrainGenome genome)
        {
            var stages = new List<IHeightShaper> { new PowerCurveShaper(genome.Exponent) };
            if (genome.FlattenTerrain)
            {
                stages.Add(new MesaShaper(genome.MaxHeight));
                if (genome.AdditionalHills)
                {
                    stages.Add(new MesaHillsShaper(genome.MaxHeight, genome.Exponent, genome.Amplitude));
                }
            }

            stages.Add(new CeilingShaper(genome.MaxHeight * CeilingFactor));
            return new HeightShapingPipeline(stages);
        }

        public double Shape(double raw, double height)
        {
            foreach (var stage in _stages)
            {
                height = stage.Shape(raw, height);
            }

            return height;
        }
    }
}
