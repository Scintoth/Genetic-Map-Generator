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
using Zenject;

namespace Assets.DependencyInjection
{
    public class ZenjectContainer : MonoInstaller
    {
        DiContainer container;

        public override void InstallBindings()
        {
            Container.Bind<IHeightCalculator>().To<PerlinHeightCalculator>().AsTransient();
            Container.Bind<IHeightMapGenerator>().To<HeightMapGenerator>().AsTransient();
            Container.Bind<IFitnessCalculator>().To<MapFitnessCalculator>().AsTransient();
            Container.Bind<IGeneticAlgorithmEngine>().To<GAEngine>().AsTransient();
            Container.Bind<IStateMachine>().To<StateMachine>().AsTransient();
            container = Container;
        }

        public DiContainer GetContainer()
        {
            return container;
        }
    }
}
