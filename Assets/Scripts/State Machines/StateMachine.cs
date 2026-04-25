using System.Collections.Generic;
using UnityEngine;

public class StateMachine<T>
{
    private IState<T> currentState;
    public IState<T> CurrentState => currentState;


    public void SetCurrent(IState<T> state)
    {
        currentState = state;
    }

    public void Update() => currentState.Execute();

    public void ChangeState(T input)
    {
        var newState = currentState.GetTransition(input);
        if (newState != null)
        {
            currentState.Sleep();
            currentState = newState;
            currentState.Awake();
        }
    }
}
