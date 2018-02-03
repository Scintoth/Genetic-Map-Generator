using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Reflection;
using Zenject;
using Assets.DependencyInjection;

namespace Assets.Scripts.GAEngine.GeneticAlgorithm.Chromosome
{
    class ChromosomeFactory : MonoBehaviour
    {
        public ZenjectContainer _diContainer;
        public static ChromosomeFactory CF;
        public List<ChromosomeData> Chromosomes;

        private void Awake()
        {
            CF = this;
        }

        public GameObject GetGameObjectForName(string name)
        {
            var gameObject = Chromosomes.FirstOrDefault(c => c.Name == name).BaseObject;
            return _diContainer.GetContainer().InstantiatePrefab(gameObject);
        }

        public GameObject GetGameObjectWithParameters(string name, GameObjectCreationParameters creationParameters)
        {
            var gameObject = Chromosomes.FirstOrDefault(c => c.Name == name).BaseObject;
            return _diContainer.GetContainer().InstantiatePrefab(gameObject, creationParameters);
        }
    }

    [Serializable]
    public class ChromosomeData
    {
        public string Name;
        public GameObject BaseObject;
    }
}
