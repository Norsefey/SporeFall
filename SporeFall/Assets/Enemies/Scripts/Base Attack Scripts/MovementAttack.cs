using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract  class MovementAttack : Attack
{
    public override AttackType AttackType => AttackType.MovementAttack;
    public bool IsMovementAttack => true;


    public override void Execute(AttackInstance instance, Damageable target)
    {
        var driver = instance.Owner.gameObject.GetComponent<MovementAttackDriver>()
                      ?? instance.Owner.gameObject.AddComponent<MovementAttackDriver>();

        driver.Begin(this, instance, target);
    }

    public override IEnumerator ExecuteAttack(BaseEnemy enemy, Transform target, float damageModifier, float corruptionModifier)
    {
        throw new System.NotImplementedException();
    }

    public abstract void BeginMovement(MovementAttackDriver ctx);
    public abstract bool TickMovement(MovementAttackDriver ctx, float dt);
    public abstract void OnImpact(MovementAttackDriver ctx);

}
