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
        Vector2 directionToTarget = (target.transform.position - creature.transform.position).normalized;

        if (creature.controller.attackTimer <= 0)
        {
            if (distance > creature.controller.attackRange)
            {
                // Move closer to get in range
                creature.controller.SetDestination((Vector2)target.transform.position);
            }
            else
            {
                // In range — attack
                Attack();
            }
        }
        else // on cooldown
        {
            if (distance < creature.controller.attackRange * 0.9f)
            {
                // Too close, back up a little
                Vector2 retreatPos = (Vector2)creature.transform.position - directionToTarget * 1.5f;
                creature.controller.SetDestination(retreatPos);
            }
            else if (distance > creature.controller.attackRange * 1.2f)
            {
                // Too far, move a bit closer
                Vector2 advancePos = (Vector2)creature.transform.position + directionToTarget * 1.5f;
                creature.controller.SetDestination(advancePos);
            }
            else
            {
                // Already in reasonable range — maybe strafe?
                Vector2 strafeDir = Vector2.Perpendicular(directionToTarget).normalized;
                Vector2 strafePos = (Vector2)creature.transform.position + strafeDir * 1f;
                creature.controller.SetDestination(strafePos);
            }
        }

    }

    private void Attack()
    {
        creature.controller.currentEnergy -= creature.controller.damage/5;
        var targetComponent = target.GetComponent<BlopBehaviour>();
        if (targetComponent != null)
        {
            targetComponent.OnHit(creature.controller.damage, this.creature.gameObject);
            creature.controller.attackTimer = creature.controller.attackSpeed;
        }
    }

    
}
