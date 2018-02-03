using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.GAEngine.GeneticAlgorithm.Selection
{
    public class SelectionFactory
    {
        public ISelectionMethod Create(MethodOfSelection methodOfSelection)
        {
            switch (methodOfSelection)
            {
                case MethodOfSelection.TwoFittest:
                    return new TwoFittest();
                case MethodOfSelection.FittestAndRandom:
                    return new FittestAndRandom();
                case MethodOfSelection.BestAndWorst:
                    return new BestAndWorst();
                case MethodOfSelection.Stochastic:
                    return new Stochastic();
                default:
                    return null;
            }
        }
    }
}
