using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using System.Collections;

public class StateExplore : CreatureState
{
    public NavMeshAgent agent;
    private Vector2Int cardinalExplo;
    private RoomInfo targetRoom;
    private Coroutine exploreCurrentRoomCoroutine;
    public StateExplore(CreatureAI creature) : base(creature) 
    {
    }

    public override void Enter()
    {
        
        agent = creature.GetComponent<NavMeshAgent>();
        cardinalExplo = Direction2D.cardinalDirectionsList[Random.Range(0, Direction2D.cardinalDirectionsList.Count)];
    }

    public override void Update()
    {


        if (!Controller.hasDestination)
        {
            if (Controller.currentRoom != null && !Controller.currentFaction.knownRoomsDict.ContainsKey(Controller.currentRoom.index))
            {
                exploreCurrentRoomCoroutine = creature.StartCoroutine(ExploreCurrentRoom(Controller.currentRoom));
            }
            else if (Controller.currentRoom == null || Controller.currentFaction.knownRoomsDict.ContainsKey(Controller.currentRoom.index))
            {
                SetNewDestination();
            }
        }

        if (Controller.data.carnivor && Controller.currentHungerState != CreatureController.hungerState.Full && DetectTreat(out CreatureController target))
        {
            creature.SwitchState(new StateAttack(creature, target));
        }

    }

    public void SetNewDestination()
    {
        
        var neighboringRooms = GetNeighboringRooms();

        if (neighboringRooms.Count > 0)
        {
            //Debug.Log($"getting new room loc");
            var randomNearby = neighboringRooms[Random.Range(0, neighboringRooms.Count)];
            var randomNearbyTiles = randomNearby.tiles[Random.Range(0, randomNearby.tiles.Count)];

                Vector2 worldPos = new Vector2(randomNearbyTiles.position.x + 0.5f, randomNearbyTiles.position.y + 0.5f);
            targetRoom = randomNearby;
                if (IsWalkable(worldPos))
                {
                    Controller.SetDestination(worldPos);
                    return;
                }
            
        }
        else
        {
            //Debug.Log($"getting new random loc");
            // Fallback: pick random direction and walk
            Vector2Int origin = Vector2Int.RoundToInt(creature.transform.position);
            for (int i = 0; i < 4; i++)
            {
                Vector2Int check = origin + cardinalExplo * 2;
                Vector3 worldCheck = new Vector3(check.x + 0.5f, check.y + 0.5f);

                if (IsWalkable(worldCheck))
                {
                    Controller.SetDestination(new Vector2(worldCheck.x, worldCheck.y));
                    return;
                }
                else
                {
                    cardinalExplo = Direction2D.cardinalDirectionsList[Random.Range(0, Direction2D.cardinalDirectionsList.Count)];
                }
            }
        }

       // Debug.LogWarning("⚠️ No valid destination found.");
    }

    private List<RoomInfo> GetNeighboringRooms()
    {
        var nearby = new List<RoomInfo>();
        var knownRooms = Controller.currentFaction.knownRoomsDict;
        var currentRoom = Controller.currentRoom;

        if (currentRoom != null && currentRoom.connectedRooms != null)
        {
            foreach (RoomInfo connected in currentRoom.connectedRooms)
            {
                if (!knownRooms.ContainsKey(connected.index))
                {
                    nearby.Add(connected);
                }
            }
        }
        if (Controller.currentTile.corridor != null)
        {
            foreach (RoomInfo connected in Controller.currentTile.corridor.connectedRooms)
            {
                if (!knownRooms.ContainsKey(connected.index))
                {
                    nearby.Add(connected);
                }
            }
        }
        if (nearby.Count == 0)
        {
            foreach (RoomInfo known in knownRooms.Values)
            {
                if (known.connectedRooms == null) continue;

                foreach (RoomInfo connected in known.connectedRooms)
                {
                    if (!knownRooms.ContainsKey(connected.index))
                    {
                        nearby.Add(connected);
                    }
                }
            }
        }

        return nearby;
    }


    private IEnumerator ExploreCurrentRoom(RoomInfo Room)
    {
        bool exploringRoom = true;
        while (exploringRoom)
        {
            //Debug.Log($"exploring room");
            var faction = Controller.currentFaction;
            bool roomIsKnown = true;
            // Find unexplored tiles
            List<TileInfo> unexplored = new List<TileInfo>();
            foreach (TileInfo tile in Room.tiles)
            {
                if (!faction.knownTilesDict.ContainsKey(tile.position))
                {
                    unexplored.Add(tile);
                    roomIsKnown = false;
                }
            }

            if (roomIsKnown && !faction.knownRoomsDict.ContainsKey(Room.index))
            {
                faction.knownRoomsDict[Room.index] = Room;
                //Debug.Log($"🧭 {faction.name} fully explored room at {Room.index}");
                faction.rooms++;
                yield break;
            }

            if (unexplored.Count > 0 && !Controller.hasDestination)
            {
                TileInfo targetTile = unexplored[Random.Range(0, unexplored.Count)];
                Vector2 destination = new Vector2(targetTile.position.x + 0.5f, targetTile.position.y + 0.5f);
                Controller.SetDestination(destination);
            }

            yield return new WaitForSeconds(0.5f);
        }

        exploreCurrentRoomCoroutine = null;

        /*if (unexplored.Count == 0 && !faction.knownRoomsDict.ContainsKey(Room.index))
            {
                faction.knownRoomsDict[Room.index] = Room;
                Debug.Log($"🧭 {faction.name} fully explored room at {Room.index}");
                faction.rooms++;
            }*/
    }
}
