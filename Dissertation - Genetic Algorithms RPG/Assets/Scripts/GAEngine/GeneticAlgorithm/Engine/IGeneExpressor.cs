using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.Entities;
using Assets.Scripts.GAEngine.GeneticAlgorithm;
using Assets.Scripts.GAEngine.Map;

namespace GeneticAlgorithmEngine
{
    public interface IGeneExpressor
    {
        Task<IExpressedGeneData> Express(List<IGeneticData> geneticData);
    }

    public class MapGeneExpressor : IGeneExpressor
    {
        private IHeightMapGenerator _heightMapGenerator;

        public MapGeneExpressor(IHeightMapGenerator heightMapGenerator, IFitnessCalculator fitnessCalculator)
        {
            _heightMapGenerator = heightMapGenerator;
        }

        public async Task<IExpressedGeneData> Express(List<IGeneticData> geneticData)
        {

            var mapGenerationParameters = new MapGenerationParameters
            {
                Exponent = (float)geneticData[0].Data["Exponent"].GetValue(),
                AdditionalHills = (bool)geneticData[0].Data["AdditionalHills"].GetValue(),
                FlattenTerrain = (bool)geneticData[0].Data["FlattenTerrain"].GetValue(),
                Genes = (List<RandomInt>)geneticData[0].Data["Coordinates"].GetValue(),
                MaximumHeight = (float)geneticData[0].Data["MaximumHeight"].GetValue(),
                MountainLevel = (float)geneticData[0].Data["MountainLevel"].GetValue(),
                NumberOfOctaves = (int)geneticData[0].Data["Octaves"].GetValue(),
                Spacing = 2.0f,
                WaterLevel = (float)geneticData[0].Data["WaterLevel"].GetValue(),
                Wavelength = (float)geneticData[0].Data["WaveLength"].GetValue(),
                Width = 255,
                XFrequency = (float)geneticData[0].Data["XFrequency"].GetValue(),
                ZFrequency = (float)geneticData[0].Data["ZFrequency"].GetValue()
            };

            var heightMapGenerationResult = _heightMapGenerator.GenerateHeightMap(mapGenerationParameters);
            
            var result = new ExpressedMapData
            {
                Data = new Dictionary<string, object>
                {
                    { "DataPoints", heightMapGenerationResult.DataPoints },
                    { "VertexCount", heightMapGenerationResult.VertexCount },
                    { "Tris", heightMapGenerationResult.Tris },
                    { "HighestVertex", heightMapGenerationResult.HighestVertex },
                    { "SumOfAllVertexHeights", heightMapGenerationResult.SumOfAllVertexHeights },
                    { "Verts", heightMapGenerationResult.Verts },
                    { "WaterHeight", heightMapGenerationResult.WaterHeight },
                    { "MountainHeight", heightMapGenerationResult.MountainHeight }
                }
            };
            result.SaveValues();
            return result;
        }
    }

    public class SettlementGeneExpressor : IGeneExpressor
    {
        public Task<IExpressedGeneData> Express(List<IGeneticData> geneticData)
        {
            throw new System.NotImplementedException();
        }
    }
}
