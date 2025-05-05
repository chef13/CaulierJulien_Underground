using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class CreatureController : MonoBehaviour
{
    public float stoppingDistance = 0.1f;

    protected NavMeshAgent agent;
    protected bool hasDestination = false;

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        // Important pour la 2D
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    protected virtual void Update()
    {
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
        // À override dans les classes enfants
    }
}
