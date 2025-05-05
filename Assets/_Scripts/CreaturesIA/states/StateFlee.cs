using UnityEngine;

public class StateFlee : CreatureState
{
    private float fleeDistance = 5f;

    public StateFlee(CreatureAI creature) : base(creature) { }

    public override void Enter()
    {
        Vector2 away = ((Vector2)creature.transform.position - FindThreatPosition()).normalized;
        Vector2 destination = (Vector2)creature.transform.position + away * fleeDistance;
        creature.controller.SetDestination(destination);
    }

    public override void Update()
    {
        if (creature.controller.HasReachedDestination())
        {
            creature.SwitchState(new StateExplore(creature));
        }
    }

    private Vector2 FindThreatPosition()
    {
        // Remplacer par ton système de détection
        return creature.transform.position;
    }
}
