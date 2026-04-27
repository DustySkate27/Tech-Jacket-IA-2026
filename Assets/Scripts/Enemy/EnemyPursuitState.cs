
using UnityEngine;

public class EnemyPursuitState : State<EnemyStates>
{
    private EnemyFSM fsm;
    private Vector3 currentSpeed;

    public float avoidanceDistance = 15f;  // que tan lejos detecta obstaculos
    public float avoidanceRadius = 10f; // grosor del SphereCast
    public float avoidanceWeight = 3f;  // que tan fuerte es la evasion

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
        float t = distance * fsm.predictionFactor;

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

        var avoidance = ComputeAvoidance();
        desired += avoidance;

        var steer = desired - currentSpeed;
        steer = Vector3.ClampMagnitude(steer, fsm.maxForce);

        currentSpeed += steer * Time.deltaTime;
        currentSpeed = Vector3.ClampMagnitude(currentSpeed, fsm.speed);

        fsm.transform.position += currentSpeed * Time.deltaTime * fsm.speed;

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

    private Vector3 ComputeAvoidance()
    {
        var forward = fsm.transform.forward;
        var right = fsm.transform.right;

        var directions = new Vector3[]
        {
        forward,
        (forward + right * 0.5f).normalized,
        (forward - right * 0.5f).normalized
        };

        var totalAvoidance = Vector3.zero;
        bool found = false;

        foreach (var dir in directions)
        {
            var hits = Physics.SphereCastAll(
                fsm.transform.position,
                avoidanceRadius,
                dir,
                avoidanceDistance,
                fsm.obsMask);

            foreach (var hit in hits)
            {
                var avoidDir = Vector3.Cross(dir, Vector3.up).normalized;

                if (Vector3.Dot(avoidDir, hit.normal) < 0)
                    avoidDir = -avoidDir;

                var strength = 1f - (hit.distance / avoidanceDistance);

                totalAvoidance += avoidDir * fsm.speed * strength * avoidanceWeight;
                found = true;
            }
        }

        if (!found) return Vector3.zero;

        // Promediar para evitar que multiples hits se cancelen entre si
        return Vector3.ClampMagnitude(totalAvoidance, fsm.speed * avoidanceWeight);
    }

    private void TargetDistanceCheck()
    {
        if (Vector3.Distance(fsm.transform.position, fsm.target.position) < 20f)
        {
            _sm.ChangeState(EnemyStates.Arrive);
        }
    }
}
