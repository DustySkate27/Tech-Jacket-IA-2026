using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyArriveState : State<EnemyStates>
{
    private EnemyFSM fsm;
    private Vector3 currentSpeed;

    public EnemyArriveState(EnemyFSM fsm, StateMachine<EnemyStates> sm) : base(sm)
    {
        this.fsm = fsm;
    }

    public override void Execute()
    {
        base.Execute();
        Arrive();
    }

    private void Arrive()
    {
        var toTarget = fsm.target.position - fsm.transform.position; //Direccion del objetivo.
        var distance = toTarget.magnitude; //Obtiene su magnitud, referencia a distancia

        float desiredSpeed;

        if (distance < fsm.slowingRadius) //si la distancia es menor al "Rango de ralentizado"
        {
            desiredSpeed = fsm.speed * (distance / fsm.slowingRadius); //Crea un "Ralentizador"
        }
        else //Si no
        { 
            desiredSpeed = fsm.speed; //La velocidad sigue igual
        }


        var desired = toTarget.normalized * desiredSpeed; //La dirección deseada es igual a la direccion normalizada por el "Ralentizador"

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

        TargetDistanceCheck();
    }

    private void TargetDistanceCheck()
    {
        if (Vector3.Distance(fsm.transform.position, fsm.target.position) <= fsm.specificLoS.range)
        {
            _sm.ChangeState(EnemyStates.Attack);
        }
    }
}
