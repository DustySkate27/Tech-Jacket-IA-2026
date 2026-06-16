using System.Diagnostics;

using UnityEngine;

public class EnemyIdleState : State<EnemyStates>
{
    private EnemyFSM fsm;

    private float counter = 0;
    public EnemyIdleState(EnemyFSM fsm, StateMachine<EnemyStates> sm) : base(sm)
    {
        this.fsm = fsm;
    }

    public override void Execute()
    {
        base.Execute();
        Idle();
    }

    private void Idle()
    {
        if (counter < 4)
        {
            counter += Time.deltaTime;
        }
        else
        {
            counter = 0;
            _sm.ChangeState(EnemyStates.Patrol);
        }
        SawTheTarget();
    }

    private void SawTheTarget()
    {
        if (fsm.ViewLoS.CheckView(fsm.targetTransform) && fsm.ViewLoS.CheckRange(fsm.targetTransform) && fsm.ViewLoS.CheckAngle(fsm.targetTransform))
        {
            _sm.ChangeState(EnemyStates.SpecificSee);
        }
    }
}
