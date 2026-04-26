using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyArriveState : State<EnemyStates>
{
    private Transform _target;
    private Transform _entity;
    private float maxSpeed;
    private float slowingDistance;
    public EnemyArriveState(Transform target, Transform entity, float maxSpeed, float slowingDistance)
    {
        _target = target;
        _entity = entity;
        this.maxSpeed = maxSpeed;
        this.slowingDistance = slowingDistance;
    }

    public override void Execute()
    {
        base.Execute();
        Arrive();
    }

    public void Arrive()
    {
        var dir = _target.position - _entity.position;
        var distance = dir.magnitude;
        var rampedSpeed = fsm.speed * (distance / slowingDistance);
        var clipedSpeed = Mathf.Min(rampedSpeed, fsm.speed);
        var desired = dir / distance * clipedSpeed;
        var steer = desired - fsm.speed;
        fsm.speed += steer * Time.deltaTime;

        fsm.transform.position += dir.normalized * fsm.speed * Time.deltaTime;
        fsm.transform.forward = dir;

        TargetDistanceCheck()
    }

    private void TargetDistanceCheck()
    {
        if (Vector3.Distance(fsm.transform.position, target.position) > 0.5f)
        {
            _sm.ChangeState(EnemyStates.SpecificSee);
        }
    }
}
