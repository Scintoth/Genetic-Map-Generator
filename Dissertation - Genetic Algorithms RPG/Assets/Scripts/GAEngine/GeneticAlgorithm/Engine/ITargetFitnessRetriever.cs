using System.Collections.Generic;

namespace Assets.Scripts.GAEngine.GeneticAlgorithm.Engine
{
    public interface ITargetFitnessRetriever
    {
        Dictionary<string, object> GetFitnessParameters();
    }

    public class MapFitnessRetriever : ITargetFitnessRetriever
    {
        public Dictionary<string, object> GetFitnessParameters()
        {
            var mapParameters = MapParameters.MP;

            var result = new Dictionary<string, object>
            {
                {"Grassland",  mapParameters.Grassland},
                {"Underwater",  mapParameters.Underwater},
                {"Mountainous", mapParameters.Mountainous },
                {"TargetPeaks", mapParameters.TargetPeaks }
            };

            return result;
        }
    }
}