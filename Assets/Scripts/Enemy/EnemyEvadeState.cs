using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyEvadeState : State<EnemyStates>
{
    private EnemyFSM fsm;

    public EnemyEvadeState(EnemyFSM fsm, StateMachine<EnemyStates> sm) : base(sm)
    {
        this.fsm = fsm;
    }

    public override void Execute()
    {
        base.Execute();
        Evade();
        MoveWithAvoidance();

        TargetDistanceCheck();
    }

    private Vector3 EvadeForce()
    {
        if (fsm.target == null) return Vector3.zero;

        Vector3 targetPos = fsm.target.position;
        Vector3 targetVelocity = fsm.target.velocity;
        targetVelocity.y = 0;

        // Predice posición futura igual que Pursuit
        float distance = Vector3.Distance(fsm.myPosition, targetPos);
        float maxLookAhead = fsm.maxLookAhead;
        float lookAheadTime = Mathf.Min(distance / fsm._maxSpeed, maxLookAhead);

        Vector3 predictedPos = targetPos + targetVelocity * lookAheadTime;
        predictedPos.y = fsm.myPosition.y;

        Debug.DrawLine(fsm.myPosition, predictedPos, Color.magenta);

        // Flee de la posición predicha (dirección invertida respecto a Pursuit)
        Vector3 desired = (fsm.myPosition - predictedPos);
        desired.y = 0;
        desired.Normalize();
        desired *= fsm._maxSpeed * 4f;

        Vector3 steering = desired - fsm._velocity;
        steering.y = 0;
        return Vector3.ClampMagnitude(steering, fsm._maxForce);
    }

    private void Evade()
    {
        Vector3 evadeForce = EvadeForce() * fsm.targetWeight;
        fsm.AddForce(evadeForce);
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

        Vector3 moveVelocity = deflectedDir * fsm._maxSpeed * 4f; 

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
        if (Vector3.Distance(fsm.transform.position, fsm.target.position) > 30f)
        {
            fsm.rend.material.color = fsm.fleeColor;
            fsm.currentMesh.mesh = fsm.fleeMesh;
            _sm.ChangeState(EnemyStates.Flee);
        }
    }
}
