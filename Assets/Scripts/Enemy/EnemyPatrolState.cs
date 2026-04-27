using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPatrolState : State<EnemyStates>
{
    private EnemyFSM fsm;
    private int currentWP;

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

        if (Vector3.Distance(fsm.transform.position,targetWP.position) > 0.1f)
        {
            Vector3 dir = targetWP.position - fsm.transform.position;

            var avoidance = fsm.ComputeAvoidance();
            dir += avoidance;

            fsm.transform.position += dir.normalized * fsm.speed * Time.deltaTime;

            fsm.transform.forward = dir;
        }
        else
        {
            ChooseNextWaypoint();
        }
        
        SawTheTarget();
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