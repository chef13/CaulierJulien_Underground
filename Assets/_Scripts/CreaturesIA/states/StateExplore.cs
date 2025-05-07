using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.AI.Navigation;
using UnityEngine.AI;

public class StateExplore : CreatureState
{
    public RoomFirstDungeonGenerator roomGenerator = GameObject.FindFirstObjectByType<RoomFirstDungeonGenerator>();
    public NavMeshAgent agent;
    private float exploreRange = 50f;
    public StateExplore(CreatureAI creature) : base(creature) { }

    public override void Enter()
    {
        agent = creature.GetComponent<NavMeshAgent>();
        SetNewDestination();
    }

    public override void Update()
    {
        if (creature.controller.HasReachedDestination() || !creature.controller.hasDestination)
            SetNewDestination();

        // Exemple de transition vers un autre état
        if (DetectEnemy(out GameObject target))
        {
            creature.SwitchState(new StateAttack(creature, target));
        }
    }

    private void SetNewDestination()
    {
        // Assuming neighboringRooms is a list of Vector3 positions for neighboring rooms
        List<Vector2> neighboringRooms = GetNeighboringRooms();

        if (neighboringRooms != null && neighboringRooms.Count > 0)
        {
            
                var potentialDestination = neighboringRooms[Random.Range(0, neighboringRooms.Count)];
                // Check if the destination is walkable using NavMesh
                if (IsWalkable(potentialDestination))
                {
                    Debug.Log($"Setting destination to neighboring room: {potentialDestination}");
                    creature.controller.SetDestination(potentialDestination);
                    return; // Exit after setting a valid destination
                }
            
        }

        // Fallback: Random exploration if no valid neighboring room is found
        for (int i = 0; i < 10; i++) // Try up to 10 times to find a valid random destination
        {
            Vector3 randomDestination = creature.transform.position + new Vector3(
                Random.Range(-exploreRange, exploreRange),
                0,
                Random.Range(-exploreRange, exploreRange)
            );

            if (IsWalkable(randomDestination))
            {
                Debug.Log($"Setting random destination: {randomDestination}");
                creature.controller.SetDestination(randomDestination);
                return;
            }
        }

        Debug.LogWarning("Failed to find a valid random destination.");
    }

    private List<Vector2> GetNeighboringRooms()
    {
        List<Vector2> neighboringRooms = new List<Vector2>();
        
        foreach (var room in roomGenerator.dgRoomslist)
        {
            Vector2 roomPosition = room.transform.position;
            if (Vector2.Distance(creature.transform.position, roomPosition) <= exploreRange && IsWalkable(roomPosition))
            {
                neighboringRooms.Add(roomPosition); // Add the room position to the list
            }
        }

        return neighboringRooms.Count > 0 ? neighboringRooms : null;

        
    }

    private bool DetectEnemy(out GameObject target)
    {
        // Simulation de détection — remplace par ton vrai système
        target = null;
        return false;
    }

    private bool IsWalkable(Vector3 position)
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
