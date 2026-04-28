using UnityEngine;

public class EnemyAttackState : State<EnemyStates>
{
    EnemyFSM fsm;
    BoxCollider hurtbox;

    float attackTimer;

    float windup = 0.4f;
    float activeTime = 0.2f;
    float recovery = 0.5f;

    public EnemyAttackState(EnemyFSM fsm,StateMachine<EnemyStates> sm) : base(sm)
    {
        this.fsm = fsm;
        this.hurtbox = fsm.hurtbox;
    }

    public override void Awake()
    {
        attackTimer = 0;
        hurtbox.enabled = false;
    }

    public override void Execute()
    {
        base.Execute();

        Attack();
    }

    void Attack()
    {
        attackTimer += Time.deltaTime;

        // Windup
        if (attackTimer < windup)
            return;
        // Active frames
        if (attackTimer < windup + activeTime)
        {
            hurtbox.enabled = true;
            return;
        }
        // Recovery
        hurtbox.enabled = false;

        if (attackTimer >= windup + activeTime + recovery)
        {
            EndAttack();
        }
    }
    void EndAttack()
    {
        if (fsm.SpecificLoS.CheckRange(fsm.target) && fsm.SpecificLoS.CheckAngle(fsm.target) && fsm.SpecificLoS.CheckView(fsm.target))
            _sm.ChangeState(EnemyStates.Pursuit);
        else
            _sm.ChangeState(EnemyStates.Idle);
    }


}
