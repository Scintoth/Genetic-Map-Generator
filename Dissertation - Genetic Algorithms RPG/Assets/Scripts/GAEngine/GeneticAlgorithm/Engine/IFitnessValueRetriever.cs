using System.Collections.Generic;

namespace Assets.Scripts.GAEngine.GeneticAlgorithm.Engine
{
    public interface IExpectedFitnessRetriever
    {
        Dictionary<string, object> RetrieveFitnessValues();
    }

    class MapExpectedFitnessRetriever : IExpectedFitnessRetriever
    {
        public MapExpectedFitnessRetriever()
        {

        }

        public Dictionary<string, object> RetrieveFitnessValues()
        {
            var mapParameters = MapParameters.MP;

            return new Dictionary<string, object>
            {
                {"Underwater", mapParameters.Underwater},
                {"Mountainous", mapParameters.Mountainous},
                {"Grassland", mapParameters.Grassland},
                {"TargetPeaks", mapParameters.TargetPeaks}
            };
        }
    }
}