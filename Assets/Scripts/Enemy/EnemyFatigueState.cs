using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFatigueState : State<EnemyStates>
{
    private EnemyFSM fsm;

    public EnemyFatigueState(EnemyFSM fsm, StateMachine<EnemyStates> sm) : base(sm)
    {
        this.fsm = fsm;
    }

    public override void Execute()
    {
        base.Execute();
        Flee();
        MoveWithAvoidance();

        TargetDistanceCheck();
    }

    private Vector3 FleeForce(Vector3 target)
    {
        // Dirección invertida: se aleja del target en vez de acercarse
        Vector3 desired = (fsm.transform.position - target);
        desired.y = 0;
        desired.Normalize();
        desired *= fsm._maxSpeed * 0.4f;

        Vector3 steering = desired - fsm._velocity;
        steering.y = 0;
        return Vector3.ClampMagnitude(steering, fsm._maxForce);
    }

    private void Flee()
    {
        if (fsm.target == null) return;

        Vector3 fleeForce = FleeForce(fsm.target.position) * fsm.targetWeight;
        fsm.AddForce(fleeForce);
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

        Vector3 moveVelocity = deflectedDir * fsm._maxSpeed;

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
        if (Vector3.Distance(fsm.transform.position, fsm.target.position) > 50f)
        {
            fsm.rend.material.color = fsm.fatigueColor;
            fsm.currentMesh.mesh = fsm.baseMesh;
            _sm.ChangeState(EnemyStates.Idle);
        }
    }
}