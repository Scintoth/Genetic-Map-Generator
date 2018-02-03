using Assets.Calculator;
using Assets.DependencyInjection;
using Assets.Scripts.GAEngine.GeneticAlgorithm;
using Assets.Scripts.GAEngine.GeneticAlgorithm.Chromosome;
using Assets.Scripts.GAEngine.GeneticAlgorithm.Engine;
using Assets.Scripts.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class GAStateMachine : MonoBehaviour
{
    public static GAStateMachine GASM;

    [Inject]
    IStateMachine SM;
    public IGeneticAlgorithmEngine MapEngine;
    public IGeneticAlgorithmEngine SettlementEngine;
    public CameraController CC;
    
    [HideInInspector]
    public GameObject FinalMap;
    [HideInInspector]
    public Settlement finalSettlement;

    public SceneContext SceneContext;

    State InitialiseMapEngine = new State("Instantiating Map Engine", new List<Action> { }, new List<Action> { }, new List<Action> { });
    State CreateMap = new State("Map Creation", new List<Action> {  }, new List<Action> { }, new List<Action> { });
    State CreateSettlement = new State("Settlement Generation", new List<Action> { }, new List<Action> { }, new List<Action> { });
    State ViewMap = new State("Viewing Map", new List<Action> { }, new List<Action> { }, new List<Action> { });

    Transition EnginesInstantiated = new Transition();
    Transition MapCompleted = new Transition();
    Transition SettlementComplete = new Transition();

    public BoolCondition EnginesInitialisedCondition = new BoolCondition();
    public BoolCondition MapCompleteCondtion = new BoolCondition();
    public BoolCondition SettlementCompleteCondition = new BoolCondition();

    bool mapGenInitialised;
    bool settlementGenInitialised;

    // Use this for initialization
    void Awake()
    {
        GASM = this;
        
    }

    void Start()
    {
        MapCompleted.Initialise("Map has been made", CreateSettlement, new List<Action> { }, MapCompleteCondtion);
        CreateMap.SetActions(new List<Action> { }, new List<Action> { InitialiseMapGA, MapEngine.GAUpdate }, new List<Action> { GetFinalMap });
        CreateMap.transitions = new List<Transition> { MapCompleted };
        SettlementComplete.Initialise("Settlement has been created", ViewMap, new List<Action> { }, SettlementCompleteCondition);
        CreateSettlement.transitions = new List<Transition> { SettlementComplete };
        CreateSettlement.SetActions(new List<Action> { }, new List<Action> { InitialiseSettlementGA, SettlementEngine.GAUpdate }, new List<Action> { GetFinalSettle });
        ViewMap.SetActions(new List<Action> { }, new List<Action> { CC.CameraControllerUpdate }, new List<Action> { });
        SM.Initialise(CreateMap, false, new List<State> { CreateMap, CreateSettlement, ViewMap });
    }
	
	// Update is called once per frame
	void Update ()
    {
	    foreach(Action action in SM.Update())
        {
            action();
        }
        CheckConditions();
	}

    void CheckConditions()
    {
        MapCompleteCondtion.A = MapEngine.StopGenerator;
        SettlementCompleteCondition.A = SettlementEngine.StopGenerator;
    }



    void InitialiseMapGA()
    {
        if (!mapGenInitialised)
        {
            var baseMap = ChromosomeFactory.CF.GetGameObjectForName("Map");
            var container = SceneContext.Container;
            var mapEngineParameters = new GAEngineParameters
            {
                Container = container,
                FitnessThreshold = 0.9f, // TODO: This needs taking in from the UI
                InitialChromosome = baseMap.GetComponent<IChromosome>(),
                MaxPopulation = 20, //TODO: Get this from UI
                MutationRate = 0.01f, //TODO: Get this from UI
                SelectionMethod = MethodOfSelection.FittestAndRandom, // TODO: Get this from UI
                UseElitism = false // TODO: Get from UI
            };
            MapEngine.Reset();
            MapEngine.SetParameters(mapEngineParameters, new MapFitnessCalculator());
            /*MapEngine.stopGenerator = false;
            MapEngine.Population.Clear();
            MapEngine.bests.Clear();
            MapEngine.worst = null;
            MapEngine.AppliedObject = container.InstantiatePrefab(baseMap).GetComponent<IChromosome>(); // new GameObject();
            MapEngine.AppliedObject.Initialise("Map");
            MapEngine.createdChromosome = null;
            MapEngine.Initial = MapEngine.AppliedObject;*/
            mapGenInitialised = true;
        }
    }

    void InitialiseSettlementGA()
    {
        /*if (!settlementGenInitialised)
        {
            SettlementEngine.stopGenerator = false;
            SettlementEngine.Population.Clear();
            SettlementEngine.bests.Clear();
            SettlementEngine.worst = null;
            SettlementEngine.AppliedObject = new GameObject();
            SettlementEngine.createdChromosome = null;
            //SettlementEngine.AppliedObject.transform.position = finalMap.settleMesh.vertices[Random.Range(0, finalMap.settleMesh.vertices.Length - 1)];

            SettlementEngine.AppliedObject.AddComponent<Settlement>();
            SettlementEngine.AppliedObject.tag = "Settlement";
            SettlementEngine.AppliedObject.GetComponent<Settlement>().Initialise(FinalMap.settleMesh);
            SettlementEngine.Initial = SettlementEngine.AppliedObject.GetComponent<Settlement>();
            settlementGenInitialised = true;
        }*/
    }

    void GetFinalMap()
    {
        FinalMap = MapEngine.GetGameObject();
        FinalMap.GetComponent<MeshCollider>().sharedMesh = FinalMap.GetComponent<Map>().ret;
        FinalMap.gameObject.AddComponent<Rigidbody>();
        FinalMap.gameObject.GetComponent<Rigidbody>().useGravity = false;
        FinalMap.gameObject.GetComponent<Rigidbody>().isKinematic = true;
        MapEngine.GetGameObject().GetComponent<Map>().SetWater();
    }

    void GetFinalSettle()
    {
        finalSettlement = SettlementEngine.GetGameObject().GetComponent<Settlement>();
        finalSettlement.GetComponent<Settlement>().FindFloor();
    }
}
