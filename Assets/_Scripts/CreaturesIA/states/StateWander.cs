using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class StateWander : CreatureState
{

    bool wanderingInCurrentRoom = false;
    bool changingRoom = false;
    private Vector2Int cardinalExplo;
    Coroutine exploreCurrentRoomCoroutine;
    public Queue<Vector3Int> waypoints = new Queue<Vector3Int>();
    public StateWander(CreatureAI creature) : base(creature)
    {
    }

    public override void Enter()
    {
        if (!Controller.hasDestination)
        {
            RoomInfo room = null;
            room = GetRandomRoom(room, 1);
            if (room != null)
            {
                room = GetRandomRoom(room, 2);
            }

            if (room != null)
            {
                changingRoom = true;
                wanderingInCurrentRoom = false;
                Vector2Int randomPosition = room.tileCenter;
                Controller.SetDestination(randomPosition);
            }
        }
    }

    public override void Update()
    {
        if (!Controller.hasDestination && !wanderingInCurrentRoom && Controller.currentRoom != null)
        {
            RoomInfo room = Controller.currentRoom;
            room = GetRandomRoom(room, 1);
            if (room != null)
            {
                room = GetRandomRoom(room, 2);
            }

            if (room != null)
            {
                changingRoom = true;
                wanderingInCurrentRoom = false;
                Vector2Int randomPosition = room.tileCenter;
                Controller.SetDestination(randomPosition);
            }
            return;
        }

        if (!Controller.hasDestination && Controller.currentRoom != null && changingRoom)
        {
            if (exploreCurrentRoomCoroutine != null)
            {
                creature.StopCoroutine(exploreCurrentRoomCoroutine);
            }
            exploreCurrentRoomCoroutine = creature.StartCoroutine(WanderingInCurrentRoom(Controller.currentRoom));
            return;
        }

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

    private IEnumerator WanderingInCurrentRoom(RoomInfo room)
    {
        wanderingInCurrentRoom = true;
        changingRoom = false;
        int numberOfWaypoints = Random.Range(3, 6);
        while (waypoints.Count < numberOfWaypoints)
        {
            Vector3Int randomPosition = room.tiles[Random.Range(0, room.tiles.Count)].position;
            if (!waypoints.Contains(randomPosition))
            {
                waypoints.Enqueue(randomPosition);
            }
        }
        

        while (wanderingInCurrentRoom)
        {
            if (Controller.hasDestination)
            {
                yield return null;
                continue;
            }

            Vector3Int randomPosition = waypoints.Dequeue();
            if (waypoints.Count == 0)
            {
                wanderingInCurrentRoom = false;
                changingRoom = true;
            }
            else
            {
                Controller.SetDestination(new Vector2(randomPosition.x, randomPosition.y));
            }
            
            yield return new WaitForSeconds(Random.Range(1f, 3f));
        }
    }
}
