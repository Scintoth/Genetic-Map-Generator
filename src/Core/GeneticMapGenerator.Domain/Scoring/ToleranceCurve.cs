using System;

namespace GeneticMapGenerator.Domain.Scoring
{
    /// <summary>Maps how close an observed value is to its target onto a [0, 1] score.</summary>
    public interface IToleranceCurve
    {
        double Score(double actual, double target);
    }

    /// <summary>
    /// The dissertation's own scoring curve, <c>tanh((tolerance / relativeError) ^ sharpness)</c>:
    /// 1 on target, ~0.76 at <c>tolerance</c> relative error, falling towards 0 beyond it.
    /// Small targets use an absolute floor so a target of zero stays well-defined.
    /// </summary>
    public sealed class HyperbolicToleranceCurve : IToleranceCurve
    {
        private readonly double _tolerance;
        private readonly double _sharpness;
        private readonly double _scaleFloor;

        public HyperbolicToleranceCurve(double tolerance = 0.3, double sharpness = 1.57, double scaleFloor = 0.05)
        {
            if (tolerance <= 0 || sharpness <= 0 || scaleFloor <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(tolerance), "Curve parameters must be positive.");
            }

            _tolerance = tolerance;
            _sharpness = sharpness;
            _scaleFloor = scaleFloor;
        }

        public double Score(double actual, double target)
        {
            var relativeError = Math.Abs(actual - target) / Math.Max(Math.Abs(target), _scaleFloor);
            return relativeError < 1e-9 ? 1.0 : Math.Tanh(Math.Pow(_tolerance / relativeError, _sharpness));
        }
    }
}
