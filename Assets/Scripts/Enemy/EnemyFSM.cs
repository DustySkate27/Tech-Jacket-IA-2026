using System.Collections.Generic;
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
    Fatigue,
    Arrive,
    Attack
}

public class EnemyFSM : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] public Transform targetTransform;
    [SerializeField] public Rigidbody target;
    [SerializeField, Range(0.0f, 3.0f)] public float targetWeight = 2f;

    [SerializeField] public BoxCollider hurtbox;

    //BasicPatrol
    [SerializeField] public Transform[] wayPoints;
    public int currentWP = 0;

    //ThetaPatrol
    [SerializeField] public List<PF_WNode> nodeList;

    private StateMachine<EnemyStates> _sm;

    //LineOfSight
    [SerializeField] private LineOfSight viewLoS;
    [SerializeField] public LineOfSight specificLoS;

    //TypeObject
    public MeshFilter currentMesh;
    public Mesh baseMesh;
    public Renderer rend;
    public Color idleColor;
    public Color patrolColor;
    public Color specificSeeColor;

    [Header("Escaper Sets")]
    public bool isEscaper;
    public Color fleeColor;
    public Color fatigueColor;
    public Mesh fleeMesh;

    [Header("Chaser Sets")]
    public Color pursuitColor;
    public Color arriveColor;
    public Color attackColor;
    public Mesh pursuitMesh;


    [SerializeField] public float _maxSpeed = 20f;
    [SerializeField] public float _maxForce = 100f;
    [SerializeField] public Vector3 _velocity;
    public Vector3 Velocity { get { return _velocity; } }
    [SerializeField] public float _rotationSpeed = 50f;

    [Header("Pursuit / Evade")]
    [SerializeField] public float maxLookAhead = 1.5f;

    [Header("Obstacle Avoidance")]
    [SerializeField] public float obstacleRadius = 15f;
    [SerializeField] public float obstacleAngle = 180f;
    [SerializeField] public float obstaclePersonalArea = 10f;
    [SerializeField] public LayerMask obstacleMask;
    [SerializeField] public int maxObstacles = 10;

    public ObstacleAvoidance _obstacleAvoidance;
    public Transform myTransform;
    public Vector3 myPosition => myTransform.position;

    public LineOfSight ViewLoS => viewLoS;
    public LineOfSight SpecificLoS => specificLoS;

    private void Awake()
    {
        myTransform = transform;
        baseMesh = currentMesh.mesh;
        idleColor = rend.material.color;

        _obstacleAvoidance = new ObstacleAvoidance(
            myTransform,
            obstacleRadius,
            obstacleAngle,
            obstaclePersonalArea,
            obstacleMask,
            maxObstacles
        );
    }

    private void Start()
    {
        AddForce(new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized * _maxSpeed);

        hurtbox.enabled = false;
        _sm = new StateMachine<EnemyStates>();

        State<EnemyStates> idle = new EnemyIdleState(this, _sm);
        State<EnemyStates> patrol = null;
        State <EnemyStates> specificSee = null;
        State <EnemyStates> pursuit = null;
        State<EnemyStates> flee = null;
        State<EnemyStates> arrive = null;
        State<EnemyStates> attack = null;
        State<EnemyStates> fatigue = null;

        if (isEscaper)
        {
            patrol = new EnemyHidingState(this, _sm);
            specificSee = new EnemyEvadeState(this, _sm);
            flee = new EnemyFleeState(this, _sm);
            fatigue = new EnemyFatigueState(this, _sm);
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

            flee.AddTransition(fatigue, EnemyStates.Fatigue);

            fatigue.AddTransition(idle, EnemyStates.Idle);
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
        
        if (isEscaper)
        {
            _sm.SetCurrent(idle);
        }
        else
        {
            _sm.SetCurrent(idle);
        }
            
    }

    private void Update()
    {
        _sm.Update();
    }

    public Vector3 CalculateSteering(Vector3 desired)
    {
        Vector3 steering = desired - _velocity;
        steering.y = 0;
        return Vector3.ClampMagnitude(steering, _maxForce);
    }
    public void AddForce(Vector3 force)
    {
        force.y = 0;
        _velocity.y = 0;
        _velocity = Vector3.ClampMagnitude(_velocity + force * Time.deltaTime, _maxSpeed);
    }

    private void OnDestroy()
    {
        _sm = null;
    }

}
