using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyArriveState : State<EnemyStates>
{
    private EnemyFSM fsm;
    private Vector3 currentSpeed;

    public EnemyArriveState(EnemyFSM fsm, StateMachine<EnemyStates> sm) : base(sm)
    {
        this.fsm = fsm;
    }

    public override void Execute()
    {
        base.Execute();
        Arrive();
    }

    private void Arrive()
    {
        var toTarget = fsm.target.position - fsm.transform.position;
        var distance = toTarget.magnitude;

        float desiredSpeed;

        if (distance < fsm.slowingRadius)
        {
            desiredSpeed = fsm.speed * (distance / fsm.slowingRadius);
        }
        else
        {
            desiredSpeed = fsm.speed;
        }


        var desired = toTarget.normalized * desiredSpeed;

        var steer = desired - currentSpeed;
        steer = Vector3.ClampMagnitude(steer, fsm.maxForce);

        currentSpeed += steer * Time.deltaTime;
        currentSpeed = currentSpeed = Vector3.ClampMagnitude(currentSpeed, desiredSpeed);

        fsm.transform.position += currentSpeed * Time.deltaTime * fsm.speed;

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
        if (Vector3.Distance(fsm.transform.position, fsm.target.position) <= fsm.specificLoS.range)
        {
            _sm.ChangeState(EnemyStates.Attack);
        }
    }
}
