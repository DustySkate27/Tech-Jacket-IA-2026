using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyEvadeState : State<EnemyStates>
{
    private EnemyFSM fsm;
    public EnemyEvadeState(EnemyFSM fsm, StateMachine<EnemyStates> sm) : base(sm)
    {
        this.fsm = fsm;
    }

    public override void Execute()
    {
        base.Execute();
    }

}
