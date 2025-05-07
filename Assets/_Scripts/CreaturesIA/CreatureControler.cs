using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class CreatureController : MonoBehaviour
{
    public NavMeshPlus.Components.NavMeshSurface surface;
    public float stoppingDistance = 1f;
    public Vector2 destination;
    protected NavMeshAgent agent;
    public bool hasDestination = false;

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        // Important pour la 2D
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    protected virtual void Update()
    {
        Debug.Log(agent.isOnNavMesh);
        Debug.Log(agent.Raycast(agent.transform.position, out NavMeshHit hit));
        if (hasDestination && agent.remainingDistance <= stoppingDistance)
        {
            hasDestination = false;
            OnDestinationReached();
        }

    }

    public virtual void SetDestination(Vector2 destination)
    {
        hasDestination = true;
        agent.SetDestination(destination);
    }

    public virtual bool HasReachedDestination()
    {
        return !hasDestination;
    }

    protected virtual void OnDestinationReached()
    {
        agent.SetDestination(destination);
    }
}
