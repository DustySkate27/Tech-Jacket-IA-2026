using UnityEngine;

public class EnemyGroupPursuitState : State<EnemyStates>
{
    private EnemyGroupFSM enemyGroupFSM;

    public EnemyGroupPursuitState(EnemyGroupFSM fsm, StateMachine<EnemyStates> sm) : base(sm)
    {
        enemyGroupFSM = fsm;
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
        if (enemyGroupFSM.target == null) return Vector3.zero;

        Vector3 targetPos = enemyGroupFSM.target.position;
        Vector3 targetVelocity = enemyGroupFSM.target.velocity;
        targetVelocity.y = 0;

        float distance = Vector3.Distance(enemyGroupFSM.myPosition, targetPos);

        // Predicción más agresiva que Arrive
        float lookAheadTime = Mathf.Min(distance / enemyGroupFSM._maxSpeed, 1.5f);

        Vector3 predictedPos = targetPos + targetVelocity * lookAheadTime;
        predictedPos.y = enemyGroupFSM.myPosition.y;

        Vector3 desired = (predictedPos - enemyGroupFSM.myPosition);
        desired.y = 0;
        desired.Normalize();
        desired *= enemyGroupFSM._maxSpeed * 0.5f; // ← más lento que Arrive

        return enemyGroupFSM.CalculateSteering(desired);
    }

    private void Flocking()
    {
        Vector3 pursuitForce = Pursuit() * enemyGroupFSM.targetWeight;

        enemyGroupFSM.AddForce(
            Separation() * enemyGroupFSM.separationWeight
            + Cohesion() * enemyGroupFSM.cohesionWeight
            + Alignment() * enemyGroupFSM.alignmentWeight
            + pursuitForce
        );
    }

    private Vector3 Separation()
    {
        var boidsInRange = Physics.OverlapSphere(enemyGroupFSM.myPosition, enemyGroupFSM.separationRadius, enemyGroupFSM.boidMask);
        Vector3 totalForce = Vector3.zero;
        int cont = 0;

        for (int i = 0; i < boidsInRange.Length; i++)
        {
            var currentBoid = boidsInRange[i];
            if (currentBoid == enemyGroupFSM.myCollider) continue;

            var direction = enemyGroupFSM.myPosition - currentBoid.transform.position;
            var force = direction.normalized / (direction.magnitude / enemyGroupFSM.separationRadius);

            totalForce += force;
            cont++;
        }
        if (cont == 0) return Vector3.zero;

        totalForce /= cont;
        return enemyGroupFSM.CalculateSteering(totalForce * enemyGroupFSM._maxSpeed);
    }

    private Vector3 Cohesion()
    {
        var avgPosition = Vector3.zero;
        int cont = 0;
        var boidsInRange = Physics.OverlapSphere(enemyGroupFSM.myPosition, enemyGroupFSM.cohesionRadius, enemyGroupFSM.boidMask);

        for (int i = 0; i < boidsInRange.Length; i++)
        {
            var currentBoid = boidsInRange[i];
            if (currentBoid == enemyGroupFSM.myCollider) continue;

            avgPosition += currentBoid.transform.position;
            cont++;
        }
        if (cont == 0) return Vector3.zero;

        avgPosition /= cont;

        Vector3 toAvg = (avgPosition - enemyGroupFSM.myPosition);
        toAvg.y = 0;
        toAvg.Normalize();
        return enemyGroupFSM.CalculateSteering(toAvg);
    }

    private Vector3 Alignment()
    {
        var boidsInRange = Physics.OverlapSphere(enemyGroupFSM.myPosition, enemyGroupFSM.cohesionRadius, enemyGroupFSM.boidMask);
        Vector3 avgVelocity = Vector3.zero;
        int cont = 0;

        for (int i = 0; i < boidsInRange.Length; i++)
        {
            var currentBoid = boidsInRange[i].GetComponent<EnemyGroupFSM>();
            if (currentBoid == enemyGroupFSM) continue;

            avgVelocity += currentBoid.transform.forward;
            cont++;
        }
        if (cont == 0) return Vector3.zero;

        return enemyGroupFSM.CalculateSteering(avgVelocity.normalized * enemyGroupFSM._maxSpeed);
    }

    private void MoveWithAvoidance()
    {
        if (enemyGroupFSM._velocity == Vector3.zero) return;

        Vector3 flatVelocity = enemyGroupFSM._velocity;
        flatVelocity.y = 0;

        Vector3 deflectedDir = enemyGroupFSM._obstacleAvoidance.GetDir(flatVelocity.normalized, calculateY: false);
        deflectedDir.y = 0;
        if (deflectedDir == Vector3.zero) deflectedDir = flatVelocity.normalized;
        deflectedDir.Normalize();

        Vector3 moveVelocity = deflectedDir * flatVelocity.magnitude;

        if (deflectedDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(deflectedDir);
            enemyGroupFSM.transform.rotation = Quaternion.RotateTowards(
                enemyGroupFSM.transform.rotation,
                targetRotation,
                enemyGroupFSM._rotationSpeed * Time.deltaTime
            );
        }

        enemyGroupFSM.transform.position += moveVelocity * Time.deltaTime;
        enemyGroupFSM._velocity.y = 0;
    }

    private void TargetDistanceCheck()
    {
        if (enemyGroupFSM.ViewLoS.CheckView(enemyGroupFSM.target.transform) &&
        enemyGroupFSM.ViewLoS.CheckRange(enemyGroupFSM.target.transform) &&
        enemyGroupFSM.ViewLoS.CheckAngle(enemyGroupFSM.target.transform))
        {
            _sm.ChangeState(EnemyStates.Arrive);
        }
    }
}