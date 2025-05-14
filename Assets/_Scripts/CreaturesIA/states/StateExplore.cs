using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.AI.Navigation;
using UnityEngine.AI;
using UnityEngine.Tilemaps;

public class StateExplore : CreatureState
{
    public RoomFirstDungeonGenerator roomGenerator = GameObject.FindFirstObjectByType<RoomFirstDungeonGenerator>();
    public NavMeshAgent agent;
    private float exploreRange = 50f;
    public GameObject currentRoom;
    List<TileInfo> tilesInSight = new List<TileInfo>();
    public StateExplore(CreatureAI creature) : base(creature) { }

    public override void Enter()
    {
        /*foreach (var room in roomGenerator.rooms)
        {
            RoomInfo roomInfo = room.GetComponent<RoomInfo>(); // Make sure RoomInfo inherits from MonoBehaviour
            if (roomInfo == null)
            {
                Debug.LogError("RoomInfo component missing on: " + room.name);
                continue;
            }
            foreach (var tile in roomInfo.positions)
            {
                if (tile.position == Vector2Int.RoundToInt(creature.transform.position))
                {
                    // If the tile is in the room, set it as the current room
                    currentRoom = room;
                    break;
                }
            }
        }
        Debug.Log("Current room: " + currentRoom.name);*/
        agent = creature.GetComponent<NavMeshAgent>();
        SetNewDestination();
    }

    public override void Update()
    {
        if (creature.controller.HasReachedDestination() || !creature.controller.hasDestination)
        {
            //if (creature.controller.currentFaction.knowRooms.Contains(currentRoom))
            SetNewDestination();
            /*else
            ExploreCurrentRoom(currentRoom);*/
        }

        // Exemple de transition vers un autre état
        if (DetectEnemy(out GameObject target))
        {
            Debug.Log("Enemy detected, switching to StateAttack!");
            creature.SwitchState(new StateAttack(creature, target));
        }

        // Check if the surrounding tiles in detection range are known, if not add them to knownTiles
        RegisterNewTiles();



    }

    private void SetNewDestination()
    {
        // Assuming neighboringRooms is a list of Vector3 positions for neighboring rooms
        List<GameObject> neighboringRooms = GetNeighboringRooms();

        if (neighboringRooms != null && neighboringRooms.Count > 0)
        {
            
                var potentialRoom = neighboringRooms[Random.Range(0, neighboringRooms.Count)];
                Vector3 potentialDestination = potentialRoom.transform.position;
                // Check if the destination is walkable using NavMesh
                if (creature.IsWalkable(potentialDestination))
                {
                    
                    creature.controller.SetDestination(potentialDestination);
                    return; // Exit after setting a valid destination
                }
            
        }

        List<TileInfo> knownTiles = creature.controller.currentFaction.knownTiles;
        // Fallback: Random exploration if no valid neighboring room is found
        if (knownTiles != null && knownTiles.Count > 0)
        {
            for (int i = 0; i < 10; i++) // Try up to 10 times to find a valid random destination
            {
                // Initialize min and max with the first tile's position
                Vector2Int min = knownTiles[0].position;
                Vector2Int max = knownTiles[0].position;

                // Find the min and max coordinates
                foreach (var tile in knownTiles)
                {
                    min = Vector2Int.Min(min, tile.position);
                    max = Vector2Int.Max(max, tile.position);
                }

                // Expand the bounds slightly to encourage exploration outside known area
                int expand = 2;
                min -= Vector2Int.one * expand;
                max += Vector2Int.one * expand;

                // Pick a random position near the edge of the bounds
                Vector2Int randomEdgePos = new Vector2Int(
                    Random.Range(min.x, max.x + 1),
                    Random.Range(min.y, max.y + 1)
                );

                Vector3 worldDestination = new Vector3(randomEdgePos.x + 0.5f, randomEdgePos.y + 0.5f, 0);

                // Check if the destination is walkable
                if (creature.IsWalkable(worldDestination))
                {
                    creature.controller.SetDestination(worldDestination);
                    return;
                }
            }
        }

        Debug.LogWarning("Failed to find a valid random destination.");
    }

    private List<GameObject> GetNeighboringRooms()
    {
        List<GameObject> neighboringRooms = new List<GameObject>();
        
        foreach (var room in roomGenerator.rooms)
        {
            Vector2 roomPosition = room.transform.position;
            if (Vector2.Distance(creature.transform.position, roomPosition) <= exploreRange && creature.IsWalkable(roomPosition))
            {
                neighboringRooms.Add(room); // Add the room position to the list
            }
        }

        return neighboringRooms.Count > 0 ? neighboringRooms : null;

        
    }

    private GameObject DetectEnemy(out GameObject target)
    {
        // Example: detect enemies within a radius using Physics.OverlapSphereNonAlloc
        float detectionRadius = 10f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(creature.transform.position, detectionRadius);
        int hitCount = hits.Length; // Get the number of hits
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Blop"))
            {
                target = hit.gameObject;
                if (target.activeInHierarchy)
                {return target;}
                else
                {target = null;
                return target;}
            }
        }
        target = null;
        return target;
    }

    /*private void ExploreCurrentRoom(GameObject currentRoom)
    {
        RoomInfo roomInfo = currentRoom.GetComponent<RoomInfo>();
        List<TileInfo> tiles = roomInfo.positions;
        if (roomInfo.positions == null)
        {
            Debug.LogError("RoomInfo.positions is null on: " + currentRoom.name);
            return;
        }
        for (int i = 0; i < 10; i++) // Try up to 10 times to find a valid random destination
        {
                    // Initialize min and max with the first tile's position
            Vector2Int min = tiles[0].position;
            Vector2Int max = tiles[0].position;

            // Find the min and max coordinates
            foreach (var tile in tiles)
            {
                min = Vector2Int.Min(min, tile.position);
                max = Vector2Int.Max(max, tile.position);
            }

            // Expand the bounds slightly to encourage exploration outside known area
            int expand = 2;
            min -= Vector2Int.one * expand;
            max += Vector2Int.one * expand;

            // Pick a random position near the edge of the bounds
            Vector2Int randomEdgePos = new Vector2Int(
                Random.Range(min.x, max.x + 1),
                Random.Range(min.y, max.y + 1)
            );

            Vector3 worldDestination = new Vector3(randomEdgePos.x + 0.5f, randomEdgePos.y + 0.5f, 0);

            // Check if the destination is walkable
            if (creature.IsWalkable(worldDestination))
            {
                creature.controller.SetDestination(worldDestination);
            }
        }

        // Check if all tiles in the room are known
        bool allTilesKnown = roomInfo.positions.TrueForAll(tile => creature.controller.currentFaction.knownTiles.Exists(knownTile => knownTile.position == tile.position));
        if (allTilesKnown)
        {
            // All tiles in the room are known, you can add logic here if needed
            creature.controller.currentFaction.knowRooms.Add(currentRoom);        
        }

       
    }*/

    private void RegisterNewTiles()
    {
        Vector2Int creaturePos = Vector2Int.RoundToInt(creature.transform.position);
        tilesInSight.Clear();
        float detectionRange = creature.controller.detectionRange;
        List<TileInfo> knownTiles = creature.controller.currentFaction.knownTiles;

        for (int dx = -Mathf.CeilToInt(detectionRange); dx <= Mathf.CeilToInt(detectionRange); dx++)
        {
            for (int dy = -Mathf.CeilToInt(detectionRange); dy <= Mathf.CeilToInt(detectionRange); dy++)
            {
                Vector2Int checkPos = creaturePos + new Vector2Int(dx, dy);
                if (Vector2.Distance(creaturePos, checkPos) <= detectionRange)
                {
                    // Check if this tile exists in the dungeon's tile list
                    TileInfo tile = roomGenerator.tilesInfos.Find(t => t.position == checkPos);
                    if (tile != null)
                    {
                        tilesInSight.Add(tile);
                    }
                }
            }
        }

        // Now you can compare tilesInSight with knownTiles
        foreach (var tile in tilesInSight)
        {
            if (!knownTiles.Exists(t => t.position == tile.position))
            {
                creature.controller.currentFaction.knownTiles.Add(tile);
            }
        }
    }

    private void RegisterNewRoom()
    {
        
    }
    
}
