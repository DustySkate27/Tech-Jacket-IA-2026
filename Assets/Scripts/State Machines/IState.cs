using UnityEngine;

public interface IState<T>
{
    public void Awake();
    public void Execute();
    public void Sleep();

    public void AddTransition( IState<T> state, T input);
    IState<T> GetTransition(T input);
}
