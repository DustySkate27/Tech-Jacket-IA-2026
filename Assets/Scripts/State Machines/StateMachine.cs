using System.Collections.Generic;
using UnityEngine;

public class StateMachine<T>
{
    private IState<T> currentState;
    private Dictionary<T, IState<T>> states = new();

    public IState<T> CurrentState => currentState;


    public void SetCurrent(IState<T> state)
    {
        currentState = state;
    }

    public void AddState(IState<T> state, T stateValue)
    {
        states.Add(stateValue, state);
    }

    public void Update() => currentState.Execute();

    public void ChangeState(T newState)
    {
        if (states.TryGetValue(newState, out IState<T> stateValue))
        {
            currentState.Sleep();
            currentState = stateValue;
            currentState.Awake();
        }
    }
}
