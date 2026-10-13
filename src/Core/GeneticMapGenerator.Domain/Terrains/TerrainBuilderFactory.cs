namespace GeneticMapGenerator.Domain.Terrains
{
    /// <summary>Creates terrain builders for a given grid; lets callers choose resolution per request.</summary>
    public interface ITerrainBuilderFactory
    {
        ITerrainBuilder Create(TerrainGridSettings grid, TerrainAnalysisSettings analysis);
    }

    public sealed class DefaultTerrainBuilderFactory : ITerrainBuilderFactory
    {
        public ITerrainBuilder Create(TerrainGridSettings grid, TerrainAnalysisSettings analysis) =>
            TerrainBuilder.CreateDefault(grid, analysis);
    }
}
