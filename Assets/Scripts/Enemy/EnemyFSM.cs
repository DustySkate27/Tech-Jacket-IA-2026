using UnityEngine;

public enum EnemyStates  
{
    Idle,
    Patrol,
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

    public float slowingRadius = 3f;


    public LineOfSight ViewLoS => viewLoS;
    public LineOfSight SpecificLoS => specificLoS;

    private void Start()
    {
        _sm = new StateMachine<EnemyStates>();

        obsAvoid = new EnemyObstacleAvoidance(transform, hitboxRadius, hitboxAngle, hitboxOffset, obsMask, maxAvoidableObs);

        State<EnemyStates> idle = new EnemyIdleState(this, _sm);
        State<EnemyStates> patrol = new EnemyPatrolState(this, _sm);

        State<EnemyStates> specificSee = null;
        State<EnemyStates> specificHaveBeenSeen = null;

        if (isEscaper)
        {
            specificSee = new EnemyEvadeState(this, _sm);
            specificHaveBeenSeen = new EnemyFleeState(this, _sm);
        }
        else
        {

        }

        idle.AddTransition(patrol, EnemyStates.Patrol);

        patrol.AddTransition(idle, EnemyStates.Idle);
        patrol.AddTransition(specificSee, EnemyStates.SpecificSee);
        patrol.AddTransition(specificHaveBeenSeen, EnemyStates.SpecificBeenSeen);

        idle.AddTransition(specificSee, EnemyStates.SpecificSee);
        idle.AddTransition(specificHaveBeenSeen, EnemyStates.SpecificBeenSeen);

        specificSee.AddTransition(idle, EnemyStates.Idle);
        specificHaveBeenSeen.AddTransition(idle, EnemyStates.Idle);

        _sm.SetCurrent(idle);
    }

    private void Update()
    {
        _sm.Update();
    }

}
