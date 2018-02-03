using Assets.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Calculator.PointElevationCalculator
{
    public class PointElevationCalculator
    {
        public PointElevationCalculator() {}

        public PointElevationCalculatorResult CalculateElevation(PointElevationCalculatorCommand command)
        {
            PointData highestPoint = null;
            PointData lowestPoint = null;

            var processingPoint = command.ProcessingPoint;

            foreach(var point in command.SurroundingPoints)
            {
                if(highestPoint == null)
                {
                    if (processingPoint.Location.y < point.Location.y)
                        highestPoint = point;
                    continue;
                }
                
                if(highestPoint.Location.y < point.Location.y)
                    highestPoint = point;
            }
            foreach(var point in command.SurroundingPoints)
            {
                if (lowestPoint == null)
                {
                    if (processingPoint.Location.y > point.Location.y)
                        lowestPoint = point;
                    continue;
                }
                if(lowestPoint.Location.y > point.Location.y)
                    lowestPoint = point;
            }
            processingPoint.HighestNeighbour = highestPoint;
            processingPoint.LowestNeighbour = lowestPoint;

            return new PointElevationCalculatorResult
            {
                Point = processingPoint
            };
        }
    }
}
