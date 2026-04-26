
using UnityEngine;

public class EnemyPursuitState : State<EnemyStates>
{
    private EnemyFSM fsm;
    private Vector3 currentSpeed;

    public EnemyPursuitState(EnemyFSM fsm, StateMachine<EnemyStates> sm) : base(sm)
    {
        this.fsm = fsm;
    }

    public override void Execute()
    {
        base.Execute();
        Pursuit();
    }

    public void Pursuit()
    {
        var toQuarry = fsm.target.position - fsm.transform.position;
        var distance = toQuarry.magnitude;
        var c = 0.05f;
        float t = distance * c;

        var pForward = fsm.transform.forward;
        var qForward = fsm.target.forward;


        var relativeHeading = Vector3.Dot(pForward, qForward);
        var toPursuer = (fsm.transform.position - fsm.target.position).normalized;
        var forwardDot = Vector3.Dot(qForward, toPursuer);

        if (forwardDot > 0 && relativeHeading < -0.95f)
        {
            t = 0;
        }
        else
        {
            if (relativeHeading < 0) t *= 1.5f;
            if (forwardDot < 0) t *= 1.2f;
        }

        var futurePosition = fsm.target.position + fsm.targetRB.velocity * t;

        var dir = futurePosition - fsm.transform.position;
        var desired = dir.normalized * fsm.speed;

        var steer = desired - currentSpeed;
        steer = Vector3.ClampMagnitude(steer, fsm.maxForce);

        currentSpeed += steer * Time.deltaTime;
        currentSpeed = Vector3.ClampMagnitude(currentSpeed, fsm.speed);

        fsm.transform.position += currentSpeed * Time.deltaTime;

        if (currentSpeed.sqrMagnitude > 0.001f)
        {
            var targetRotation = Quaternion.LookRotation(currentSpeed.normalized);
            fsm.transform.rotation = Quaternion.Slerp(
                fsm.transform.rotation,
                targetRotation,
                fsm.rotationSpeed * Time.deltaTime
            );
        }


        TargetDistanceCheck();
    }

    private void TargetDistanceCheck()
    {
        if (Vector3.Distance(fsm.transform.position, fsm.target.position) < 20f)
        {
            _sm.ChangeState(EnemyStates.Arrive);
        }
    }
}
