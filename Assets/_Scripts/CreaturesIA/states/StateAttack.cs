using UnityEngine;

public class StateAttack : CreatureState
{
    private GameObject target;

    public StateAttack(CreatureAI creature, GameObject target) : base(creature)
    {
        this.target = target;
    }

    public override void Update()
    {
        if (!target.activeInHierarchy)
        {
            target = null;
            creature.SwitchState(new StateExplore(creature));
            return;
        }

        float distance = Vector2.Distance(creature.transform.position, target.transform.position);

        if (distance > creature.controller.attackRange)
        {
            creature.controller.SetDestination(target.transform.position);
        }
        else if (creature.controller.attackTimer <= 0)
        {
            creature.controller.SetDestination(creature.transform.position);
            Attack();
        }

    }

    private void Attack()
    {
        creature.controller.currentEnergy -= creature.controller.damage/2;
        var targetComponent = target.GetComponent<BlopBehaviour>();
        if (targetComponent != null)
        {
            targetComponent.OnHit(creature.controller.damage, this.creature.gameObject);
            creature.controller.attackTimer = creature.controller.attackSpeed;
        }
    }

    
}
