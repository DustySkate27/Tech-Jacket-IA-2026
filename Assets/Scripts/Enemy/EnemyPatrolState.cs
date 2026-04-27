using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPatrolState : State<EnemyStates>
{
    private EnemyFSM fsm;
    private int currentWP;

    private Vector3 currentSpeed;

    public EnemyPatrolState(
        EnemyFSM fsm,
        StateMachine<EnemyStates> sm) : base(sm)
    {
        this.fsm = fsm;
        currentWP = fsm.currentWP;
    }

    public override void Execute()
    {
        base.Execute();

        Patrol();
    }

    private void Patrol()
    {
        Transform targetWP = fsm.wayPoints[currentWP];

        if (Vector3.Distance(fsm.transform.position,targetWP.position) > 0.5f)
        {
            MoveTowards(fsm.wayPoints[currentWP].position);
        }
        else
        {
            ChooseNextWaypoint();
        }
        
        SawTheTarget();
    }

    private void MoveTowards(Vector3 targetPosition)
    {
        var dir = targetPosition - fsm.transform.position;
        var desired = dir.normalized * fsm.speed;

        // Sumar avoidance a la direccion deseada ANTES de calcular el steer
        var avoidance = fsm.ComputeAvoidance();
        desired += avoidance;

        // Steering igual que en Pursuit
        var steer = desired - currentSpeed;
        steer = Vector3.ClampMagnitude(steer, fsm.maxForce);

        currentSpeed += steer * Time.deltaTime;
        currentSpeed = Vector3.ClampMagnitude(currentSpeed, fsm.speed);

        currentSpeed.y = 0;

        fsm.transform.position += currentSpeed * Time.deltaTime;

        // Rotar hacia donde se mueve

        if (currentSpeed.sqrMagnitude > 0.001f)
        {
            var targetRotation = Quaternion.LookRotation(currentSpeed.normalized);
            fsm.transform.rotation = Quaternion.Slerp(
                fsm.transform.rotation,
                targetRotation,
                fsm.rotationSpeed * Time.deltaTime
            );
        }
    }


    private void ChooseNextWaypoint()
    {
        var weights = SetWeights(currentWP,fsm.wayPoints);

        Transform next = MyRandom.RouletteWheelSelection(weights);

        currentWP = Array.IndexOf(fsm.wayPoints,next);
            
    }


    private Dictionary<Transform, float> SetWeights(int currentIndex, Transform[] waypoints)
    {
        Dictionary<Transform, float> weights = new();

        Transform current = waypoints[currentIndex];

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (i == currentIndex)
                continue;

            Transform wp = waypoints[i];

            float dist =
                Vector3.Distance(current.position, wp.position);

            dist = Mathf.Max(dist,0.01f);

            // peso segun distancia
            float weight = 1f / dist;

            if (Mathf.Abs(i - currentIndex) == 1)
            {
                weight *= 2f;
            }

            weights.Add(wp,weight);
        }

        return weights;
    }


    private void SawTheTarget()
    {
        if (fsm.ViewLoS.CheckView(fsm.target) &&
            fsm.ViewLoS.CheckRange(fsm.target) &&
            fsm.ViewLoS.CheckAngle(fsm.target))
        {
            _sm.ChangeState(EnemyStates.SpecificSee);
        }
    }
}