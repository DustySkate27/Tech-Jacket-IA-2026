using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewEnemyPatrolState : State<EnemyStates>
{
    private EnemyGroupFSM enemyGroupFSM;
    private Transform[] wayPoints;
    private int currentWayPoint = 0;

    public NewEnemyPatrolState(EnemyGroupFSM fsm, StateMachine<EnemyStates> sm) : base(sm)
    {
        enemyGroupFSM = fsm;
        wayPoints = fsm.wayPoints;
    }

    public override void Execute()
    {
        base.Execute();

        CheckWayPoint();
        Flocking();
        MoveWithAvoidance();
        TargetDistanceCheck();
    }

    private void CheckWayPoint()
    {
        if (wayPoints == null || wayPoints.Length == 0) return;

        float dist = Vector3.Distance(enemyGroupFSM.myPosition, wayPoints[currentWayPoint].position);

        if (dist < 5f)
        {
            ChangeTargetWayPoint();
        }
    }

    public void ChangeTargetWayPoint()
    {
        currentWayPoint = (currentWayPoint + 1) % wayPoints.Length;
        Debug.Log(currentWayPoint + " " + enemyGroupFSM.name);
    }

    private Vector3 Seek(Vector3 target)
    {
        Vector3 desired = (target - enemyGroupFSM.transform.position);
        desired.y = 0;
        desired.Normalize();

        desired *= enemyGroupFSM._maxSpeed;

        Vector3 steering = desired - enemyGroupFSM._velocity;
        steering.y = 0;
        return Vector3.ClampMagnitude(steering, enemyGroupFSM._maxForce);
    }

    private void Flocking()
    {
        if (wayPoints == null || wayPoints.Length == 0) return;

        Vector3 seekForce = Seek(wayPoints[currentWayPoint].position) * enemyGroupFSM.targetWeight;

        enemyGroupFSM.AddForce(seekForce);
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



