using UnityEngine;

public class StatePatrol : CreatureState
{
    private Vector2[] waypoints;
    private int currentIndex = 0;

    public StatePatrol(CreatureAI creature, Vector2[] waypoints) : base(creature)
    {
        this.waypoints = waypoints;
    }

    public override void Enter()
    {
        creature.controller.SetDestination(waypoints[currentIndex]);
    }

    public override void Update()
    {
        if (creature.controller.HasReachedDestination())
        {
            currentIndex = (currentIndex + 1) % waypoints.Length;
            creature.controller.SetDestination(waypoints[currentIndex]);
        }
    }
}
