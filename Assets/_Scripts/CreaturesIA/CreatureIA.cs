using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CreatureController))]
public class CreatureAI : MonoBehaviour
{
    public CreatureState currentState;
    public NavMeshPlus.Components.NavMeshSurface surface;
    [HideInInspector] public CreatureController controller;

    void Awake()
    {
        controller = GetComponent<CreatureController>();
    }

    void Start()
    {
        SwitchState(new StateExplore(this));
    }

    void Update()
    {
        currentState?.Update();
    }

    public void SwitchState(CreatureState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
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
}
