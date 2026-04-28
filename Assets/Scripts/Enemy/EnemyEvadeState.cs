using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyEvadeState : State<EnemyStates>
{
    private EnemyFSM fsm;
    private Vector3 currentSpeed;
    public EnemyEvadeState(EnemyFSM fsm, StateMachine<EnemyStates> sm) : base(sm)
    {
        this.fsm = fsm;
    }

    public override void Execute()
    {
        base.Execute();

        Evade();
    }

    public void Evade()
    {
        var toQuarry = fsm.target.position - fsm.transform.position; //direccion al objetivo
        var distance = toQuarry.magnitude; //distancia
        float t = distance * fsm.predictionFactor; //factor de prediccion

        var pForward = fsm.transform.forward; //forward del enemigo
        var qForward = fsm.target.forward; //forward objetivo


        var relativeHeading = Vector3.Dot(pForward, qForward); //dot product para prediccion de direccion
        var toPursuer = (fsm.transform.position - fsm.target.position).normalized; //direccion al enemigo
        var forwardDot = Vector3.Dot(qForward, toPursuer); //dot product para direccion con prediccion

        if (forwardDot > 0 && relativeHeading < -0.95f) //si la direccion con prediccion es mayor a 0 y la prediccion es menor a -0.95
        {
            t = 0; //no hay prediccion
        }
        else //sino
        {
            if (relativeHeading < 0) t *= 1.5f; //Si prediccion menor a 0 => aumenta prediccion
            if (forwardDot < 0) t *= 1.2f; //Si direccion con prediccion => aumenta aun mas prediccion
        }

        var futurePosition = fsm.target.position + fsm.targetRB.velocity * t; //Añade a la posicion enemiga la posicion del objetivo por la prediccion

        var dir = fsm.transform.position - futurePosition; //direccion prediciendo al objetivo
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

        TargetDistanceCheck();
    }

    private void TargetDistanceCheck()
    {
        if (Vector3.Distance(fsm.transform.position, fsm.target.position) > 30f)
        {
            fsm.speed = fsm.speed * 2;
            _sm.ChangeState(EnemyStates.Flee);
        }
    }
}
