using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGroupPatrolState : State<EnemyStates>
{
    private EnemyGroupFSM enemyGroupFSM;
    private Transform[] wayPoints;
    private int currentWayPoint = 0;
    private bool reachedWayPoint = false;
    private bool isLeader;

    public EnemyGroupPatrolState(EnemyGroupFSM fsm, StateMachine<EnemyStates> sm) : base(sm)
    {
        enemyGroupFSM = fsm;
        wayPoints = fsm.wayPoints;
        isLeader = fsm.isLeader; // ← flag en el FSM, solo uno lo tiene en true en el inspector

        EventBus.Subscribe<ChangeWayPoint>(ChangeTargetWayPoint);
        EventBus.Subscribe<EnterPursuitState>(ChangePursuitState);
    }

    public override void Awake()
    {
        base.Awake();
    }

    public override void Execute()
    {
        base.Execute();

        CheckWayPoint();
        Flocking();
        MoveWithAvoidance();
        TargetDistanceCheck();
    }

    public override void Sleep()
    {
        base.Sleep();

        EventBus.Unsubscribe<ChangeWayPoint>(ChangeTargetWayPoint);
        EventBus.Unsubscribe<EnterPursuitState>(ChangePursuitState);
    }

    private void CheckWayPoint()
    {
        if (!isLeader) return;
        if (wayPoints == null || wayPoints.Length == 0) return;

        float dist = Vector3.Distance(enemyGroupFSM.myPosition, wayPoints[currentWayPoint].position);

        if (dist < 5f && !reachedWayPoint)
        {
            reachedWayPoint = true;
            
            EventBus.Publish(new ChangeWayPoint());
        }
    }

    public void ChangeTargetWayPoint(ChangeWayPoint wayPointEvent)
    {
        currentWayPoint = (currentWayPoint + 1) % wayPoints.Length;
        reachedWayPoint = false; 
    }

    private Vector3 Seek(Vector3 target)
    {
        Vector3 desired = (target - enemyGroupFSM.transform.position);
        desired.y = 0;
        desired.Normalize();
        desired *= enemyGroupFSM._maxSpeed;
        return enemyGroupFSM.CalculateSteering(desired);
    }

    private void Flocking()
    {
        if (wayPoints == null || wayPoints.Length == 0) return;

        // BUG 1 fix: el weight multiplica la fuerza, no la posición
        Vector3 seekForce = Seek(wayPoints[currentWayPoint].position) * enemyGroupFSM.targetWeight;

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

    private void ChangePursuitState(EnterPursuitState pursuitState)
    {
        _sm.ChangeState(EnemyStates.Pursuit);
    }

    private void TargetDistanceCheck()
    {
        if (enemyGroupFSM.ViewLoS.CheckView(enemyGroupFSM.target.transform) &&
            enemyGroupFSM.ViewLoS.CheckRange(enemyGroupFSM.target.transform) &&
            enemyGroupFSM.ViewLoS.CheckAngle(enemyGroupFSM.target.transform))
        {
            EventBus.Publish(new EnterPursuitState());
        }
    }
}



