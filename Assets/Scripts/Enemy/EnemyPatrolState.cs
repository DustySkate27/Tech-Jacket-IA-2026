using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class EnemyPatrolState : State<EnemyStates>
{
    private EnemyFSM fsm;
    private int currentWP;
    private PF_WNode lastNode;
    private Vector3 currentSpeed;

    private List<PF_WNode> path = null;

    public EnemyPatrolState(EnemyFSM fsm, StateMachine<EnemyStates> sm) : base(sm)
    {
        this.fsm = fsm;
        currentWP = fsm.currentWP;
    }

    public override void Execute()
    {
        base.Execute();

        //Patrol();
        ThetaPatrol();
    }

    private void Patrol()
    { 

        if (Vector3.Distance(fsm.transform.position, fsm.wayPoints[currentWP].position) > 1f) //Si la distancia es mayor a 1, estan lejos todavia
        {
            MoveTowards(fsm.wayPoints[currentWP].position); //Se acercan al waypoint asignado
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
            else if (currentWP < path.Count && Vector3.Distance(fsm.transform.position, path[currentWP].transform.position) > 1f) //Si la distancia es mayor a 1, estan lejos todavia
            {
                MoveTowards(path[currentWP].transform.position); //Se acercan al waypoint asignado
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

    private int ChooseNextWaypoint()
    {
        var weights = SetWeights(currentWP,fsm.wayPoints);

        Transform next = MyRandom.RouletteWheelSelection(weights);

        return Array.IndexOf(fsm.wayPoints,next);
    }

    private PF_WNode ChooseNextNode()
    {
        //ACA HAY QUE METER EL DYNAMIC RANDOM PIPA

        return fsm.nodeList[UnityEngine.Random.Range(0,fsm.nodeList.Count)];
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