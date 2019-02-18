using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assets.Scripts.GAEngine.GeneticAlgorithm.Chromosome;
using Assets.Scripts.GAEngine.GeneticAlgorithm.Selection;
using Assets.Scripts.Utilities;
using GeneticAlgorithmEngine;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.GAEngine.GeneticAlgorithm.Engine
{
    public class GeneticAlgorithmEngine : IGeneticAlgorithmEngine
    {
        private IGeneExpressor _geneExpressor;
        private IFitnessCalculator _fitnessCalculator;
        private IExpectedFitnessRetriever _fitnessRetriever;

        private List<IGeneInfo> _population = new List<IGeneInfo>();

        private int _maxPopulation;
        private float _fitnessThreshold;
        private IGeneInfo _initialChromosome;
        private bool _useElitism;
        private MethodOfSelection _methodOfSelection;
        private float _mutationRate;
        private DiContainer _container;
        private string _nameOfObject;

        private IGeneInfo _fittestMember;

        public bool _stopGenerator;

        public GeneticAlgorithmEngine(IGeneExpressor geneExpressor, IFitnessCalculator fitnessCalculator, IExpectedFitnessRetriever expectedFitnessRetriever)
        {
            _geneExpressor = geneExpressor;
            _fitnessCalculator = fitnessCalculator;
            _fitnessRetriever = expectedFitnessRetriever;
        }

        public IGeneInfo Update()
        {
            if (_stopGenerator) return _fittestMember;

            if (_population.Count == 0)
            {
                for (var i = 0; i < _maxPopulation; i++)
                {
                    _population.Add(new MapGeneInfo(_geneExpressor, _fitnessCalculator, new MapExpectedFitnessRetriever()));
                    _population.Last().Initialise();
                }
            }

            foreach(var individual in _population)
            {
                if (!individual.Expressed)
                {
                    var expressionTask = Task.Run(() => _geneExpressor.Express(individual.GeneticData));

                    expressionTask.Wait();
                    individual.ExpressedGeneticData = expressionTask.Result;
                    individual.Expressed = true;
                    individual.Fitness = _fitnessCalculator.CalculateFitness(individual.ExpressedGeneticData);
                }
            }

            var random = RandomService.GetRandomNumberGenerator();
            var killRate = (double)random.Next(0, 5) / 100;
            for (var i = 0; i < _population.Count; i++)
            {
                if(_population[i].Fitness < killRate)
                {
                    _population[i] = null;
                    _population.RemoveAt(i);
                    i--;
                }
            }

            if (_population.Count == 0)
                return null;

            if (_population.Count == 1)
            {
                _fittestMember = _population[0];
                return _fittestMember;
            }

            var selector = SelectionFactory.Create(_methodOfSelection);

            var selectedPopulation = selector.Select(_population);

            var selectedCount = selectedPopulation.Count;
            while(_population.Count < _maxPopulation)
            {
                var firstPopulation = random.Next(0, selectedCount);
                var secondPopulation = random.Next(0, selectedCount);
                while(secondPopulation == firstPopulation)
                {
                    secondPopulation = random.Next(0, selectedCount);
                }
                var offspring = selectedPopulation[firstPopulation].Crossover(selectedPopulation[secondPopulation]);
                _population.Add(offspring);
            }
            _population = _population.OrderBy(p => p.Fitness).ToList();

            _fittestMember = _population.Last();
            if (_fittestMember.Fitness > _fitnessThreshold)
                _stopGenerator = true;

            return _fittestMember;
            // if population == 0, generate new population
            // Send population to gene expressor if it has not already been expressed
            // Evaluate fitness of the expressed gene data
            // Sort the population by fitness
            // Kill off a number of the population with the chance of death being the inverse of the fitness
            // Crossover chromosomes until population is full
            // Return fittest Genetic data
        }

        public GameObject GetGameObject()
        {
            return ChromosomeFactory.CF.GetGameObjectForName(_nameOfObject);
            throw new System.NotImplementedException();
        }

        public IExpressedGeneData GetFittest()
        {
            return _fittestMember.ExpressedGeneticData;
        }

        public void Reset()
        {
            _maxPopulation = 0;
            _fitnessThreshold = 0;
            _initialChromosome = null;
            _useElitism = false;
            _methodOfSelection = 0;
            _mutationRate = 0;
            _nameOfObject = "";
            _population.Clear();
        }

        public void SetParameters(GAEngineParameters parameters)
        {
            /*_fitnessCalculator = fitnessCalculator;
            _geneExpressor = geneExpressor;*/
            _maxPopulation = parameters.MaxPopulation;
            _fitnessThreshold = parameters.FitnessThreshold;
            _initialChromosome = parameters.InitialChromosome;
            _useElitism = parameters.UseElitism;
            _methodOfSelection = parameters.SelectionMethod;
            _mutationRate = parameters.MutationRate;
            _nameOfObject = parameters.NameOfObject;
        }

        public bool StopGenerator()
        {
            Console.WriteLine("Generator stopped!");
            return _stopGenerator;
        }
    }

    public interface IGeneInfo
    {
        List<IGeneticData> GeneticData { get; set; }
        IExpressedGeneData ExpressedGeneticData { get; set; }
        bool Expressed { get; set; }
        float Fitness { get; set; }

        IGeneInfo Crossover(IGeneInfo partner);
        void Initialise();
        Task Express();
        void CalculateFitness();
    }

    public class MapGeneInfo : IGeneInfo
    {
        private IGeneExpressor _geneExpressor;
        private IFitnessCalculator _fitnessCalculator;
        private IExpectedFitnessRetriever _fitnessValueRetriever;

        public List<IGeneticData> GeneticData { get; set; } = new List<IGeneticData>();
        public IExpressedGeneData ExpressedGeneticData { get; set; }
        public bool Expressed { get; set; } = false;
        public float Fitness { get; set; }

        public MapGeneInfo(IGeneExpressor geneExpressor, IFitnessCalculator fitnessCalculator, IExpectedFitnessRetriever fitnessValueRetriever)
        {
            _geneExpressor = geneExpressor;
            _fitnessCalculator = fitnessCalculator;
            _fitnessValueRetriever = fitnessValueRetriever;
        }

        public IGeneInfo Crossover(IGeneInfo partner)
        {
            var random = RandomService.GetRandomNumberGenerator();

            var result = new MapGeneInfo(_geneExpressor, _fitnessCalculator, _fitnessValueRetriever);
            result.GeneticData.Add(new HeightMapParameters());
            foreach (var datum in GeneticData[0].Data)
            {
                if (random.Next(0, 100) < 50)
                {
                    result.GeneticData[0].Data.Add(datum.Key,datum.Value);
                    continue;
                }

                result.GeneticData[0].Data.Add(datum.Key, partner.GeneticData[0].Data[datum.Key]);
            }

            return result;
            throw new NotImplementedException();
        }

        public void Initialise()
        {
            var mapFieldsType = typeof(HeightMapFields);
            GeneticData.Add(new HeightMapParameters());
            GeneticData[0].Initialise();
        }

        public async Task Express()
        {
            ExpressedGeneticData = await _geneExpressor.Express(GeneticData);

            Expressed = true;
        }

        public void CalculateFitness()
        {
            Fitness = _fitnessCalculator.CalculateFitness(ExpressedGeneticData);
            //throw new NotImplementedException();
        }
    }
}
