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


    public LineOfSight ViewLoS => viewLoS;
    public LineOfSight SpecificLoS => specificLoS;

    private void Start()
    {
        _sm = new StateMachine<EnemyStates>();

        obsAvoid = new EnemyObstacleAvoidance(transform, hitboxRadius, hitboxAngle, hitboxOffset, obsMask, maxAvoidableObs);

        State<EnemyStates> idle = new EnemyIdleState(this, _sm);


        State<EnemyStates> patrol = null;
        State <EnemyStates> specificSee = null;
        State <EnemyStates> pursuit = null;
        State<EnemyStates> flee = null;
        State<EnemyStates> arrive = null;

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
            arrive.AddTransition(specificSee, EnemyStates.Idle);
        }
        

        _sm.SetCurrent(idle);
    }

    private void Update()
    {
        _sm.Update();
    }

    private void OnDestroy()
    {
        _sm = null;
    }
}
