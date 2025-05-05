using UnityEngine;

public class StateAttack : CreatureState
{
    private GameObject target;
    private float attackRange = 1.5f;

    public StateAttack(CreatureAI creature, GameObject target) : base(creature)
    {
        this.target = target;
    }

    public override void Update()
    {
        if (target == null)
        {
            creature.SwitchState(new StateExplore(creature));
            return;
        }

        float distance = Vector2.Distance(creature.transform.position, target.transform.position);

        if (distance > attackRange)
        {
            creature.controller.SetDestination(target.transform.position);
        }
        else
        {
            creature.controller.SetDestination(creature.transform.position);
            Attack();
        }

        // Exemple de condition de fuite
        if (LowHealth())
        {
            creature.SwitchState(new StateFlee(creature));
        }
    }

    private void Attack()
    {
        // Logique d’attaque
    }

    private bool LowHealth()
    {
        // Remplacer par ton système de vie
        return false;
    }
}
