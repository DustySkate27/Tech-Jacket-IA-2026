using UnityEditor.Animations;
using UnityEngine;
using System.Collections.Generic;
public enum EntityStates
{
    Idle,
    Patrol,
    Chase,
    Attack
}

public class SimpleFSM : MonoBehaviour
{
    //[SerializeField] EntityStates _currentState;
    [SerializeField] Transform target;
    [SerializeField] Transform[] wayPoints;
    [SerializeField] float speed;
    private int currentWP;
    private float energy = 3;
    [SerializeField] private LineOfSight viewLoS;
    [SerializeField] private LineOfSight attackLoS;
    private Animator animator;
    private AudioSource auso;

    private StateMachine<EntityStates> _sm;

    public float Energy { get { return energy; } set { energy = value; } }
    public float Speed { get { return speed; } set { speed = value; } }
    public Transform Target => target;
    public LineOfSight ViewLoS => viewLoS;
    public LineOfSight AttackLos => attackLoS;
    public Transform[] WayPoints => wayPoints;

    void Start()
    {
        animator = GetComponent<Animator>();
        auso = GetComponent<AudioSource>();

        _sm = new StateMachine<EntityStates>();

        var idle = new EntityIdleState(this, _sm);
        var patrol = new EntityPatrolState(this, _sm);
        var chase = new EntityChaseState(this, _sm);

        idle.AddTransition(patrol, EntityStates.Patrol);

        patrol.AddTransition(idle, EntityStates.Idle);
        patrol.AddTransition(chase, EntityStates.Chase);

        chase.AddTransition(idle, EntityStates.Idle);
        chase.AddTransition(patrol, EntityStates.Patrol);

        _sm.SetCurrent(idle);
    }

    // Update is called once per frame
    void Update()
    {
        _sm.Update();
    }

    private void Attack()
    {
        animator.SetTrigger("Attack");
    }

    public void EndAttack()
    {
        _sm.ChangeState(EntityStates.Chase);
    }
    private void TargetDetected()
    {

        auso.PlayOneShot(auso.clip);
    }
}
