using UnityEngine;

public enum EnemyStates  
{
    Idle,
    Patrol,
    PatrolStack,
    ObstacleAvoidance,
    SpecificSee,
    SpecificBeenSeen,
    Arrive,
    Persuit
}

public class EnemyFSM : MonoBehaviour
{
    [SerializeField] public Transform target;
    [SerializeField] public Rigidbody targetRB;

    public int speed;
    [SerializeField] public Transform[] wayPoints;
    public int currentWP = 0;

    private StateMachine<EnemyStates> _sm;

    [SerializeField] private LineOfSight viewLoS;
    [SerializeField] private LineOfSight specificLoS;

    public bool isEscaper;

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


    public LineOfSight ViewLoS => viewLoS;
    public LineOfSight SpecificLoS => specificLoS;

    private void Start()
    {
        _sm = new StateMachine<EnemyStates>();

        obsAvoid = new EnemyObstacleAvoidance(transform, hitboxRadius, hitboxAngle, hitboxOffset, obsMask, maxAvoidableObs);

        State<EnemyStates> idle = new EnemyIdleState(this, _sm);
        State<EnemyStates> patrol = new EnemyPatrolStack(this, _sm);

        State<EnemyStates> specificSee = null;
        State<EnemyStates> specificHaveBeenSeen = null;

        State<EnemyStates> pursuit = null;
        State<EnemyStates> arrive = null;

        if (isEscaper)
        {
            specificSee = new EnemyEvadeState(this, _sm);
            specificHaveBeenSeen = new EnemyFleeState(this, _sm);
        }
        else
        {
            pursuit = new EnemyPursuitState(this, _sm);
            arrive = new EnemyArriveState(this, _sm);
        }

        idle.AddTransition(patrol, EnemyStates.PatrolStack);

        patrol.AddTransition(idle, EnemyStates.Idle);

        if (isEscaper)
        {
            patrol.AddTransition(specificSee, EnemyStates.SpecificSee);
            patrol.AddTransition(specificHaveBeenSeen, EnemyStates.SpecificBeenSeen);

            idle.AddTransition(specificSee, EnemyStates.SpecificSee);
            idle.AddTransition(specificHaveBeenSeen, EnemyStates.SpecificBeenSeen);

            specificSee.AddTransition(idle, EnemyStates.Idle);
            specificHaveBeenSeen.AddTransition(idle, EnemyStates.Idle);
        }
        
        if (!isEscaper)
        {
            pursuit.AddTransition(arrive, EnemyStates.Arrive);
            arrive.AddTransition(pursuit, EnemyStates.Persuit);
        }
        

        _sm.SetCurrent(idle);
    }

    private void Update()
    {
        _sm.Update();
    }

}
