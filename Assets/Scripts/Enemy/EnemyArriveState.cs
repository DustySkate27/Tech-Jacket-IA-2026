using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyArriveState : State<EnemyStates>
{
    private EnemyFSM fsm;

    public float arriveRadius = 50f;
    public float stopRadius = 2f;

    public EnemyArriveState(EnemyFSM fsm, StateMachine<EnemyStates> sm) : base(sm)
    {
        this.fsm = fsm;
    }

    public override void Execute()
    {
        base.Execute();
        Flocking();
        MoveWithAvoidance();

        TargetDistanceCheck();
    }

    private Vector3 Arrive(Vector3 targetPos)
    {
        Vector3 toTarget = targetPos - fsm.myPosition;
        toTarget.y = 0;
        float distance = toTarget.magnitude;

        // Dentro del stopRadius, frenamos completamente
        if (distance < stopRadius)
        {
            fsm._velocity = Vector3.zero;
            return Vector3.zero;
        }

        // Entre stopRadius y arriveRadius, reducimos la velocidad proporcionalmente
        float speed = fsm._maxSpeed;
        if (distance < arriveRadius)
            speed = fsm._maxSpeed * (distance / arriveRadius);

        Vector3 desired = toTarget.normalized * speed;
        return fsm.CalculateSteering(desired);
    }

    private void Flocking()
    {
        Vector3 arriveForce = (fsm.target != null)
            ? Arrive(fsm.target.position) * fsm.targetWeight
            : Vector3.zero;

        fsm.AddForce(arriveForce);
    }

    private void MoveWithAvoidance()
    {
        if (fsm._velocity == Vector3.zero) return;

        Vector3 flatVelocity = fsm._velocity;
        flatVelocity.y = 0;

        Vector3 deflectedDir = fsm._obstacleAvoidance.GetDir(flatVelocity.normalized, calculateY: false);
        deflectedDir.y = 0;
        if (deflectedDir == Vector3.zero) deflectedDir = flatVelocity.normalized;
        deflectedDir.Normalize();

        Vector3 moveVelocity = deflectedDir * flatVelocity.magnitude;

        if (deflectedDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(deflectedDir);
            fsm.transform.rotation = Quaternion.RotateTowards(
                fsm.transform.rotation,
                targetRotation,
                fsm._rotationSpeed * Time.deltaTime
            );
        }

        fsm.transform.position += moveVelocity * Time.deltaTime;
        fsm._velocity.y = 0;
    }

    private void TargetDistanceCheck()
    {
        if (Vector3.Distance(fsm.transform.position, fsm.target.position) <= fsm.specificLoS.range)
        {
            _sm.ChangeState(EnemyStates.Attack);
        }
    }
}
