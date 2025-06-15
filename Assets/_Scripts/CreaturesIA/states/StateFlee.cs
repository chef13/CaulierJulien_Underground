using UnityEngine;
using System.Collections.Generic;
using Vector2 = UnityEngine.Vector2;

public class StateFlee : CreatureState
{
    private float fleeDistance = 5f;
    private CreatureController attacker;
    public StateFlee(CreatureAI creature, CreatureController attacker) : base(creature)
    {
        this.attacker = attacker;
    }

    public override void Enter()
    {
        Vector2 away = ((Vector2)creature.transform.position - FindRetreatPosition(attacker)).normalized;
        Vector2 destination = (Vector2)creature.transform.position + away * fleeDistance;
        Controller.SetDestination(destination);
    }

    public override void Exit()
    {
        // Optionally reset any fleeing state or properties here
        attacker = null;
        Controller.SetDestination(Controller.transform.position); // Stop fleeing
        creature.SwitchState(new StateIdle(creature)); // Switch to idle state after fleeing
    }

    public override void Update()
    {
        if (!Controller.hasDestination)
        {
            DetectTreat(out attacker);
            if (attacker != null)
            {
                if (Controller.currentFaction.currentHQ.Count > 0)
                {
                    RoomInfo posHQ = GetCloserHQ();
                    float distance = Vector3.Distance(Controller.transform.position, new Vector3(posHQ.tileCenter.x, posHQ.tileCenter.y, 0f));
                    if (distance < 50f)
                    {
                        // If already close to HQ, just stay there
                        Controller.SetDestination(posHQ.tileCenter);
                        return;
                    }
                }

                    Vector2 retreatPos = FindRetreatPosition(attacker);
                    Vector2 away = ((Vector2)creature.transform.position - retreatPos).normalized;
                    Vector2 destination = (Vector2)creature.transform.position + away * fleeDistance;
                    Controller.SetDestination(destination);
            }
            else
            {
                Exit();
            }
        }
    }

    private Vector2 FindRetreatPosition(CreatureController attacker)
    {
        List<Vector2> potentialRetreats = new List<Vector2>();
        Vector2 currentPos = creature.transform.position;
        Vector2 attackerPos = attacker != null ? (Vector2)attacker.transform.position : currentPos;
        Vector2 fleeDir = (currentPos - attackerPos).normalized;

        // Sample positions along the flee direction up to fleeDistance
        for (float dist = fleeDistance; dist > 0; dist -= 1f)
        {
            Vector2 checkPos = currentPos + fleeDir * dist;
            if (IsWalkable(checkPos))
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
