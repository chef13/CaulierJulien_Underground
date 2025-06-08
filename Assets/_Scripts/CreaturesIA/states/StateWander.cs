using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class StateWander : CreatureState
{
    private bool wanderingInCurrentRoom = false;
    private bool changingRoom = false;

    private Vector2Int cardinalExplo;
    private Coroutine exploreCurrentRoomCoroutine;
    private Queue<Vector3Int> waypoints = new Queue<Vector3Int>();

    public StateWander(CreatureAI creature) : base(creature) {}

    public override void Enter()
    {
        // Reset state
        if (exploreCurrentRoomCoroutine != null)
        {
            creature.StopCoroutine(exploreCurrentRoomCoroutine);
            exploreCurrentRoomCoroutine = null;
        }

        waypoints.Clear();
        cardinalExplo = Direction2D.cardinalDirectionsList[Random.Range(0, Direction2D.cardinalDirectionsList.Count)];

        if (Controller.currentRoom == null || Controller.currentRoom.faction != Controller.currentFaction)
        {
            RoomInfo room = GetNextRoom();
            if (room != null)
            {
                changingRoom = true;
                wanderingInCurrentRoom = false;
                Controller.SetDestination(room.tileCenter);
            }
        }
        else
        {
            StartWanderingInRoom(Controller.currentRoom);
        }
    }

    public override void Update()
    {
        if (!Controller.hasDestination)
        {
            if (changingRoom && Controller.currentRoom != null)
            {
                StartWanderingInRoom(Controller.currentRoom);
                return;
            }

            if (wanderingInCurrentRoom)
            {
                if (waypoints.Count > 0)
                {
                    Vector3Int next = waypoints.Dequeue();
                    Controller.SetDestination(new Vector2(next.x, next.y));
                }
                else
                {
                    // Finished patrolling this room
                    RoomInfo room = GetNextRoom();
                    if (room != null)
                    {
                        changingRoom = true;
                        wanderingInCurrentRoom = false;
                        Controller.SetDestination(room.tileCenter);
                    }
                }
            }
            else
            {
                // Try random cardinal movement
                TryRandomMove();
            }
        }
    }

    private void StartWanderingInRoom(RoomInfo room)
    {
        if (exploreCurrentRoomCoroutine != null)
            creature.StopCoroutine(exploreCurrentRoomCoroutine);

        exploreCurrentRoomCoroutine = creature.StartCoroutine(WanderingInCurrentRoom(room));
    }

    private IEnumerator WanderingInCurrentRoom(RoomInfo room)
    {
        if (room == null || room.tiles == null || room.tiles.Count == 0)
            yield break;

        waypoints.Clear();
        wanderingInCurrentRoom = true;
        changingRoom = false;

        int numberOfWaypoints = Random.Range(1, 3);
        while (waypoints.Count < numberOfWaypoints)
        {
            Vector3Int pos = room.tiles[Random.Range(0, room.tiles.Count)].position;
            if (!waypoints.Contains(pos))
                waypoints.Enqueue(pos);
        }

        while (wanderingInCurrentRoom)
        {
            if (!Controller.hasDestination && waypoints.Count > 0)
            {
                Vector3Int next = waypoints.Dequeue();
                Controller.SetDestination(new Vector2(next.x, next.y));
            }

            yield return new WaitForSeconds(Random.Range(1f, 2f));
        }
    }

    private RoomInfo GetNextRoom()
    {
        RoomInfo room = Controller.currentRoom;
        if (room == null || room.connectedRooms == null || room.connectedRooms.Count == 0)
            return null;

        return room.connectedRooms[Random.Range(0, room.connectedRooms.Count)];
    }

    private void TryRandomMove()
    {
        Vector2Int origin = Vector2Int.RoundToInt(creature.transform.position);

        for (int i = 0; i < 4; i++)
        {
            Vector2Int check = origin + cardinalExplo * 10;
            Vector3 worldCheck = new Vector3(check.x + 0.5f, check.y + 0.5f);

            if (IsWalkable(worldCheck))
            {
                Controller.SetDestination(worldCheck);
                return;
            }
            else
            {
                cardinalExplo = Direction2D.cardinalDirectionsList[Random.Range(0, Direction2D.cardinalDirectionsList.Count)];
            }
        }
    }
}
