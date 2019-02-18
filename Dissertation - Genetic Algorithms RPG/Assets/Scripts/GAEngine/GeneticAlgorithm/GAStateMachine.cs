using Assets.Calculator;
using Assets.DependencyInjection;
using Assets.Scripts.GAEngine.GeneticAlgorithm;
using Assets.Scripts.GAEngine.GeneticAlgorithm.Chromosome;
using Assets.Scripts.GAEngine.GeneticAlgorithm.Engine;
using Assets.Scripts.GAEngine.GeneticAlgorithm.Selection;
using Assets.Scripts.GAEngine.Map;
using Assets.Scripts.StateMachine;
using GeneticAlgorithmEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using GeneticAlgorithmEngine = Assets.Scripts.GAEngine.GeneticAlgorithm.Engine.GeneticAlgorithmEngine;

public class GAStateMachine : MonoBehaviour
{
    public static GAStateMachine GASM;

    public ZenjectContainer _container;

    [Inject]
    IStateMachine SM;
    [Inject]
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
    /*State CreateSettlement = new State("Settlement Generation", new List<Action> { }, new List<Action> { }, new List<Action> { });*/
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
        SM = _container.GetContainer().Resolve<IStateMachine>();
        //MapEngine = _container.GetContainer().InstantiateComponentOnNewGameObject<GeneticAlgorithmEngine.GeneticAlgorithmEngine>("MapEngine");
        
        //InitialiseMapGA();
        /*var testBaseMap = _container.GetContainer().InstantiateComponentOnNewGameObject<Map>("Test Map");
        testBaseMap.Initialise("Map");*/

        var initialMap = _container.GetContainer().Resolve<IGeneInfo>();// new MapGeneInfo(new MapGeneExpressor(new HeightMapGenerator(new PerlinHeightCalculator())));

        initialMap.Initialise();

        var testParameters = new GAEngineParameters
        {
            Container = _container.GetContainer(),
            FitnessThreshold = 0.9f,
            InitialChromosome = initialMap,
            MutationRate = 0.01f,
            UseElitism = false,
            MaxPopulation = 20,
            SelectionMethod = MethodOfSelection.FittestAndRandom
        };
        MapEngine = _container.GetContainer().Resolve<IGeneticAlgorithmEngine>();
        MapEngine.SetParameters(testParameters); // TODO: Implement this from the UI
        //SettlementEngine = _container.GetContainer().InstantiateComponentOnNewGameObject<GAEngine>("SettlementEngine");
        //SettlementEngine.SetParameters(); // TODO: Implement this from the UI
        EnginesInitialisedCondition.A = IsMapGenInitialised;
        MapCompleted.Initialise("Map has been made", /*CreateSettlement*/ViewMap, new List<Action> { }, MapCompleteCondtion);
        CreateMap.SetActions(new List<Action> { }, new List<Action> { InitialiseMapGa, () => { MapEngine.Update(); } }, new List<Action> { GetFinalMap });
        CreateMap.transitions = new List<Transition> { MapCompleted };
        SettlementComplete.Initialise("Settlement has been created", ViewMap, new List<Action> { }, SettlementCompleteCondition);
        //CreateSettlement.transitions = new List<Transition> { SettlementComplete };
        //CreateSettlement.SetActions(new List<Action> { }, new List<Action> { InitialiseSettlementGA, SettlementEngine.Update }, new List<Action> { GetFinalSettle });
        ViewMap.SetActions(new List<Action> { }, new List<Action> { CC.CameraControllerUpdate }, new List<Action> { });
        SM.Initialise(CreateMap, false, new List<State> { CreateMap, /*CreateSettlement,*/ ViewMap });
        CheckConditions();
    }
	
	// Update is called once per frame
	void Update ()
    {
	    foreach(Action action in SM.Update())
        {
            action();
        }
	}

    void CheckConditions()
    {
        MapCompleteCondtion.A = MapEngine.StopGenerator;
        //SettlementCompleteCondition.A = SettlementEngine.StopGenerator;
    }

    void InitialiseMapGa()
    {
        if (!mapGenInitialised)
        {
            var baseMap = ChromosomeFactory.CF.GetGameObjectForName("Map");
            var container = SceneContext.Container;
            var mapEngineParameters = new GAEngineParameters
            {
                Container = container,
                FitnessThreshold = 0.9f, // TODO: This needs taking in from the UI
                InitialChromosome = baseMap.GetComponent<IGeneInfo>(),
                MaxPopulation = 20, //TODO: Get this from UI
                MutationRate = 0.01f, //TODO: Get this from UI
                SelectionMethod = MethodOfSelection.FittestAndRandom, // TODO: Get this from UI
                UseElitism = false,  // TODO: Get from UI
                NameOfObject = "Map"
            };
            MapEngine.Reset();
            MapEngine.SetParameters(mapEngineParameters);
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
        FinalMap.GetComponent<Map>().Initialise((ExpressedMapData)MapEngine.GetFittest(), "Map");
        //FinalMap.GetComponent<MeshCollider>().sharedMesh = FinalMap.GetComponent<Map>().ret;
        FinalMap.gameObject.AddComponent<Rigidbody>();
        FinalMap.gameObject.GetComponent<Rigidbody>().useGravity = false;
        FinalMap.gameObject.GetComponent<Rigidbody>().isKinematic = true;
        FinalMap.gameObject.transform.Rotate(180, 0, 0);
        //MapEngine.GetGameObject().GetComponent<Map>().SetWater();
    }

    void GetFinalSettle()
    {
        finalSettlement = SettlementEngine.GetGameObject().GetComponent<Settlement>();
        finalSettlement.GetComponent<Settlement>().FindFloor();
    }

    bool IsMapGenInitialised()
    {
        return mapGenInitialised;
    }
}
