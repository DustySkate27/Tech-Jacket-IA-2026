using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

public class EnemyHidingState : State<EnemyStates>
{
    private EnemyFSM fsm;
    private int currentWP;


    private List<PF_WNode> path = null;
    private PF_WNode lastNode;
    private Dictionary<PF_WNode, float> dynamicWeights = new();



    public EnemyHidingState(EnemyFSM fsm, StateMachine<EnemyStates> sm) : base(sm)
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

    private void SawTheTarget()
    {
        if (fsm.ViewLoS.CheckView(fsm.targetTransform) && fsm.ViewLoS.CheckRange(fsm.targetTransform) && fsm.ViewLoS.CheckAngle(fsm.targetTransform))
        {
            fsm.rend.material.color = fsm.specificSeeColor;
            _sm.ChangeState(EnemyStates.SpecificSee);
        }
    }
}