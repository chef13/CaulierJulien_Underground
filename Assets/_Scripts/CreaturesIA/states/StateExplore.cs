using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class StateExplore : CreatureState
{
    public NavMeshAgent agent;
    private float exploreRange = 5f;
    private Vector2Int cardinalExplo;
    private RoomInfo targetRoom;

    public StateExplore(CreatureAI creature) : base(creature) { }

    public override void Enter()
    {
        agent = creature.GetComponent<NavMeshAgent>();
        cardinalExplo = Direction2D.cardinalDirectionsList[Random.Range(0, Direction2D.cardinalDirectionsList.Count)];
    }

    public override void Update()
    {


        if (!creature.controller.hasDestination)
        {
            if (creature.controller.currentRoom != null && !creature.controller.currentFaction.knownRoomsDict.ContainsKey(creature.controller.currentRoom.position))
            {
                ExploreCurrentRoom(creature.controller.currentRoom);
            }
            else if (creature.controller.currentRoom == null || creature.controller.currentFaction.knownRoomsDict.ContainsKey(creature.controller.currentRoom.position))
            {
                SetNewDestination();
            }
        }

        if (DetectEnemy(out GameObject target))
            {
                creature.SwitchState(new StateAttack(creature, target));
            }
    }

    public override void SetNewDestination()
    {
        
        var neighboringRooms = GetNeighboringRooms();

        if (neighboringRooms.Count > 0)
        {
            Debug.Log($"getting new room loc");
            var randomNearby = neighboringRooms[Random.Range(0, neighboringRooms.Count)];

                Vector2 worldPos = new Vector2(randomNearby.position.x + 0.5f, randomNearby.position.y + 0.5f);
            targetRoom = randomNearby;
                if (creature.IsWalkable(worldPos))
                {
                    creature.controller.SetDestination(worldPos);
                    return;
                }
            
        }
        else
        {
            Debug.Log($"getting new random loc");
            // Fallback: pick random direction and walk
            Vector2Int origin = Vector2Int.RoundToInt(creature.transform.position);
            for (int i = 0; i < 4; i++)
            {
                Vector2Int check = origin + cardinalExplo * 2;
                Vector3 worldCheck = new Vector3(check.x + 0.5f, check.y + 0.5f);

                if (creature.IsWalkable(worldCheck))
                {
                    creature.controller.SetDestination(new Vector2(worldCheck.x, worldCheck.y));
                    return;
                }
                else
                {
                    cardinalExplo = Direction2D.cardinalDirectionsList[Random.Range(0, Direction2D.cardinalDirectionsList.Count)];
                }
            }
        }

        Debug.LogWarning("⚠️ No valid destination found.");
    }

    private List<RoomInfo> GetNeighboringRooms()
    {
        var nearby = new List<RoomInfo>();
        var knownRooms = creature.controller.currentFaction.knownRoomsDict;
        var currentRoom = creature.controller.currentRoom;

        if (currentRoom != null && currentRoom.connectedRooms != null)
        {
            foreach (RoomInfo connected in currentRoom.connectedRooms)
            {
                if (!knownRooms.ContainsKey(connected.position))
                {
                    nearby.Add(connected);
                }
            }
        }
        else
        {
            foreach (RoomInfo known in knownRooms.Values)
            {
                if (known.connectedRooms == null) continue;

                foreach (RoomInfo connected in known.connectedRooms)
                {
                    if (!knownRooms.ContainsKey(connected.position))
                    {
                        nearby.Add(connected);
                    }
                }
            }
        }

        return nearby;
    }


        private void ExploreCurrentRoom(RoomInfo Room)
    {
        Debug.Log($"exploring room");
        var faction = creature.controller.currentFaction;

        // Find unexplored tiles
        List<TileInfo> unexplored = new List<TileInfo>();
        foreach (TileInfo tile in Room.tiles)
        {
            if (!faction.knownTilesDict.ContainsKey(tile.position))
            {
                unexplored.Add(tile);
            }
        }

        if (unexplored.Count > 0)
        {
            TileInfo targetTile = unexplored[Random.Range(0, unexplored.Count)];
            Vector2 destination = new Vector2(targetTile.position.x + 0.5f, targetTile.position.y + 0.5f);
            creature.controller.SetDestination(destination);
        }
        else if (!faction.knownRoomsDict.ContainsKey(Room.position))
        {
            faction.knownRoomsDict[Room.position] = Room;
            Debug.Log($"🧭 {faction.name} fully explored room at {Room.position}");
            faction.rooms++;
        }
    }
}
