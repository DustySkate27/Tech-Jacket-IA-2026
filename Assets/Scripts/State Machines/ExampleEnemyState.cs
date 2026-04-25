//using UnityEngine;
//using static UnityEngine.EventSystems.EventTrigger;
//using static UnityEngine.GraphicsBuffer;

//public class EntityIdleState : EntityState
//{
//    private SimpleFSM entity;
//    public EntityIdleState(SimpleFSM entity, StateMachine<EntityStates> sm) : base(sm)
//    {
//        this.entity = entity;
//    }
//    public override void Execute()
//    {
//        base.Execute();
//        Idle();
//    }

//    private void Idle()
//    {
//        entity.Energy += Time.deltaTime * 4;

//        if (entity.Energy >= 10)
//        {
//            if (entity.ViewLoS.CheckRange(entity.Target) && entity.ViewLoS.CheckAngle(entity.Target) && entity.ViewLoS.CheckView(entity.Target))
//                _sm.ChangeState(EntityStates.Chase);
//            else
//                _sm.ChangeState(EntityStates.Patrol);

//        }
//    }
//}
