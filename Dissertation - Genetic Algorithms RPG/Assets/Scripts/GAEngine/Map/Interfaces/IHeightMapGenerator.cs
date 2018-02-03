using Assets.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.GAEngine.Map
{
    public interface IHeightMapGenerator
    {
        MapGenerationResult GenerateHeightMap(MapGenerationParameters parameters);
    }
}
