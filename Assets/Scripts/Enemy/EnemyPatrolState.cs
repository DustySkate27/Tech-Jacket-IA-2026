using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

public class EnemyPatrolState : State<EnemyStates>
{
    private EnemyFSM fsm;
    private Stack<Transform> stackWP;
    private bool goingBack = false;
    private int currentWP;
    private int speed;

    public EnemyPatrolState(EnemyFSM fsm, StateMachine<EnemyStates> sm) : base(sm)
    {
        this.fsm = fsm;
        currentWP = fsm.currentWP;
        stackWP = new Stack<Transform>();
    }

    public override void Execute()
    {
        base.Execute();
        Patrol();
    }

    private void Patrol()
    {
        if (stackWP.Count != fsm.wayPoints.Length && !goingBack)
        {
            if (Vector3.Distance(fsm.transform.position, fsm.wayPoints[currentWP].position) < 0.5f)
            {
                stackWP.Push(fsm.wayPoints[currentWP]);
                Vector3 dir = fsm.transform.position - fsm.wayPoints[currentWP].position;
                fsm.transform.position += dir.normalized * speed * Time.deltaTime;
                fsm.transform.forward = dir;
            }
            else
            {
                currentWP++;
            }
        }
        else if (goingBack && stackWP.TryPop(out Transform currentPos))
        {
            if (Vector3.Distance(fsm.transform.position,currentPos.position) < 0.5f)
            {
                Vector3 dir = fsm.transform.position - currentPos.position;
                fsm.transform.position += dir.normalized * speed * Time.deltaTime;
                fsm.transform.forward = dir;
            }
        }
        else if (stackWP.Count == 0 && goingBack)
        {
            goingBack = false;
            _sm.ChangeState(EnemyStates.Idle);
        }
        else
        {
            goingBack = true;
        }

        SawTheTarget();
    }

    private void SawTheTarget()
    {
        if (fsm.ViewLoS.CheckView(fsm.target) && fsm.ViewLoS.CheckRange(fsm.target) && fsm.ViewLoS.CheckAngle(fsm.target))
        {
            _sm.ChangeState(EnemyStates.SpecificSee);
        }
    }
}
