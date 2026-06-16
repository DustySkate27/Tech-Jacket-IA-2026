using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class EnemyPatrolState : State<EnemyStates>
{
    private EnemyFSM fsm;
    private int currentWP;
    private PF_WNode lastNode;
    private Vector3 currentSpeed;

    private List<PF_WNode> path = null;

    private Dictionary<PF_WNode, float> dynamicWeights = new();

    public EnemyPatrolState(EnemyFSM fsm, StateMachine<EnemyStates> sm) : base(sm)
    {
        this.fsm = fsm;
        currentWP = fsm.currentWP;

        foreach (var node in fsm.nodeList)
        {
            dynamicWeights[node] = 1f;
        }
    }


    public override void Execute()
    {
        base.Execute();

        ThetaPatrol();
        MoveWithAvoidance();
    }

    private void Patrol()
    { 

        if (Vector3.Distance(fsm.transform.position, fsm.wayPoints[currentWP].position) > 1f) //Si la distancia es mayor a 1, estan lejos todavia
        {
            Flocking(fsm.wayPoints[currentWP].position); //Se acercan al waypoint asignado
        }
        else
        {
            currentWP = ChooseNextWaypoint(); //Si no, van al próximo.
        }
        
        SawTheTarget(); //Si ven al player cambia su estado
    }

    private void ThetaPatrol()
    {
        if (path != null)
        {
            if (currentWP >= path.Count)
            {
                lastNode = path[path.Count - 1];
                path = null;
            }
            else if (currentWP < path.Count && Vector3.Distance(fsm.transform.position, path[currentWP].transform.position) > 2f) //Si la distancia es mayor a 1, estan lejos todavia
            {
                Flocking(path[currentWP].transform.position); //Se acercan al waypoint asignado
            }
            else
            {
                currentWP++;
            }
        }
        else
        {
            currentWP = 0;

            PF_WNode newStart = lastNode ?? ChooseNextNode();
            PF_WNode newTarget = ChooseNextNode();
            do { newTarget = ChooseNextNode(); } while (newTarget == newStart);

            path = Theta.ThetaStar(newStart, node => node == newTarget, node => node.Neighbors,
                (a, b) => Vector3.Distance(a.transform.position, b.transform.position),
                node => Vector3.Distance(node.transform.position, newTarget.transform.position), (a, b) => a.CanSee(b));

            if (path.Count == 0)
            {
                path = null;
                lastNode = null;
            }

            Debug.Log(newStart.name);
            Debug.Log(newTarget.name);
        }
        

        SawTheTarget(); //Si ven al player cambia su estado
    }

    private Vector3 Seek(Vector3 target)
    {
        Vector3 desired = (target - fsm.transform.position);
        desired.y = 0;
        desired.Normalize();

        desired *= fsm._maxSpeed;

        Vector3 steering = desired - fsm._velocity;
        steering.y = 0;
        return Vector3.ClampMagnitude(steering, fsm._maxForce);
    }

    private void Flocking(Vector3 target)
    {
        Vector3 seekForce = Seek(target) * fsm.targetWeight;

        fsm.AddForce(seekForce);
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

    private int ChooseNextWaypoint()
    {
        var weights = SetWeights(currentWP,fsm.wayPoints);

        Transform next = MyRandom.RouletteWheelSelection(weights);

        return Array.IndexOf(fsm.wayPoints,next);
    }

    private PF_WNode ChooseNextNode()
    {

        PF_WNode selected = MyRandom.RouletteWheelSelection(dynamicWeights);

        dynamicWeights[selected] *= 0.3f;

        foreach (var node in new List<PF_WNode>(dynamicWeights.Keys))
        {
            if (node != selected)
            {
                dynamicWeights[node] += 0.2f;
            }
        }

        return selected;
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
        if (fsm.ViewLoS.CheckView(fsm.targetTransform) &&
            fsm.ViewLoS.CheckRange(fsm.targetTransform) &&
            fsm.ViewLoS.CheckAngle(fsm.targetTransform))
        {
            _sm.ChangeState(EnemyStates.SpecificSee);
        }
    }
}