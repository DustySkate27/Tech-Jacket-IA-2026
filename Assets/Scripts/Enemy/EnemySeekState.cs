using UnityEngine;

public class EnemySeekState : State<EnemyStates>
{
    EnemyFSM fsm;
    float baseRange;

    public EnemySeekState(EnemyFSM fsm, StateMachine<EnemyStates> sm) : base(sm)
    {
        this.fsm = fsm;
        baseRange = fsm.specificLoS.range;
    }

    public override void Execute()
    {
        base.Execute();
        Seek();
    }

    public void Seek()
    {
        if (fsm.ViewLoS.CheckAngle(fsm.target) && fsm.ViewLoS.CheckRange(fsm.target) && fsm.ViewLoS.CheckView(fsm.target))
        {
            if(fsm.specificLoS.range < fsm.ViewLoS.range)
            {
                fsm.specificLoS.range += 5 * Time.deltaTime;
            }
            else
            {
                fsm.specificLoS.range = fsm.ViewLoS.range;
            }
            SawTheTarget();
        }
        else
        {
            _sm.ChangeState(EnemyStates.Patrol);
        }
    }

    private void SawTheTarget()
    {
        if (fsm.specificLoS.CheckView(fsm.target) &&
            fsm.specificLoS.CheckRange(fsm.target) &&
            fsm.specificLoS.CheckAngle(fsm.target))
        {
            fsm.specificLoS.range = baseRange;
            _sm.ChangeState(EnemyStates.Pursuit);
        }
    }
}
