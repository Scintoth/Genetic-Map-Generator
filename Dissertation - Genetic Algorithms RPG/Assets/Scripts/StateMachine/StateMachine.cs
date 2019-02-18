using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.StateMachine;

public delegate void Action();

public class StateMachine : IStateMachine 
{
    public List<State> Memory = new List<State>();
    public List<State> states;
    public State initialState;
    public State currentState;
    Transition triggeredTransition;
    State targetState;
    bool useMemory;

    public StateMachine() { }

    public void Initialise(State initial, bool memoryEnabled, List<State> possiblestates)
    {
        currentState = initialState = initial;
        states = possiblestates;
        useMemory = memoryEnabled;
    }

    public void Initialise(State initial, bool memoryEnabled, params State[] possibleStates)
    {
        currentState = initialState = initial;
        states.Clear();
        states.AddRange(possibleStates);
        useMemory = memoryEnabled;
    }

    public List<Action> Update()
    {
        triggeredTransition = null;

        if (currentState != null)
        {
            if (currentState.transitions != null)
            {
                if (currentState.transitions.Count > 0)
                {
                    foreach (Transition transition in currentState.transitions)
                    {
                        if (transition.isTriggered)
                        {
                            triggeredTransition = transition;
                            break;
                        }
                    }
                }
            }

            if (triggeredTransition != null)
            {
                if(useMemory) Memory.Add(currentState);
                targetState = triggeredTransition.TargetState;
                List<Action> actions = new List<Action>();
                actions.AddRange(currentState.exitAction);

                actions.AddRange(triggeredTransition.Action);
                actions.AddRange(targetState.entryAction);
                if (currentState.transitions.Count > 0)
                {
                    for (int i = 0; i < currentState.transitions.Count; ++i)
                    {
                        currentState.transitions[i].useForceTrue = false;
                    }
                    if (targetState != null)
                    {
                        if (targetState.transitions != null)
                        {
                            for (int i = 0; i < targetState.transitions.Count; ++i)
                            {
                                targetState.transitions[i].useForceTrue = false;
                            }
                        }
                    }
                }
                currentState = states.Find(x => x.name == targetState.name);
                return actions;
            }
            else
            {
                return currentState.action;
            }
        }
        return null;
    }
}

public class State
{
    public string name;
    public List<Action> entryAction = new List<Action>();
    public List<Action> action = new List<Action>();
    public List<Action> exitAction = new List<Action>();
    public List<Transition> transitions = new List<Transition>();

    public State(string Name, List<Action> EntryActions, List<Action> RunTimeActions, List<Action> ExitActions)
    {
        name = Name;
        entryAction = EntryActions;
        action = RunTimeActions;
        exitAction = ExitActions;
    }

    public void SetTransitions(params Transition[] Transitions)
    {
        transitions.Clear();
        transitions.AddRange(Transitions);
    }

    public void SetActions(List<Action> Entry, List<Action> RunTime, List<Action> Exit)
    {
        entryAction = Entry;
        action = RunTime;
        exitAction = Exit;
    }
}

public class Transition
{
    public string Name;
    State targetState;
    List<Action> actions = new List<Action>();
    public ICondition condition;
    public bool useForceTrue;

    public void Initialise(string name, State Target, List<Action> triggerActions, ICondition Condition)
    {
        Name = name;
        targetState = Target;
        actions = triggerActions;
        condition = Condition;
    }

    public bool isTriggered
    {
        get
        {
            return condition.Test() || condition.ForceBool(useForceTrue);
        }
    }

    public List<Action> Action
    {
        get { return actions; }
        set { actions = value; }
    }

    public State TargetState
    {
        get { return targetState;}
        set { targetState = value; }
    }
}
