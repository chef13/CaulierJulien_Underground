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
        Controller.SetDestination(waypoints[currentIndex]);
    }

    public override void Update()
    {
        if (Controller.HasReachedDestination())
        {
            currentIndex = (currentIndex + 1) % waypoints.Length;
            Controller.SetDestination(waypoints[currentIndex]);
        }
    }
}
