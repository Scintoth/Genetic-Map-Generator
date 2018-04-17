using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.GAEngine.Map.Interfaces
{
    interface IMap : IChromosome
    {
        /*
         * Needs two methods, one returning a result that contains the data for a map, that being
         * Vertex data, triangle data, uvs1-4, a dictionary of key "LandType" (enum) and value int, average displacement from the average height of all vertices (flatness), and the settlemesh.
         * Other method takes in these values and applies the following to the Map, which will need to be done in the main thread.
         * Probably best to create a whole new implementation of Map at this rate
         */


    }
}
