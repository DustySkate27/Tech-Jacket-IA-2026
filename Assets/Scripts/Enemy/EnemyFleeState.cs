using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFleeState<T> : State<T>
{
    private EnemyFSM fsm;
    public EnemyFleeState(EnemyFSM fsm, StateMachine<T> sm) : base(sm)
    {
        this.fsm = fsm;
    }

    public override void Execute()
    {
        base.Execute();
    }

}
