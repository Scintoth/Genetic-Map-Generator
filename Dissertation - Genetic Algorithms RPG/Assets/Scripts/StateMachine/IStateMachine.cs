using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.StateMachine
{
    public interface IStateMachine
    {
        void Initialise(State initial, bool memoryEnabled, List<State> possibleStates);
        List<Action> Update();
    }
}