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


    public float maxForce = 5f;
    public float rotationSpeed = 5f;
    public float predictionFactor = 0.05f;

    public float slowingRadius = 15f;

    private Collider[] colliders;
    public float personalArea;
    public float avoidanceRadius;
    public int colliderCapacity;
    public LayerMask obsMask;

    public LineOfSight ViewLoS => viewLoS;
    public LineOfSight SpecificLoS => specificLoS;

    private void Start()
    {
        colliders = new Collider[colliderCapacity];
        hurtbox.enabled = false;
        _sm = new StateMachine<EnemyStates>();

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

    public Vector3? ComputeAvoidance()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, avoidanceRadius, colliders, obsMask);

        Collider nearestColl = null;
        float nearestDistance = float.MaxValue;
        Vector3 nearestClosestPoint = Vector3.zero;

        for (int i = 0; i < count; i++)
        {
            Vector3 closestPoint = colliders[i].ClosestPoint(transform.position);
            closestPoint.y = transform.position.y;

            Vector3 dirToColl = closestPoint - transform.position;
            float distance = dirToColl.magnitude;

            if (distance < nearestDistance)
            {
                nearestColl = colliders[i];
                nearestDistance = distance;
                nearestClosestPoint = closestPoint;
            }
        }

        if (nearestColl == null) return null;

        Vector3 relativePos = transform.InverseTransformPoint(nearestClosestPoint);
        Vector3 dirToObstacle = (nearestClosestPoint - transform.position).normalized;
        Vector3 avoidDir = relativePos.x < 0
            ? Vector3.Cross(transform.up, dirToObstacle)
            : -Vector3.Cross(transform.up, dirToObstacle);

        float weight = (avoidanceRadius - Mathf.Clamp(nearestDistance - personalArea, 0, avoidanceRadius)) / avoidanceRadius;

        return avoidDir * weight;
    }

    private void OnDestroy()
    {
        _sm = null;
    }

}
