using UnityEngine;
using UnityEngine.UIElements;

public class Boid : SteeringEntity
{
    [SerializeField] private float separationRadius;
    [SerializeField] private float cohesionRadius;

    [SerializeField, Range(0.0f, 3.0f)] private float separationWeight;
    [SerializeField, Range(0.0f, 3.0f)] private float cohesionWeight;
    [SerializeField, Range(0.0f, 3.0f)] private float alignmentWeight;

    [SerializeField] private LayerMask boidMask;

    [Header("Target")]
    [SerializeField] private Rigidbody target;
    [SerializeField, Range(0.0f, 3.0f)] private float targetWeight = 1f;

    [Header("Obstacle Avoidance")]
    [SerializeField] private float obstacleRadius = 3f;
    [SerializeField] private float obstacleAngle = 90f;
    [SerializeField] private float obstaclePersonalArea = 0.5f;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private int maxObstacles = 5;

    private ObstacleAvoidance _obstacleAvoidance;
    private Transform myTransform;
    private Vector3 myPosition => myTransform.position;

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
    }

    void Update()
    {
        Flocking();
        MoveWithAvoidance();   // ← reemplaza Move()
    }

    // Sobreescribe el movimiento para desviar la dirección antes de mover
    private void MoveWithAvoidance()
    {
        if (_velocity == Vector3.zero) return;

        Vector3 flatVelocity = _velocity;
        flatVelocity.y = 0;

        Vector3 deflectedDir = _obstacleAvoidance.GetDir(flatVelocity.normalized, calculateY: false);
        deflectedDir.y = 0;
        if (deflectedDir == Vector3.zero) deflectedDir = flatVelocity.normalized;
        deflectedDir.Normalize();

        Vector3 moveVelocity = deflectedDir * flatVelocity.magnitude;

        if (deflectedDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(deflectedDir);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                _rotationSpeed * Time.deltaTime
            );
        }

        transform.position += moveVelocity * Time.deltaTime;
        _velocity.y = 0;
    }

    private void Flocking()
    {
        Vector3 seekForce = (target != null)
            ? Seek(target.position) * targetWeight
            : Vector3.zero;

        AddForce(
            Separation() * separationWeight
            + Cohesion() * cohesionWeight
            + Alignment() * alignmentWeight
            + seekForce
        );
    }

    private Vector3 Separation()
    {
        var boidsInRange = Physics.OverlapSphere(myPosition, separationRadius, boidMask);
        Vector3 totalForce = Vector3.zero;
        int cont = 0;
        for (int i = 0; i < boidsInRange.Length; i++)
        {
            var currentBoid = boidsInRange[i];
            if (currentBoid == this.GetComponent<Collider>()) continue;

            var direction = myPosition - currentBoid.transform.position;
            var force = direction.normalized / (direction.magnitude / separationRadius);

            totalForce += force;
            cont++;
        }
        if (cont == 0) return Vector3.zero;

        totalForce /= cont;

        return CalculateSteering(totalForce * _maxSpeed);
    }

    private Vector3 Cohesion()
    {
        var avgPosition = Vector3.zero;
        int cont = 0;
        var boidsInRange = Physics.OverlapSphere(myPosition, cohesionRadius, boidMask);

        for (int i = 0; i < boidsInRange.Length; i++)
        {
            var currentBoid = boidsInRange[i];
            if (currentBoid == this.GetComponent<Collider>()) continue;

            avgPosition += currentBoid.transform.position;
            cont++;
        }
        if (cont == 0) return Vector3.zero;

        avgPosition /= cont;
        return Seek(avgPosition);
    }

    private Vector3 Alignment()
    {
        var boidsInRange = Physics.OverlapSphere(myPosition, cohesionRadius, boidMask);
        Vector3 avgVelocity = Vector3.zero;
        int cont = 0;
        for (int i = 0; i < boidsInRange.Length; i++)
        {
            var currentBoid = boidsInRange[i].GetComponent<Boid>();
            if (currentBoid == this) continue;


            avgVelocity += currentBoid.transform.forward;
            cont++;
        }
        if (cont == 0) return Vector3.zero;

        return CalculateSteering(avgVelocity.normalized * _maxSpeed);
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
