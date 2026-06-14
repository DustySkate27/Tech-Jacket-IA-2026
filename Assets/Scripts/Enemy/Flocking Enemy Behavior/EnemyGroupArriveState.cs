using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGroupArriveState : State<EnemyStates>
{
    private EnemyGroupFSM enemyGroupFSM;

    public float arriveRadius = 50f;  
    public float stopRadius = 2f;

    public EnemyGroupArriveState(EnemyGroupFSM fsm, StateMachine<EnemyStates> sm) : base(sm)
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

    private Vector3 Arrive(Vector3 targetPos)
    {
        Vector3 toTarget = targetPos - enemyGroupFSM.myPosition;
        toTarget.y = 0;
        float distance = toTarget.magnitude;

        // Dentro del stopRadius, frenamos completamente
        if (distance < stopRadius)
        {
            enemyGroupFSM._velocity = Vector3.zero;
            return Vector3.zero;
        }

        // Entre stopRadius y arriveRadius, reducimos la velocidad proporcionalmente
        float speed = enemyGroupFSM._maxSpeed;
        if (distance < arriveRadius)
            speed = enemyGroupFSM._maxSpeed * (distance / arriveRadius);

        Vector3 desired = toTarget.normalized * speed;
        return enemyGroupFSM.CalculateSteering(desired);
    }

    private void Flocking()
    {
        Vector3 arriveForce = (enemyGroupFSM.target != null)
            ? Arrive(enemyGroupFSM.target.position) * enemyGroupFSM.targetWeight
            : Vector3.zero;

        enemyGroupFSM.AddForce(
            Separation() * enemyGroupFSM.separationWeight
            + Cohesion() * enemyGroupFSM.cohesionWeight
            + Alignment() * enemyGroupFSM.alignmentWeight
            + arriveForce
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

        Vector3 toAvg = avgPosition - enemyGroupFSM.myPosition;
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
        if (Vector3.Distance(enemyGroupFSM.transform.position, enemyGroupFSM.target.position) > enemyGroupFSM.ViewLoS.range)
        {
            _sm.ChangeState(EnemyStates.Pursuit);
        }
    }
}
