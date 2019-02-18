using Assets.Calculator;
using Assets.Scripts.GAEngine.GeneticAlgorithm;
using Assets.Scripts.GAEngine.GeneticAlgorithm.Engine;
using Assets.Scripts.GAEngine.Map;
using Assets.Scripts.StateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Utilities;
using GeneticAlgorithmEngine;
using Zenject;

namespace Assets.DependencyInjection
{
    public class ZenjectContainer : MonoInstaller
    {
        DiContainer _container;

        public override void InstallBindings()
        {
            Container.Bind<IHeightCalculator>().To<PerlinHeightCalculator>().AsTransient();
            Container.Bind<IHeightMapGenerator>().To<HeightMapGenerator>().AsTransient();
            Container.Bind<IFitnessCalculator>().To<MapFitnessCalculator>().AsTransient();
            Container.Bind<IGeneticAlgorithmEngine>().To<Scripts.GAEngine.GeneticAlgorithm.Engine.GeneticAlgorithmEngine>().AsTransient();
            Container.Bind<IStateMachine>().To<StateMachine>().AsTransient();
            Container.Bind<IGeneInfo>().To<MapGeneInfo>().AsTransient();
            Container.Bind<IGeneExpressor>().To<MapGeneExpressor>().AsTransient();
            Container.Bind<IExpectedFitnessRetriever>().To<MapExpectedFitnessRetriever>().AsTransient();

            _container = Container;
        }

        public DiContainer GetContainer()
        {
            return _container;
        }
    }
}
