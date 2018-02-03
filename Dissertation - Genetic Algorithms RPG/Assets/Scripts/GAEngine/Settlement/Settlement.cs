
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settlement : MonoBehaviour, IChromosome
{
    #region Chromosome Implementation
    public float Fitness
    {
        get;
        set;
    }

    public List<GeneticData> GeneData
    {
        get;
        set;
    }

    public List<int> Genes
    {
        get;
        set;
    }

    public string Name { get; set; }
    #endregion

    #region Specific Genes
    public float fitVal;
    public float Area;
    public int BuildingsToMake;
    public float TargetProsperity;
    public int TargetNumInhabitants;
    public float Connectedness;
    #endregion

    #region related variables
    Mesh viableArea;

    public float unusedArea;
    public float Radius;
    public Vector3 Centre;
    public int CenreIndex;
    public List<Building> Buildings = new List<Building>();
    public List<Citizen> AllCitizens = new List<Citizen>();
    public int UnusedInhabitants;
    public float UnusedProsperity;
    #endregion

    #region Fitness
    public float areaFitness;
    public float prosperityFitness;
    public float inhabitantsPerBuildingFitness;
    public float buildingNumFitness;
    #endregion


    // PROSPERITY AND CITIZENS NEED TO BE RECALCULATED
    public void CalculateFitness()
    {
        float totalBuildingArea = 0;
        float totalProsperity = 0;
        int totalCitizens = 0;
        for(int i = 0; i < Buildings.Count; ++i)
        {
            totalBuildingArea += Buildings[i].Area;
            totalProsperity += Buildings[i].Prosperity;
            totalCitizens += Buildings[i].Citizens.Count;
        }
        //print("Building Area:" + totalBuildingArea + ", Prosperity: " + totalProsperity + ", Total Citizens: " + totalCitizens);
        //GeneData.Add(new GeneticData(totalProsperity));
        //GeneData.Add(new GeneticData(totalCitizensPerBuilding));
        areaFitness = 1 - Mathf.Abs(1 - (totalBuildingArea / SettlementParameters.SP.TargetArea));
        //print("Area fitness: " + areaFitness);
        prosperityFitness =  Mathf.Abs(1 - ( totalProsperity / SettlementParameters.SP.TargetProsperity));
        //print("Prosperity fitness: " + prosperityFitness); 
        inhabitantsPerBuildingFitness = 1-Mathf.Abs(1 - ((float)totalCitizens / (float)SettlementParameters.SP.TargetInhabitants));
        //print("inhabitants per building fitness: " + inhabitantsPerBuildingFitness);
        buildingNumFitness = 1 - Mathf.Abs(1- ((float)Buildings.Count / (float)SettlementParameters.SP.TargetNumBuildings));
        //print("Buildings fitness: " + buildingNumFitness);

        Fitness = GAFunctions.Average(areaFitness, prosperityFitness, inhabitantsPerBuildingFitness, buildingNumFitness) ;
    }

    public List<IChromosome> CrossOver(Settlement partner)
    {
        return null;
    }

    public List<IChromosome> CrossOver(Map a)
    {
        return null;
    }

    public List<IChromosome> CrossOver(IChromosome a)
    {
        List<IChromosome> children = new List<IChromosome>();
        List<GeneticData> child1 = new List<GeneticData>();
        List<GeneticData> child2 = new List<GeneticData>();

        Settlement temp = (Settlement)a;

        if (GAFunctions.FlipACoin())
        {
            child1.Add(new GeneticData(Area));
            child2.Add(new GeneticData(temp.Area));
        }
        else
        {
            child1.Add(new GeneticData(temp.Area));
            child2.Add(new GeneticData(Area));
        }
        if (GAFunctions.FlipACoin())
        {
            child1.Add(new GeneticData(BuildingsToMake));
            child2.Add(new GeneticData(temp.BuildingsToMake));
        }
        else
        {
            child1.Add(new GeneticData(temp.BuildingsToMake));
            child2.Add(new GeneticData(BuildingsToMake));
        }
        if (GAFunctions.FlipACoin())
        {
            child1.Add(new GeneticData(TargetProsperity));
            child2.Add(new GeneticData(temp.TargetProsperity));
        }
        else
        {
            child1.Add(new GeneticData(temp.TargetProsperity));
            child2.Add(new GeneticData(TargetProsperity));
        }
        if (GAFunctions.FlipACoin())
        {
            child1.Add(new GeneticData(TargetNumInhabitants));
            child2.Add(new GeneticData(temp.TargetNumInhabitants));
        }
        else
        {
            child1.Add(new GeneticData(temp.TargetNumInhabitants));
            child2.Add(new GeneticData(TargetNumInhabitants));
        }

        AssignData(child1);
        children.Add(GetComponent<Settlement>());
        AssignData(child2);
        children.Add(GetComponent<Settlement>());
        return children;
    }

    public IChromosome GenerateGene(int length)
    {
        GeneData.Clear();
        Area = Random.Range(9f, 180f);
        GeneData.Add(new GeneticData(Area));
        BuildingsToMake = Random.Range(1, 20);
        GeneData.Add(new GeneticData(BuildingsToMake));
        TargetProsperity = Random.Range(3f, 14f);
        GeneData.Add(new GeneticData(TargetProsperity));
        TargetNumInhabitants = Random.Range(5, 40);
        GeneData.Add(new GeneticData(TargetNumInhabitants));
        GenerateBuildings();
        return this;
    }

    public void AssignData(List<GeneticData> dataToAdd)
    {
        //print("Assign data called");
        Area = (float)dataToAdd[0].DataSet[0];
        BuildingsToMake = (int)dataToAdd[1].DataSet[0];
        TargetProsperity = (float)dataToAdd[2].DataSet[0];
        TargetNumInhabitants = (int)dataToAdd[3].DataSet[0];
        GenerateBuildings();
    }

    public void Mutate(float mutationRate)
    {
        if (Random.Range(0f, 1f) <= mutationRate)
        {
            GeneData[0].DataSet[0] = Random.Range(9f, 180f);
        }
        if (Random.Range(0f, 1f) <= mutationRate)
        {
            GeneData[1].DataSet[0] = Random.Range(1, 20);
        }
        if (Random.Range(0f, 1f) <= mutationRate)
        {
            GeneData[2].DataSet[0] = Random.Range(3f, 14f);
        }
        if (Random.Range(0f, 1f) <= mutationRate)
        {
            GeneData[3].DataSet[0] = Random.Range(1, 40);
        }
    }

    void GenerateBuildings()
    {
        ClearBuildings();
        Initialise(viableArea);
        unusedArea = Area;
        Radius = Mathf.Pow(Area / Mathf.PI, 0.5f);
        UnusedInhabitants = TargetNumInhabitants;
        UnusedProsperity = TargetProsperity;
        Centre = transform.position;
        for(int i = 0; i < BuildingsToMake; ++i)
        {
            Vector3 buildingLocation = Centre + (Random.insideUnitSphere * Radius);
            if(i > 1 && Buildings.Count > 2)
            {
                buildingLocation = Buildings[Random.Range(0, Buildings.Count -1)].transform.position + (Random.insideUnitSphere * Radius * 2);
            }
            //buildingLocation.y = Mathf.Abs(buildingLocation.y);
            if (UnusedProsperity > 0)
            {
                Quaternion buildingRotation = new Quaternion();
                buildingRotation.eulerAngles = new Vector3(0, Random.Range(-180, 180));
                Vector3 Scale = new Vector3(Random.Range(0.5f, 3f), Random.Range(1, 5), Random.Range(0.5f, 3f));
                Buildings.Add(Instantiate(SettlementParameters.SP.BuildingTemplate, buildingLocation, buildingRotation));
                Buildings[i].transform.SetParent(transform, true);
                Buildings[i].gameObject.name = "Building" + (i + 1);
                Buildings[i].Initialise(Scale, this);
            }
        }
    }

    public void Initialise(Mesh viabilityArea)
    {
        viableArea = viabilityArea;
        int centreIndex = Random.Range(0, viabilityArea.triangles.Length - 1);
        transform.position = viabilityArea.vertices[viabilityArea.triangles[centreIndex]];
        CenreIndex = centreIndex;
    }

    void Awake()
    {
    }

    private void OnDestroy()
    {
        ClearBuildings();
    }

    void ClearBuildings()
    {
        for (int i = 0; i < Buildings.Count; ++i)
        {
            Destroy(Buildings[i].gameObject);
            Buildings[i] = null;
        }
        Buildings.Clear();
    }

    // Use this for initialization
    void Start ()
    {
        
    }
	
	// Update is called once per frame
	void Update ()
    {
        fitVal = Fitness;
	}

    public void FindFloor()
    {
        print("Find Floor called");
        for(int i = 0; i < Buildings.Count; ++i)
        {
            Buildings[i].FindFloor();
        }
    }

    public void Initialise()
    {
        GeneData = new List<GeneticData>();
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public List<float> GetFitnessVariables()
    {
        return new List<float>
        {
            areaFitness,
            prosperityFitness,
            inhabitantsPerBuildingFitness,
            buildingNumFitness
        };
    }

    public List<float> GetExpectedFitnesses()
    {
        return new List<float>
        {
            SettlementParameters.SP.TargetArea,
            SettlementParameters.SP.TargetProsperity,
            SettlementParameters.SP.TargetInhabitants,
            SettlementParameters.SP.TargetNumBuildings
        };
    }

    public void Initialise(string name)
    {
        Name = name;
    }
}
