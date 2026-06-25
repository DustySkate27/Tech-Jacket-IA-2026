
using UnityEngine;

public class EnemyPursuitState : State<EnemyStates>
{
    private EnemyFSM fsm;
    private Vector3 currentSpeed;

    public EnemyPursuitState(EnemyFSM fsm, StateMachine<EnemyStates> sm) : base(sm)
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

    private Vector3 Pursuit()
    {
        if (fsm.target == null) return Vector3.zero;

        Vector3 targetPos = fsm.target.position;
        Vector3 targetVelocity = fsm.target.velocity;
        targetVelocity.y = 0;

        float distance = Vector3.Distance(fsm.myPosition, targetPos);

        // Limita el lookAhead: cerca del target predice poco, lejos predice más
        float maxLookAhead = 1.5f;  // ajustable en segundos
        float lookAheadTime = Mathf.Min(distance / fsm._maxSpeed, maxLookAhead);

        Vector3 predictedPos = targetPos + targetVelocity * lookAheadTime;
        predictedPos.y = fsm.myPosition.y;

        Debug.DrawLine(fsm.myPosition, predictedPos, Color.green);

        Vector3 desired = (predictedPos - fsm.myPosition);
        desired.y = 0;
        desired.Normalize();

        desired *= fsm._maxSpeed;

        return fsm.CalculateSteering(desired);
    }

    private void Flocking()
    {
        Vector3 pursuitForce = Pursuit() * fsm.targetWeight;

        fsm.AddForce(pursuitForce);
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
        if (Vector3.Distance(fsm.transform.position, fsm.target.position) < fsm.specificLoS.range)
        {
            fsm.rend.material.color = fsm.arriveColor;
            _sm.ChangeState(EnemyStates.Arrive);
        }
    }
}
