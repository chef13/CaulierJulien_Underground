using UnityEngine;

public class StateExplore : CreatureState
{
    private float exploreRange = 5f;

    public StateExplore(CreatureAI creature) : base(creature) { }

    public override void Enter()
    {
        SetNewDestination();
    }

    public override void Update()
    {
        if (creature.controller.HasReachedDestination())
            SetNewDestination();

        // Exemple de transition vers un autre état
        if (DetectEnemy(out GameObject target))
        {
            creature.SwitchState(new StateAttack(creature, target));
        }
    }

    private void SetNewDestination()
    {
        Vector2 randomOffset = Random.insideUnitCircle * exploreRange;
        Vector2 destination = (Vector2)creature.transform.position + randomOffset;
        creature.controller.SetDestination(destination);
    }

    private bool DetectEnemy(out GameObject target)
    {
        // Simulation de détection — remplace par ton vrai système
        target = null;
        return false;
    }
}
