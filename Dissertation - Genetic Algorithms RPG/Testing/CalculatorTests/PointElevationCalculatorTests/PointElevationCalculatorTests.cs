using Assets.Calculator.PointElevationCalculator;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Entities;
using UnityEngine;
using Utilities;
using FluentAssertions;

namespace Testing.CalculatorTests.PointElevationCalculatorTests
{
    [TestFixture]
    class PointElevationCalculatorTests
    {
        PointElevationCalculator _pointElevationCalculator;
        PointData _processingPoint;


        [SetUp]
        public void SetUp()
        {
            _pointElevationCalculator = new PointElevationCalculator();
            _processingPoint = Point(new Vector3(0, 0, 0));
        }

        [Test]
        public async Task GivenThereIsAPointHigherThanTheProcessingPoint_ItIsReturned()
        {
            var higherPoint = Point(new Vector3(0, 2, 0));
            var listIncludingHigherPoint = higherPoint.Only();

            var command = GenerateCommand(listIncludingHigherPoint);
            var result = _pointElevationCalculator.CalculateElevation(command);
            result.Point.HighestNeighbour.ShouldBeEquivalentTo(higherPoint);
        }

        [Test]
        public async Task GivenThereAreMultiplePointsHigherThanTheProcessingPoint_TheHighestAmongThemIsReturned()
        {
            var higherPoint = Point(new Vector3(0, 1, 0));
            var highestPointLocation = new Vector3(0, 2, 0);
            var highestPoint = Point(highestPointLocation);

            var surroundingPoints = ListThis(higherPoint, highestPoint);

            var command = GenerateCommand(surroundingPoints);
            var result = _pointElevationCalculator.CalculateElevation(command);
            result.Point.HighestNeighbour.Location.ShouldEqual(highestPointLocation);
        }

        [Test]
        public async Task GivenThereIsAPointThatIsLowerAndAPointThatIsHigher_TheHighestNeighbourReturnedIsTheHigherPoint()
        {
            var lowerPoint = Point(new Vector3(0, -1, 0));
            var higherPointLocation = new Vector3(0, 1, 0);
            var higherPoint = Point(higherPointLocation);

            var surroundingPoints = ListThis(lowerPoint, higherPoint);

            var command = GenerateCommand(surroundingPoints);
            var result = _pointElevationCalculator.CalculateElevation(command);
            result.Point.HighestNeighbour.Location.ShouldEqual(higherPointLocation);
        }

        [Test]
        public async Task GivenThereAreOnlyPointsThatAreLowerThanTheProcessingPoint_TheHighestPointShouldBeNull()
        {
            var lowerPoint1 = Point(new Vector3(0,-1,0));
            var lowerPoint2 = Point(new Vector3(0,-2,0));
            
            var surroundingPoints = ListThis(lowerPoint1, lowerPoint2);

            var command = GenerateCommand(surroundingPoints);
            var result = _pointElevationCalculator.CalculateElevation(command);
            result.Point.HighestNeighbour.ShouldBeNull();
        }

        [Test]
        public async Task GivenThereIsAPointLowerThanTheProcessingPoint_TheLowestNeighbourIsThatPoint()
        {
            var lowerPointLocation = new Vector3(0, -1, 0);
            var lowerPoint = Point(lowerPointLocation);

            var surroundingPoints = lowerPoint.Only();

            var command = GenerateCommand(surroundingPoints);
            var result = _pointElevationCalculator.CalculateElevation(command);
            result.Point.LowestNeighbour.Location.ShouldEqual(lowerPointLocation);
        }

        [Test]
        public async Task GivenThereAreMultiplePointsLowerThanTheProcessingPoint_TheLowestNeighbourIsTheLowestAmongThem()
        {
            var lowestPointLocation = new Vector3(0,-2,0);
            var lowPoint = Point(new Vector3(0,-1,0));
            var lowestPoint = Point(lowestPointLocation);

            var surroundingPoints = ListThis(lowPoint, lowestPoint);

            var command = GenerateCommand(surroundingPoints);
            var result = _pointElevationCalculator.CalculateElevation(command);
            result.Point.LowestNeighbour.Location.ShouldEqual(lowestPointLocation);
        }

        #region Private Functions

        private PointElevationCalculatorCommand GenerateCommand(List<PointData> surroundingPoints)
        {
            return new PointElevationCalculatorCommand
            {
                ProcessingPoint = _processingPoint,
                SurroundingPoints = surroundingPoints
            };
        }

        private PointData Point(Vector3 location)
        {
            return new PointData
            {
                Location = location,
                HighestNeighbour = null,
                LowestNeighbour = null              
            };
        }

        private List<T> ListThis<T>(params T[] listItems)
        {
            return listItems.ToList();
        }

        #endregion
    }
}
