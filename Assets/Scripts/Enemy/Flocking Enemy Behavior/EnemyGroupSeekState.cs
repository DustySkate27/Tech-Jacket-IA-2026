using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class EnemyGroupSeekState : State<EnemyStates>
{
    private EnemyGroupFSM enemyGroupFSM;

    public EnemyGroupSeekState(EnemyGroupFSM fsm, StateMachine<EnemyStates> sm) : base(sm)
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

    private Vector3 Seek(Vector3 target)
    {
        Vector3 desired = (target - enemyGroupFSM.transform.position);
        desired.y = 0;                          
        desired.Normalize();
        return enemyGroupFSM.CalculateSteering(desired);
    }

    private void Flocking()
    {
        Vector3 seekForce = (enemyGroupFSM.target != null)
            ? Seek(enemyGroupFSM.target.position) * enemyGroupFSM.targetWeight
            : Vector3.zero;

        enemyGroupFSM.AddForce(
            Separation() * enemyGroupFSM.separationWeight
            + Cohesion() * enemyGroupFSM.cohesionWeight
            + Alignment() * enemyGroupFSM.alignmentWeight
            + seekForce
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
        return Seek(avgPosition);
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
        /*
        if (Vector3.Distance(fsm.transform.position, fsm.target.position) < fsm.specificLoS.range)
        {
            _sm.ChangeState(EnemyStates.Arrive);
        }
        */
    }
}
