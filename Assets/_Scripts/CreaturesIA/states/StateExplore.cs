using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

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
        SetNewDestination();
    }

    public override void Update()
    {
        if (!creature.controller.hasDestination)
        {
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

        foreach (var kvp in DungeonGenerator.Instance.roomsMap)
        {
            Vector3 centerWorld = new Vector3(kvp.Key.x + 0.5f, kvp.Key.y + 0.5f);
            float dist = Vector2.Distance(creature.transform.position, centerWorld);

            if (dist <= exploreRange && !creature.controller.currentFaction.knownRoomsDict.ContainsKey(kvp.Key))
            {
                nearby.Add(kvp.Key);
            }
        }

        return nearby;
    }

    private void ExploreCurrentRoom(RoomInfo currentRoom)
    {
        var faction = creature.controller.currentFaction;

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