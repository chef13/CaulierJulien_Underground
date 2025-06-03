
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class CreatureState
{
    protected CreatureAI creature;
    private CreatureController controller;
    public CreatureController Controller
    {
        get { return controller; }
        set { controller = value; }
    }

    public CreatureState(CreatureAI creature)
    {
        this.creature = creature;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update() { }

    public CreatureController DetectTreat(out CreatureController target)
    {
        List<CreatureController> treats = new List<CreatureController>();
        target = null;
        for (int i = 0; i < controller.CreaturesInRange.Count; i++)
        {
            if (controller.CreaturesInRange[i] != null &&
                controller.CreaturesInRange[i].currentFaction != controller.currentFaction)
            {
                RaycastHit2D hit = Physics2D.Raycast(controller.transform.position,
                    controller.CreaturesInRange[i].transform.position - controller.transform.position,
                    Vector2.Distance(controller.transform.position, controller.CreaturesInRange[i].transform.position),
                    LayerMask.GetMask("Wall"));
                if (hit.collider == null && !controller.CreaturesInRange[i].isDead)
                    treats.Add(controller.CreaturesInRange[i]);
            }
        }

        if (treats.Count == 0)
        {
            return null;
        }
        // Find the closest treat
        float minDistance = float.MaxValue;
        for (int t = 0; t < treats.Count; t++)
        {
            float dist = controller.GetPathDistance(controller.agent, treats[t].position);
            if (dist < minDistance)
            {
                minDistance = dist;
                target = treats[t];
            }
        }

        return target;
    }

    public bool IsWalkable(Vector2 position)
    {
        NavMeshHit hit;
        // Check if the position is on the NavMesh within a small distance
        if (NavMesh.SamplePosition(position, out hit, 1.0f, NavMesh.AllAreas))
        {
            // Ensure the sampled position is close to the original position
            return Vector3.Distance(position, hit.position) < 0.5f;
        }
        return false;
    }

    public RoomInfo GetCloserHQ()
    {
        float? minDistance = null;
        RoomInfo closerHQ = null;
        for (int i = 0; i < controller.currentFaction.currentHQ.Count; i++)
        {
            float distanceFromHQ =
                controller.GetPathDistance(
                    controller.agent,
                    controller.currentFaction.currentHQ[i].tiles
                    [Random.Range(0, controller.currentFaction.currentHQ[i].tiles.Count)].position);
            if (minDistance == null || distanceFromHQ < minDistance)
            {
                minDistance = distanceFromHQ;
                closerHQ = controller.currentFaction.currentHQ[i];
            }
        }
        return closerHQ;
    }

}