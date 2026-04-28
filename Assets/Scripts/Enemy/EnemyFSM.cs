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

    [SerializeField] public Transform[] wayPoints;
    public int currentWP = 0;

    private StateMachine<EnemyStates> _sm;

    [SerializeField] private LineOfSight viewLoS;
    [SerializeField] public LineOfSight specificLoS;

    
    public bool isEscaper;

    public float speed;
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
            patrol = new EnemyStackState(this, _sm);
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
        int count = Physics.OverlapSphereNonAlloc(transform.position, avoidanceRadius, colliders, obsMask); //Detección de colisiones

        Collider nearestColl = null; //Inicializa en nulo la colision más cercana.
        float nearestDistance = float.MaxValue; //Inicializa en "infinito" la distancia a esa colisión.
        Vector3 nearestClosestPoint = Vector3.zero;//Inicializa en nulo la dirección a esa colisión.

        for (int i = 0; i < count; i++) //Recorre count
        {
            Vector3 closestPoint = colliders[i].ClosestPoint(transform.position); //Inicializa una direccion al punto más cercano de un collider
            closestPoint.y = transform.position.y; //Neutraliza la altura, para que no influya en el movimiento.

            Vector3 dirToColl = closestPoint - transform.position; //Inicializa una dirección equivalente a la diferencia al closestPoint
            float distance = dirToColl.magnitude; //Y almacena su magnitud, representando la distancia

            if (distance < nearestDistance) //Si la magnitud es menor al "infinito" ó al nearestDistance "mas cercano" previo. 
            {
                nearestColl = colliders[i]; //Se asigna el collider al que se considera "más cercano" por ahora.
                nearestDistance = distance; //Se almacena la distancia
                nearestClosestPoint = closestPoint; //Se almacena la direccion al punto más cercano de un collider
            }
        }

        if (nearestColl == null) return null; //Si no hay colliders, se devuelve null.

        Vector3 relativePos = transform.InverseTransformPoint(nearestClosestPoint); //Si sí hay colliders, convierte la dirección al punto más cercano de World a Local Space
        Vector3 dirToObstacle = (nearestClosestPoint - transform.position).normalized; //Inicializa la dirección normalizada al obstáculo
        Vector3 avoidDir = relativePos.x < 0 ?  //Evalua por qué lado rodear en funcion de la dirección en Local Space
            Vector3.Cross(transform.up, dirToObstacle) : -Vector3.Cross(transform.up, dirToObstacle);

        //Calcula la "Fuerza de la evasión" por medio de la diferencia entre el radio de evasión y un clampeo de la diferencia entre "más cercana" y "distancia mínima obligatoria" sobre radio de evasión
        float weight = (avoidanceRadius - Mathf.Clamp(nearestDistance - personalArea, 0, avoidanceRadius)) / avoidanceRadius; 
        return avoidDir * weight; //Multiplica la dirección de evasión por la fuerza para respetar la distancia mínima obligatoria.
    }

    private void OnDestroy()
    {
        _sm = null;
    }

}
