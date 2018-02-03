using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;


public interface IChromosome
{
    string Name { get; set; }
    List<GeneticData> GeneData { get; set; }
    float Fitness { get; set; }
    List<int> Genes { get; set; }

    void AssignData(List<GeneticData> geneticData);
    IChromosome GenerateGene(int length);
    void Mutate(float mutationRate);    
    List<IChromosome> CrossOver(IChromosome a);
    void Initialise(string name);
    GameObject GetGameObject();
    List<float> GetFitnessVariables();
    List<float> GetExpectedFitnesses();
}

public class GeneticData
{
    public List<object> DataSet { get; set; } = new List<object>();
    public bool IsDominant { get; set; }
    public float DominanceWeighting { get; set; }

    public GeneticData(List<object> dataToAdd)
    {
        DataSet.AddRange(dataToAdd);
    }

    public GeneticData(object dataToAdd)
    {
        DataSet.Add(dataToAdd);
    }

    public GeneticData()
    {
    }
}
