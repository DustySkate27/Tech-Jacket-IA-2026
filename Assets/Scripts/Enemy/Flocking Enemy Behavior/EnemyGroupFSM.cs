using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGroupFSM : MonoBehaviour
{
    [SerializeField] public float _maxSpeed = 20f;
    [SerializeField] public float _maxForce = 100f;
    [SerializeField] public Vector3 _velocity;
    public Vector3 Velocity { get { return _velocity; } }
    [SerializeField] public float _rotationSpeed = 20f;

    [SerializeField] public float separationRadius = 4f;
    [SerializeField] public float cohesionRadius = 12f;

    [SerializeField, Range(0.0f, 3.0f)] public float separationWeight = 2f;
    [SerializeField, Range(0.0f, 3.0f)] public float cohesionWeight = 1f;
    [SerializeField, Range(0.0f, 3.0f)] public float alignmentWeight = 1.5f;

    [SerializeField] public LayerMask boidMask;

    [Header("Target")]
    [SerializeField] public Rigidbody target;
    [SerializeField, Range(0.0f, 3.0f)] public float targetWeight = 2f;

    [Header("WayPoints")]
    [SerializeField] public Transform[] wayPoints;

    [Header("Obstacle Avoidance")]
    [SerializeField] public float obstacleRadius = 15f;
    [SerializeField] public float obstacleAngle = 180f;
    [SerializeField] public float obstaclePersonalArea = 10f;
    [SerializeField] public LayerMask obstacleMask;
    [SerializeField] public int maxObstacles = 10;

    public ObstacleAvoidance _obstacleAvoidance;
    public Transform myTransform;
    [SerializeField] public Collider myCollider;
    public Vector3 myPosition => myTransform.position;

    [SerializeField] private LineOfSight viewLoS;
    public LineOfSight ViewLoS => viewLoS;

    private StateMachine<EnemyStates> _sm;

    private void Awake()
    {
        myTransform = transform;

        _obstacleAvoidance = new ObstacleAvoidance(
            myTransform,
            obstacleRadius,
            obstacleAngle,
            obstaclePersonalArea,
            obstacleMask,
            maxObstacles
        );
    }

    void Start()
    {
        AddForce(new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized * _maxSpeed);

        _sm = new StateMachine<EnemyStates>();

        State<EnemyStates> seek = new EnemyGroupSeekState(this, _sm);
        State<EnemyStates> arrive = new EnemyGroupArriveState(this, _sm);
        State<EnemyStates> pursuit = new EnemyGroupPursuitState(this, _sm);
        State<EnemyStates> patrol = new EnemyGroupPatrolState(this, _sm);

        patrol.AddTransition(pursuit, EnemyStates.Pursuit);
        pursuit.AddTransition(arrive, EnemyStates.Arrive);
        arrive.AddTransition(pursuit, EnemyStates.Pursuit);

        _sm.SetCurrent(patrol);
    }

    void Update()
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, separationRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, cohesionRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, obstacleRadius);
    }
}
