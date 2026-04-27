using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFleeState : State<EnemyStates>
{
    private EnemyFSM fsm;
    private Vector3 currentSpeed;

    public EnemyFleeState(EnemyFSM fsm, StateMachine<EnemyStates> sm) : base(sm)
    {
        this.fsm = fsm;
    }

    public override void Execute()
    {
        base.Execute();
        Flee();
    }

    private void Flee()
    {
        var toTarget = fsm.target.position - fsm.transform.position;

        var dir = -toTarget;
        
        var desired = dir.normalized * fsm.speed;

        var avoidance = fsm.ComputeAvoidance();
        desired += avoidance;

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

        TargetDistanceCheck();
    }

    private void TargetDistanceCheck()
    {
        if (Vector3.Distance(fsm.transform.position, fsm.target.position) > 50f)
        {
            fsm.speed = fsm.speed / 2;
            _sm.ChangeState(EnemyStates.Idle);
        }
    }
}
