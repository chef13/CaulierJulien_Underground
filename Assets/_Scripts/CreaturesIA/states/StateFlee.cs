using UnityEngine;
using System.Collections.Generic;

public class StateFlee : CreatureState
{
    private float fleeDistance = 5f;
    private GameObject attacker;
    public StateFlee(CreatureAI creature, GameObject attacker) : base(creature)
    {
        this.attacker = attacker;
    }

    public override void Enter()
    {
        Vector2 away = ((Vector2)creature.transform.position - FindRetreatPosition(attacker)).normalized;
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

    private Vector2 FindRetreatPosition(GameObject attacker)
    {
        List<Vector2> potentialRetreats = new List<Vector2>();
        Vector2 currentPos = creature.transform.position;
        Vector2 attackerPos = attacker != null ? (Vector2)attacker.transform.position : currentPos;
        Vector2 fleeDir = (currentPos - attackerPos).normalized;

        // Sample positions along the flee direction up to fleeDistance
        for (float dist = fleeDistance; dist > 0; dist -= 1f)
        {
            Vector2 checkPos = currentPos + fleeDir * dist;
            if (creature.IsWalkable(checkPos))
            {
                potentialRetreats.Add(checkPos);
            }
        }

        // Return the furthest valid position, or current position if none found
        if (potentialRetreats.Count > 0)
            return potentialRetreats[0];
        else
            return currentPos;
    }
}
