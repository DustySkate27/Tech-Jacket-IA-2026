using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

public class EnemyPatrolStack : State<EnemyStates>
{
    private EnemyFSM fsm;
    private Stack<Transform> stackWP;
    private bool goingBack = false;
    private Transform currentStackPos = null;
    private int currentWP;

    private Vector3 currentSpeed;

    public EnemyPatrolStack(EnemyFSM fsm, StateMachine<EnemyStates> sm) : base(sm)
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
            if (Vector3.Distance(fsm.transform.position, fsm.wayPoints[currentWP].position) > 0.5f)
            {
                MoveTowards(fsm.wayPoints[currentWP].position);
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
                    MoveTowards(currentStackPos.position);
                else
                    currentStackPos = null;
            }
        }

        SawTheTarget();
    }

    private void MoveTowards(Vector3 targetPosition)
    {
        var dir = targetPosition - fsm.transform.position;
        var desired = dir.normalized * fsm.speed;

        // Sumar avoidance a la direccion deseada ANTES de calcular el steer
        var avoidance = fsm.ComputeAvoidance();
        desired += avoidance;

        // Steering igual que en Pursuit
        var steer = desired - currentSpeed;
        steer = Vector3.ClampMagnitude(steer, fsm.maxForce);

        currentSpeed += steer * Time.deltaTime;
        currentSpeed = Vector3.ClampMagnitude(currentSpeed, fsm.speed);

        currentSpeed.y = 0;

        fsm.transform.position += currentSpeed * Time.deltaTime;

        
        if (currentSpeed.sqrMagnitude > 0.001f)
        {
            var targetRotation = Quaternion.LookRotation(currentSpeed.normalized);
            fsm.transform.rotation = Quaternion.Slerp(
                fsm.transform.rotation,
                targetRotation,
                fsm.rotationSpeed * Time.deltaTime
            );
        }
    }

    private void SawTheTarget()
    {
        if (fsm.ViewLoS.CheckView(fsm.target) && fsm.ViewLoS.CheckRange(fsm.target) && fsm.ViewLoS.CheckAngle(fsm.target))
        {
            _sm.ChangeState(EnemyStates.SpecificSee);
        }
    }
}