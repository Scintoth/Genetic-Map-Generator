using Assets.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.GAEngine.Map
{
    interface IHeightCalculator
    {
        float GetHeight(HeightCalculationParameters parameters);
    }
}
