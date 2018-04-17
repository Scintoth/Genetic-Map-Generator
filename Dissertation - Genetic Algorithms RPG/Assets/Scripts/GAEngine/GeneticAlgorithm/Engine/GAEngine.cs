using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Assets.Scripts.GAEngine.GeneticAlgorithm;
using Zenject;
using Assets.DependencyInjection;
using Assets.Scripts.GAEngine.GeneticAlgorithm.Selection;
using Assets.Scripts.GAEngine.GeneticAlgorithm.Engine;
using Assets.Scripts.GAEngine.GeneticAlgorithm.Chromosome;
using System.Threading.Tasks;

public delegate List<IChromosome> SelectionMethod();

public class GAEngine : MonoBehaviour, IGeneticAlgorithmEngine 
{
    IFitnessCalculator _fitnessCalculator;

    SelectionFactory _selectionFactory;
    MethodOfSelection _methodOfSelection;
    DiContainer _container;

    float _mutationRate;

    // GA Core Variables
    int MaxPopulation;
    [Range(0.6f, 0.99f)]
    float FitnessThreshold;
    int Generation;

    IChromosome Initial;

    IChromosome AppliedObject;
    List<IChromosome> bests = new List<IChromosome>();
    IChromosome worst;

    // Created chromosomes
    List<IChromosome> createdChromosome;
    List<IChromosome> Population = new List<IChromosome>();
    List<GameObject> created = new List<GameObject>();

     List<IChromosome> SelectedPopulation;

    bool stopGenerator;

    // Elitism
    bool UseElitism;
    List<IChromosome> elite = new List<IChromosome>();
    
    public void Selection(MethodOfSelection method)
    {
        var selectionMethod = _selectionFactory.Create(method);

        var SelectedPopulation = selectionMethod.Select(Population);
        //AppliedObject.GetComponent<Map>() = bests[0];
    }

    // Selection Methods
    // TwoFittest, Fittest, BestAndWorst
    public List<IChromosome> TwoFittest()
    {
        if (bests.Count > 0)
            return bests[0].CrossOver(bests[1]);
        else
            return null;
    }

    public List<IChromosome> Fittest()
    {
        return bests;
    }

    public List<IChromosome> BestAndWorst()
    {
        return bests[0].CrossOver(worst);
    }

    public List<IChromosome> BreedingWheel()
    {
        List<IChromosome> pickedChromosomes = new List<IChromosome>();
        float totalFitness = 0;
        List<float> individualFitnesses = new List<float>();
        List<float> moddedFitness = new List<float>();
        for (int i = 0; i < Population.Count; ++i)
        {
            totalFitness += Population[i].Fitness;
            individualFitnesses.Add(Population[i].Fitness);
        }
        for (int i = 0; i < individualFitnesses.Count; ++i)
        {
            moddedFitness.Add(individualFitnesses[i] / totalFitness);
        }
        while (pickedChromosomes.Count < 2)
        {
            for (int i = 0; i < moddedFitness.Count; ++i)
            {
                if (moddedFitness[i] < Random.Range(0f, 1f))
                {
                    pickedChromosomes.Add(Population[i]);
                }
            }
        }

        return pickedChromosomes[0].CrossOver(pickedChromosomes[1]);
    }

    void CalculateFitness()
    {
        bests.Clear();
        float fitComparison = 0;
        float worstComparison = 1;
        for (int i = 0; i < Population.Count; ++i)
        {
            var chromosomeFitnessValues = Population[i].GetFitnessVariables();
            var chromosomeExpectedFitnessValues = Population[i].GetExpectedFitnesses();

            Debug.Log(chromosomeFitnessValues.ToString());
            Debug.Log(chromosomeExpectedFitnessValues.ToString());

            Population[i].Fitness = _fitnessCalculator.CalculateFitness(chromosomeFitnessValues, chromosomeExpectedFitnessValues);

            if (Population[i].Fitness >= fitComparison)
            {
                fitComparison = Population[i].Fitness;
                bests.Add(Population[i]);
            }

            if (Population[i].Fitness < worstComparison)
            {
                worstComparison = Population[i].Fitness;
                worst = Population[i];
            }
        }
        if (bests.Count < 2)
        {
            bests.Clear();
            Population.Sort((x, y) => x.Fitness.CompareTo(y.Fitness));
            for (int i = 0; i < 3; ++i)
            {
                if (Population.Count > 0)
                    bests.Add(Population[i]);
            }
        }

        bests.Sort((x, y) => x.Fitness.CompareTo(y.Fitness));
    }

    void Generate(List<IChromosome> createdOffspring, float mutationRate)
    {
        for (int i = 0; i < Population.Count; ++i)
        {
            Population[i] = null;
            //Destroy(Population[i]);
        }
        Population.Clear();
        var populationLock = new object();
        if (createdOffspring == null)
        {
            //StartCoroutine("CreateNewGenes");
            Parallel.For(0, MaxPopulation - 1, (i) =>
            //for (int i = 0; i < MaxPopulation; ++i)
            {
                var temp = Initial.GenerateGene(20); /*ChromosomeFactory.CF.GetGameObjectForName(Initial.Name);*/
                lock (populationLock)
                {
                    created.Add(temp.GetGameObject());
                    Population.Add(Initial.GenerateGene(10));
                }
            });
        }
        else
        {
            Parallel.For(0, MaxPopulation, (i) =>
            //for (int i = 0; i < MaxPopulation; ++i)
            {
                lock (populationLock)
                {
                    Population.AddRange(createdOffspring);
                    Population[i].Mutate(mutationRate);
                }
            });
        }
    }

    IEnumerable CreateNewGenes()
    {
        for (int i = 0; i < MaxPopulation; ++i)
        {
            var temp = Initial.GenerateGene(20); /*ChromosomeFactory.CF.GetGameObjectForName(Initial.Name)*/;
                created.Add(temp.GetGameObject());
                Population.Add(Initial.GenerateGene(10));
            yield return null;
        }
    }

    /*void Generate(List<Map> createdOffspring, float mutationRate)
    {
        ClearPopulation();

        Population.Clear();
        if (createdOffspring == null)
        {
            for (int i = 0; i < MaxPopulation; ++i)
            {
                var temp = ChromosomeFactory.CF.GetGameObjectForName(chromo)
                //_container.InstantiatePrefab(BasePrefab);
                // Create a map factory
                /*GameObject temp = ; //new GameObject("Map " + i );
                temp.transform.position = MapParameters.MP.Water.transform.position;
                temp.AddComponent<Map>();
                created.Add(temp);
                Population.Add(created[i].GetComponent<Map>());
            }
        }
        else
        {
            for (int i = 0; i < MaxPopulation; ++i)
            {
                for (int j = 0; j < createdOffspring.Count; ++j)
                {
                    Population.Add(createdOffspring[j]);
                }
                Population[i].Mutate(mutationRate);
            }
        }
    }*/

    void Generate(List<Settlement> createdOffspring, float mutationRate)
    {
        ClearPopulation();

        Population.Clear();
        if (createdOffspring == null)
        {
            for (int i = 0; i < MaxPopulation; ++i)
            {
                GameObject temp = new GameObject("Settlement " + i);
                temp.AddComponent<Settlement>();
                temp.GetComponent<Settlement>().Initialise(GAStateMachine.GASM.FinalMap.GetComponent<Map>().settleMesh);
                created.Add(temp);
                Population.Add(created[i].GetComponent<Settlement>());
            }
        }
        else
        {
            for (int i = 0; i < MaxPopulation; ++i)
            {
                for (int j = 0; j < createdOffspring.Count; ++j)
                {
                    createdOffspring[j].Initialise(GAStateMachine.GASM.FinalMap.GetComponent<Map>().settleMesh);
                    Population.Add(createdOffspring[j]);
                }
                Population[i].Mutate(mutationRate);
            }
        }
    }

    public void UpdateGA(SelectionMethod selMethod, float mutationRate)
    {
        if (!stopGenerator)
        {
            
            //Selection(selMethod);

            Generate(createdChromosome, mutationRate);

            CalculateFitness();

            ClearPopulation();

            CheckDone();

            if(AppliedObject == null)
            {
                AppliedObject = bests[0];
            }
            AppliedObject.AssignData(bests[0].GeneData);

            Generation++;
        }
    }

    void CheckDone()
    {
        if (bests[0].Fitness > FitnessThreshold)
        {
            stopGenerator = true;
            AppliedObject.AssignData(bests[0].GeneData);
            /*if (AppliedObject.tag == "Map")
            {
                print("map generated");
                AppliedObject.GetComponent<Map>().AssignData(bests[0].GeneData);
                AppliedObject.GetComponent<Map>().SetWater();
            }
            if (AppliedObject.tag == "Settlement")
            {
                AppliedObject.GetComponent<Settlement>().AssignData(bests[0].GeneData);
            }*/
            //ClearMaps();
        }
    }

    void ClearPopulation()
    {
        for (int i = 0; i < created.Count; ++i)
        {
            Destroy(created[i]);
        }
        created.Clear();
        Population.Clear();

    }
    
    // Use this for initialization
    void Start()
    {

    }
    
    public void GAUpdate()
    {
        if (Initial != null)
        {
            UpdateGA(TwoFittest, 0.01f);
        }
        else
        {
            print("Initial needs to be set for GA to run.");
        }
        //throw new System.NotImplementedException();
    }

    void IGeneticAlgorithmEngine.GAUpdate()
    {
        if (!stopGenerator)
        {

            //Selection(selMethod);

            Generate(createdChromosome, _mutationRate);

            CalculateFitness();

            ClearPopulation();

            CheckDone();
            if (AppliedObject == null)
            {
                AppliedObject = bests[0];
            }
            AppliedObject.AssignData(bests[0].GeneData);

            /*if (AppliedObject.tag == "Map")
            {
                AppliedObject.AssignData(bests[0].GeneData);
                AppliedObject.GetComponent<Map>().SetWater();
                AppliedObject.gameObject.name = "Map";
            }
            if (AppliedObject.tag == "Settlement")
            {
                AppliedObject.GetComponent<Settlement>().AssignData(bests[0].GeneData);
                AppliedObject.gameObject.name = "Settlement";
            }*/
            Generation++;
        }
    }

    bool IGeneticAlgorithmEngine.StopGenerator()
    {
        return stopGenerator;
    }

    void IGeneticAlgorithmEngine.Reset()
    {
        stopGenerator = false;
        Population.Clear();
        bests.Clear();
        worst = null;
        createdChromosome = null;
        Initial = null;
    }

    public void SetParameters(GAEngineParameters parameters, IFitnessCalculator fitnessCalculator)
    {
        _fitnessCalculator = fitnessCalculator;
        _methodOfSelection = parameters.SelectionMethod;
        UseElitism = parameters.UseElitism;
        Initial = parameters.InitialChromosome;
        _container = parameters.Container;
        _mutationRate = parameters.MutationRate;
        FitnessThreshold = parameters.FitnessThreshold;
        MaxPopulation = parameters.MaxPopulation;
    }

    GameObject IGeneticAlgorithmEngine.GetGameObject()
    {
        return AppliedObject.GetGameObject();
    }
}