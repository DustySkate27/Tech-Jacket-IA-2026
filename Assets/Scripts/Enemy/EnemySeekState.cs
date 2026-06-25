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
        if (fsm.ViewLoS.CheckAngle(fsm.targetTransform) && fsm.ViewLoS.CheckRange(fsm.targetTransform) && fsm.ViewLoS.CheckView(fsm.targetTransform))
        {
            if(fsm.specificLoS.range < fsm.ViewLoS.range)
            {
                fsm.specificLoS.range += 10 * Time.deltaTime;
            }
            else
            {
                fsm.specificLoS.range = fsm.ViewLoS.range;
            }
            SawTheTarget();
        }
        else
        {
            fsm.rend.material.color = fsm.patrolColor;
            _sm.ChangeState(EnemyStates.Patrol);
        }
    }

    private void SawTheTarget()
    {
        if (fsm.specificLoS.CheckView(fsm.targetTransform) &&
            fsm.specificLoS.CheckRange(fsm.targetTransform) &&
            fsm.specificLoS.CheckAngle(fsm.targetTransform))
        {
            fsm.specificLoS.range = baseRange;
            fsm.rend.material.color = fsm.pursuitColor;
            fsm.currentMesh.mesh = fsm.pursuitMesh;
            _sm.ChangeState(EnemyStates.Pursuit);
        }
    }
}
