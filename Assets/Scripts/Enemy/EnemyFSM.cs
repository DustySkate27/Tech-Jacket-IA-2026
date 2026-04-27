using UnityEngine;

public enum EnemyStates  
{
    Idle,
    Patrol,
    ObstacleAvoidance,
    SpecificSee,
    Pursuit,
    Flee,
    Seek,
    Arrive,
    Attack
}

public class EnemyFSM : MonoBehaviour
{
    [SerializeField] public Transform target;
    [SerializeField] public Rigidbody targetRB;
    [SerializeField] public BoxCollider hurtbox;

    [SerializeField] public float speed;
    [SerializeField] public Transform[] wayPoints;
    public int currentWP = 0;

    private StateMachine<EnemyStates> _sm;

    [SerializeField] private LineOfSight viewLoS;
    [SerializeField] public LineOfSight specificLoS;

    public bool isEscaper;

    [SerializeField, Header("ObsAvoid")]
    public EnemyObstacleAvoidance obsAvoid;
    public float hitboxRadius;
    public float hitboxAngle;
    public float hitboxOffset;
    public int maxAvoidableObs;
    public LayerMask obsMask;

    public float maxForce = 5f;
    public float rotationSpeed = 5f;
    public float predictionFactor = 0.05f;

    public float slowingRadius = 15f;

    public float avoidanceDistance = 10f;  // que tan lejos detecta obstaculos
    public float avoidanceRadius = 10f; // grosor del SphereCast
    public float avoidanceWeight = 2f;  // que tan fuerte es la evasion


    public LineOfSight ViewLoS => viewLoS;
    public LineOfSight SpecificLoS => specificLoS;

    private void Start()
    {

        hurtbox.enabled = false;
        _sm = new StateMachine<EnemyStates>();

        obsAvoid = new EnemyObstacleAvoidance(transform, hitboxRadius, hitboxAngle, hitboxOffset, obsMask, maxAvoidableObs);

        State<EnemyStates> idle = new EnemyIdleState(this, _sm);


        State<EnemyStates> patrol = null;
        State <EnemyStates> specificSee = null;
        State <EnemyStates> pursuit = null;
        State<EnemyStates> flee = null;
        State<EnemyStates> arrive = null;
        State<EnemyStates> attack = null;

        if (isEscaper)
        {
            patrol = new EnemyPatrolStack(this, _sm);
            specificSee = new EnemyEvadeState(this, _sm);
            flee = new EnemyFleeState(this, _sm);
        }
        else
        {
            patrol = new EnemyPatrolState(this, _sm);
            specificSee = new EnemySeekState(this, _sm); 
            pursuit = new EnemyPursuitState(this, _sm);
            arrive = new EnemyArriveState(this, _sm);
            attack = new EnemyAttackState(this, _sm);
        }

        patrol.AddTransition(idle, EnemyStates.Idle);

        idle.AddTransition(patrol, EnemyStates.Patrol);

        if (isEscaper)
        {
            patrol.AddTransition(specificSee, EnemyStates.SpecificSee);
            patrol.AddTransition(flee, EnemyStates.Flee);
            
            idle.AddTransition(specificSee, EnemyStates.SpecificSee);
            idle.AddTransition(flee, EnemyStates.Flee);

            specificSee.AddTransition(idle, EnemyStates.Idle);
            specificSee.AddTransition(flee, EnemyStates.Flee);

            flee.AddTransition(idle, EnemyStates.Idle);
        }
        else
        {
            patrol.AddTransition(specificSee, EnemyStates.SpecificSee);
            specificSee.AddTransition(pursuit, EnemyStates.Pursuit);
            specificSee.AddTransition(patrol, EnemyStates.Patrol);
            pursuit.AddTransition(arrive, EnemyStates.Arrive);
            arrive.AddTransition(attack, EnemyStates.Attack);
            attack.AddTransition(pursuit, EnemyStates.Pursuit);
            attack.AddTransition(idle, EnemyStates.Idle);
        }
        

        _sm.SetCurrent(idle);
    }

    private void Update()
    {
        _sm.Update();
    }

    public Vector3 ComputeAvoidance()
    {
        var forward = transform.forward;
        var right = transform.right;

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
                transform.position,
                avoidanceRadius,
                dir,
                avoidanceDistance,
                obsMask);

            foreach (var hit in hits)
            {
                var avoidDir = Vector3.Cross(dir, Vector3.up).normalized;

                if (Vector3.Dot(avoidDir, hit.normal) < 0)
                    avoidDir = -avoidDir;

                var strength = 1f - (hit.distance / avoidanceDistance);

                totalAvoidance += avoidDir * speed * strength * avoidanceWeight;
                found = true;
            }
        }

        if (!found) return Vector3.zero;

        // Promediar para evitar que multiples hits se cancelen entre si
        return Vector3.ClampMagnitude(totalAvoidance, speed * avoidanceWeight);
    }

    private void OnDestroy()
    {
        _sm = null;
    }

}
