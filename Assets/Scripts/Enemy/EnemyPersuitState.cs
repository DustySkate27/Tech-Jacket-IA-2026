using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class EnemyPersuitState : State<EnemyStates>
{
    private Transform _target;
    private Transform _entity;
    private Rigidbody playerRB;

    public Persuit(Transform target, Transform entity)
    {
        _target = target;
        _entity = entity;
        playerRB = _entity.GetComponent<Rigidbody>();
    }

    public override void Execute()
    {
        base.Execute();
        Persuit();
    }

    public void Persuit()
    {
        var toQuarry = _target.position - _entity.position;
        var distance = toQuarry.magnitude;
        var c = 2f;
        float t = distance * c;
        var pForward = _entity.forward;
        var qForward = _target.forward;
        var relativeHeading = Vector3.Dot(pForward, qForward);
        var toPursuer = (_entity.position - _target.position).normalized;
        var forwardDot = Vector3.Dot(qForward, toPursuer);
        //
        if (forwardDot > 0 && relativeHeading < -0.95f)
        {
            t = 0;
        }
        else
        {
            if (relativeHeading < 0)
            {
                t *= 1.5f;
            }
            if (forwardDot < 0)
            {
                t *= 1.2f;
            }
        }
        var futurePosition = _target.position + playerRB.linearVelocity * t;
        var dir = futurePosition - _entity.position;
        var desired = dir.normalized * fsm.speed;
        var steer = desired - fsm.speed;
        fsm.speed += steer * Time.deltaTime;

        fsm.transform.position += dir.normalized * fsm.speed * Time.deltaTime;
        fsm.transform.forward = dir;

        TargetDistanceCheck()
    }

    private void TargetDistanceCheck()
    {
        if (Vector3.Distance(fsm.transform.position, target.position) < 0.5f)
        {
            _sm.ChangeState(EnemyStates.SpecificSee);
        }
    }
}
