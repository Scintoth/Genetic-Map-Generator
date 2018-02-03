using Assets.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Calculator.PointElevationCalculator
{
    public class PointElevationCalculatorCommand
    {
        public PointData ProcessingPoint { get; set; }
        public List<PointData> SurroundingPoints { get; set; }
    }
}
