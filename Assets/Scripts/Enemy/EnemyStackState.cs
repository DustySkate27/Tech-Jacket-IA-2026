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
    }

    private void Patrol()
    {
        if (stackWP.Count != fsm.wayPoints.Length && !goingBack) //Si el stack tiene distintos elementos de la lista de wayPoints y goingBack esta desactivado
        {
            if (Vector3.Distance(fsm.transform.position, fsm.wayPoints[currentWP].position) > 0.5f) //Si la distancia es mayor a 0.5
            {
                MoveTowards(fsm.wayPoints[currentWP].position); //Sigue acercandose
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
                    MoveTowards(currentStackPos.position);
                else
                    currentStackPos = null;
            }
        }

        SawTheTarget();
    }

    private void MoveTowards(Vector3 targetPosition) //Obstacle Avoidance
    {
        var dir = targetPosition - fsm.transform.position; //Direccion del objetivo.
        var desired = dir.normalized * fsm.speed; //Direccion a la que va a ir el enemigo

        var avoidForce = fsm.ComputeAvoidance(); //Ejecución de Obstacle Avoidance

        Vector3 steer; //Inicializa el virado
        if (avoidForce.HasValue) //Si existe un obstáculo, obtiene la dirección de evasión
        {
            var evadeDesired = avoidForce.Value.normalized * fsm.speed; //Inicializa la evasión objetivo multiplicando la fuerza de evasión normalizada por la velocidad.
            steer = evadeDesired - currentSpeed; //El virado es equivalente a la diferencia entre la evasión objetivo y la dirección actual
        }
        else //Si no existe
        {
            steer = desired - currentSpeed; //El virado es equivalente a la dirección objetivo menos la actual.
        }

        steer = Vector3.ClampMagnitude(steer, fsm.maxForce); //Camplea la magnitud de la dirección entre si mismo y la potencia máxima de virado.
        currentSpeed += steer * Time.deltaTime; //le suma a la dirección actual el virado a lo largo del tiempo.
        currentSpeed = Vector3.ClampMagnitude(currentSpeed, fsm.speed); //Clampea la magnitud de la dirección actual entre si misma y la velocidad.
        currentSpeed.y = 0; //Neutraliza la altura de la dirección actual

        fsm.transform.position += currentSpeed * Time.deltaTime; //Suma a la posición.

        if (currentSpeed.sqrMagnitude > 0.001f) //Si la magnitud al cuadrado es menor al un número infimo
        {
            var targetRotation = Quaternion.LookRotation(currentSpeed.normalized); //Inicializa rotacion objetivo
            fsm.transform.rotation = Quaternion.Slerp(fsm.transform.rotation, targetRotation, fsm.rotationSpeed * Time.deltaTime); //La iguala a la rotacion del transform
        }
    }

    private void SawTheTarget()
    {
        if (fsm.ViewLoS.CheckView(fsm.target) && fsm.ViewLoS.CheckRange(fsm.target) && fsm.ViewLoS.CheckAngle(fsm.target))
        {
            _sm.ChangeState(EnemyStates.SpecificSee);
        }
    }
}