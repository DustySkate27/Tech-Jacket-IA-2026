using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

public class EnemyStackState : State<EnemyStates>
{
    private EnemyFSM fsm;
    private Stack<Transform> stackWP;
    private bool goingBack = false;
    private Transform currentStackPos = null;
    private int currentWP;

    private Vector3 currentSpeed;

    public EnemyStackState(EnemyFSM fsm, StateMachine<EnemyStates> sm) : base(sm)
    {
        this.fsm = fsm;
        currentWP = fsm.currentWP;
        stackWP = new Stack<Transform>();
    }

    public override void Execute()
    {
        base.Execute();
        Patrol();
        MoveWithAvoidance();
    }

    private void Patrol()
    {
        if (stackWP.Count != fsm.wayPoints.Length && !goingBack) //Si el stack tiene distintos elementos de la lista de wayPoints y goingBack esta desactivado
        {
            if (Vector3.Distance(fsm.transform.position, fsm.wayPoints[currentWP].position) > 0.5f) //Si la distancia es mayor a 0.5
            {
                Flocking(fsm.wayPoints[currentWP].position); //Sigue acercandose
            }
            else //Si no
            {
                stackWP.Push(fsm.wayPoints[currentWP]); //Pushea al stack
                currentWP++; //Avanza al siguiente WayPoint
                if (currentWP >= fsm.wayPoints.Length) //Si se excede la cantidad de waypoints
                {
                    goingBack = true; //Se inicia el proceso inverso de recorrido
                    currentWP = 0; //Se reinician los waypoints
                }
            }
        }
        else if (goingBack) //Si tiene que recorrer el stack
        {
            if (currentStackPos == null) 
            {
                if (!stackWP.TryPop(out currentStackPos))
                {
                    goingBack = false;
                    _sm.ChangeState(EnemyStates.Idle);
                }
            }
            else
            {
                if (Vector3.Distance(fsm.transform.position, currentStackPos.position) > 0.5f)
                    Flocking(currentStackPos.position);
                else
                    currentStackPos = null;
            }
        }

        SawTheTarget();
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

    private void SawTheTarget()
    {
        if (fsm.ViewLoS.CheckView(fsm.targetTransform) && fsm.ViewLoS.CheckRange(fsm.targetTransform) && fsm.ViewLoS.CheckAngle(fsm.targetTransform))
        {
            _sm.ChangeState(EnemyStates.SpecificSee);
        }
    }
}