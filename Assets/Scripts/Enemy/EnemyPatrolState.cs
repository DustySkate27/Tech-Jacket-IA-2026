using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

public class EnemyPatrolState : State<EnemyStates>
{
    private EnemyFSM fsm;
    private Stack<Transform> stackWP;
    private bool goingBack = false;
    private Transform currentStackPos = null;
    private int currentWP;

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
            if (Vector3.Distance(fsm.transform.position, fsm.wayPoints[currentWP].position) > 0.1f)
            {
                Vector3 dir = fsm.wayPoints[currentWP].position - fsm.transform.position;
                dir = fsm.obsAvoid.GetDir(dir);
                dir.y = 0;
                dir = dir.normalized;
                fsm.transform.position += dir.normalized * fsm.speed * Time.deltaTime;
                fsm.transform.forward = dir;
            }
            else
            {
                stackWP.Push(fsm.wayPoints[currentWP]);
                currentWP++;
                if (currentWP >= fsm.wayPoints.Length)
                {
                    goingBack = true;
                    currentWP = 0;
                }

            }
        }
        else if (goingBack)
        {
            if (currentStackPos == null)
            {
                if (!stackWP.TryPop(out currentStackPos))
                {
                    goingBack = false;
                    _sm.ChangeState(EnemyStates.Idle);
                }
            }
            else
            {
                if (Vector3.Distance(fsm.transform.position, currentStackPos.position) > 0.5f)
                {
                    Vector3 dir = fsm.wayPoints[currentWP].position - fsm.transform.position;
                    dir = fsm.obsAvoid.GetDir(dir);
                    dir.y = 0;
                    dir = dir.normalized;
                    fsm.transform.position += dir.normalized * fsm.speed * Time.deltaTime;
                    fsm.transform.forward = dir;
                }
                else
                    currentStackPos = null;
            }
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
