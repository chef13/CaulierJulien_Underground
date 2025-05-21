using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine.Tilemaps;

public class StateExplore : CreatureState
{
    public NavMeshAgent agent;
    private float exploreRange = 5f;
    private Vector2Int cardinalExplo;

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
            if (creature.controller.currentRoom != null &&
                !creature.controller.currentFaction.knownRoomsDict.ContainsValue(creature.controller.currentRoom))
            {
                ExploreCurrentRoom(creature.controller.currentRoom);
            }

            if (creature.controller.currentRoom == null ||
                creature.controller.currentFaction.knownRoomsDict.ContainsValue(creature.controller.currentRoom))
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
        var neighboringRooms = GetNeighboringRoomCenters();

        if (neighboringRooms.Count > 0)
        {
            var randomCenter = neighboringRooms[Random.Range(0, neighboringRooms.Count)];
            if (DungeonGenerator.Instance.roomsMap.TryGetValue(randomCenter, out RoomInfo targetRoom))
            {
                TileInfo randomTile = targetRoom.tiles[Random.Range(0, targetRoom.tiles.Count)];
                Vector2 worldPos = new Vector2(randomTile.position.x + 0.5f, randomTile.position.y + 0.5f);

                if (creature.IsWalkable(worldPos))
                {
                    creature.controller.SetDestination(worldPos);
                    return;
                }
            }
        }

        // Fallback: pick random direction and walk
        Vector2Int origin = Vector2Int.RoundToInt(creature.transform.position);
        for (int i = 0; i < 4; i++)
        {
            Vector2Int check = origin + cardinalExplo * 2;
            Vector3 worldCheck = new Vector3(check.x + 0.5f, check.y + 0.5f);

            if (creature.IsWalkable(worldCheck))
            {
                creature.controller.SetDestination(worldCheck);
                return;
            }
            else
            {
                cardinalExplo = Direction2D.cardinalDirectionsList[Random.Range(0, Direction2D.cardinalDirectionsList.Count)];
            }
        }

        Debug.LogWarning("⚠️ No valid destination found.");
    }

    private List<Vector3Int> GetNeighboringRoomCenters()
    {
        var nearby = new List<Vector3Int>();

        if (creature.controller.currentRoom != null && creature.controller.currentRoom.connectedRooms.Count > 0)
        {
            foreach (RoomInfo room in creature.controller.currentRoom.connectedRooms)
            {
                if (!creature.controller.currentFaction.knownRoomsDict.ContainsValue(room))
                {
                    nearby.Add(room.position);
                }
            }
        }
        else if (creature.controller.currentRoom == null)
            foreach (RoomInfo room in creature.controller.currentFaction.knownRoomsDict.Values)
            {
                if (room.connectedRooms != null)
                {
                    foreach (RoomInfo roomNeighbore in room.connectedRooms)
                    {
                        if (!creature.controller.currentFaction.knownRoomsDict.ContainsValue(roomNeighbore))
                        {
                            nearby.Add(roomNeighbore.position);
                        }
                    }
                }
            }

        return nearby;
    }

    private void ExploreCurrentRoom(RoomInfo currentRoom)
    {
         var faction = creature.controller.currentFaction;
        List<TileInfo> unexplo = new List<TileInfo>();

        foreach (TileInfo kvp in currentRoom.tiles)
        {
            if (!faction.knownTilesDict.ContainsKey(kvp.position))
            {
                creature.controller.SetDestination(new Vector2(kvp.position.x, kvp.position.y));
            }
        }

       

        bool allTilesKnown = true;

        foreach (TileInfo tile in currentRoom.tiles)
        {
            if (!faction.knownTilesDict.ContainsKey(tile.position))
            {
                allTilesKnown = false;
                break;
            }
        }

        if (allTilesKnown && !faction.knownRoomsDict.ContainsValue(currentRoom))
        {
            faction.knownRoomsDict[currentRoom.position] = currentRoom;
            Debug.Log($"🧭 {faction.name} fully explored room at {currentRoom.position}");
        }
    }
}