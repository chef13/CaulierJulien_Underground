
using System.Collections;
using System.Collections.Generic;
using CrashKonijn.Goap.Runtime;
using UnityEngine;
using UnityEngine.AI;

public abstract class CreatureState
{
    protected CreatureAI creature;
    private CreatureController controller;
    public CreatureController Controller
    {
        get { return controller; }
        set { controller = value; }
    }

    public CreatureState(CreatureAI creature)
    {
        this.creature = creature;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update() { }
    public void FixedUpdate()
    {
        CreatureController target;
        if (
            DetectTreat(out target)&&
            Controller.data.carnivor &&
            Controller.currentHungerState != CreatureController.hungerState.Full ||

            DetectTreat(out target) &&
            target.currentFaction == FactionSpawner.instance.dungeonFaction &&
            Controller.currentFaction.dungeonFav < -2 ||

            DetectTreat(out target) &&
            Controller.currentFaction != FactionSpawner.instance.dungeonFaction &&
            Controller.currentFaction.dungeonFav < -2 &&
            target.currentFaction == FactionSpawner.instance.dungeonFaction
        )
        {
            creature.SwitchState(new StateAttack(creature, target));
        }
    }

    public virtual void CheckCarnivor() { }
    public virtual void CheckHerbivor() { }

    public CreatureController DetectTreat(out CreatureController target)
    {
        List<CreatureController> treats = new List<CreatureController>();
        target = null;
        for (int i = 0; i < controller.CreaturesInRange.Count; i++)
        {
            if (controller.CreaturesInRange[i] != null &&
                controller.CreaturesInRange[i].currentFaction != controller.currentFaction)
            {
                RaycastHit2D hit = Physics2D.Raycast(controller.transform.position,
                    controller.CreaturesInRange[i].transform.position - controller.transform.position,
                    Vector2.Distance(controller.transform.position, controller.CreaturesInRange[i].transform.position),
                    LayerMask.GetMask("Wall"));
                if (hit.collider == null && !controller.CreaturesInRange[i].isDead)
                    treats.Add(controller.CreaturesInRange[i]);
            }
        }

        if (treats.Count == 0)
        {
            return null;
        }
        // Find the closest treat
        float minDistance = float.MaxValue;
        for (int t = 0; t < treats.Count; t++)
        {
            float dist = controller.GetPathDistance(controller.agent, treats[t].agent.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                target = treats[t];
            }
        }

        return target;
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

    public RoomInfo GetCloserHQ()
    {
        float minDistance = 1000f;
        RoomInfo closerHQ = null;
        for (int i = 0; i < controller.currentFaction.currentHQ.Count; i++)
        {
            float distanceFromHQ =
                controller.GetPathDistance(
                    controller.agent,
                    controller.currentFaction.currentHQ[i].tiles
                    [Random.Range(0, controller.currentFaction.currentHQ[i].tiles.Count)].position);
            if (distanceFromHQ < minDistance)
            {
                minDistance = distanceFromHQ;
                closerHQ = controller.currentFaction.currentHQ[i];
            }
        }
        return closerHQ;
    }

    public CreatureController CheckForRoomWith(CreatureController creature, int range)
    {
        List<CreatureController> targets = new List<CreatureController>();
        RoomInfo room = null;
        List<RoomInfo> roomRange1 = new List<RoomInfo>();
        List<RoomInfo> roomRange2 = new List<RoomInfo>();
        if (Controller.currentRoom == null)
        {
            room = Controller.currentTile.corridor.connectedRooms[0];
        }
        else
        {
            room = Controller.currentRoom;
        }
        //RoomInfo room = null;
        if (range == 0)
        {
            foreach (TileInfo t in room.tiles)
            {
                if (t.creatures != null && t.creatures.Count != 0)
                {
                    foreach (CreatureController c in t.creatures)
                    {
                        if (c.gameObject.activeInHierarchy && c.isDead && c.isCorpse || !c.isDead && c.currentFaction != Controller.currentFaction)
                        {
                            targets.Add(c);
                        }
                    }
                }

            }
        }
        else if (range == 1)
        {
            for (int i = 0; i < room.connectedRooms.Count; i++)
            {
                roomRange1.Add(room.connectedRooms[i]);
                if (range == 1)
                {
                    for (int j = 0; j < room.connectedRooms[i].tiles.Count; j++)
                    {
                        if (room.connectedRooms[i].tiles[j].creatures != null &&
                        room.connectedRooms[i].tiles[j].creatures.Count != 0)
                        {
                            foreach (CreatureController c in room.connectedRooms[i].tiles[j].creatures)
                            {
                                if (c.gameObject.activeInHierarchy && c.isDead && c.isCorpse || !c.isDead && c.currentFaction != Controller.currentFaction)
                                {
                                    targets.Add(c);
                                }
                            }

                        }

                    }
                }
            }
        }
        else if (range >= 2)
        {
            for (int r = 0; r < roomRange1.Count; r++)
            {
                for (int i = 0; i < roomRange1[r].connectedRooms.Count; i++)
                {
                    if (!roomRange2.Contains(roomRange1[r].connectedRooms[i]) && roomRange1[r].connectedRooms[i] != room)
                    {
                        roomRange2.Add(roomRange1[r].connectedRooms[i]);
                    }
                    if (range == 2)
                    {
                        for (int j = 0; j < room.connectedRooms[i].tiles.Count; j++)
                        {
                            if (room.connectedRooms[i].tiles[j].creatures != null &&
                            room.connectedRooms[i].tiles[j].creatures.Count != 0)
                            {
                                foreach (CreatureController c in room.connectedRooms[i].tiles[j].creatures)
                                {
                                    if (c.gameObject.activeInHierarchy && c.isDead && c.isCorpse || !c.isDead && c.currentFaction != Controller.currentFaction)
                                    {
                                        return c;
                                    }
                                }

                            }

                        }
                    }
                }
            }
        }

        float minDistance = 1000f;
        creature = null;
        for (int i = 0; i < targets.Count; i++)
        {
            float distanceFromTarget =
                Controller.GetPathDistance(
                    Controller.agent,
                    targets[i].transform.position);
            if (distanceFromTarget < minDistance)
            {
                minDistance = distanceFromTarget;
                creature = targets[i];
            }
        }

        return creature;
    }

    public FlaureBehaviour CheckForRoomWith(FlaureBehaviour flaure, int range)
    {
        List<FlaureBehaviour> targets = new List<FlaureBehaviour>();
        //RoomInfo room = null;
        RoomInfo room = null;
        List<RoomInfo> roomRange1 = new List<RoomInfo>();
        List<RoomInfo> roomRange2 = new List<RoomInfo>();
        if (Controller.currentRoom == null)
        {
            room = Controller.currentTile.corridor.connectedRooms[Random.Range(0, Controller.currentTile.corridor.connectedRooms.Count)];
        }
        else
        {
            room = Controller.currentRoom;
        }
        if (range == 0)
        {
            foreach (TileInfo t in room.tiles)
            {
                if (t.objects != null && t.objects.Count != 0)
                {
                    foreach (GameObject c in t.objects)
                    {
                        FlaureBehaviour flaureBehaviour = c.GetComponent<FlaureBehaviour>();
                        if (flaureBehaviour != null && flaureBehaviour.gameObject.activeInHierarchy && flaureBehaviour.isEdible)
                        {
                            targets.Add(flaureBehaviour);
                        }
                    }
                }

            }
        }
        else if (range >= 1)
        {
            for (int i = 0; i < room.connectedRooms.Count; i++)
            {
                roomRange1.Add(room.connectedRooms[i]);
                if (range == 1)
                {
                    for (int j = 0; j < room.connectedRooms[i].tiles.Count; j++)
                    {
                        if (room.connectedRooms[i].tiles[j].objects != null &&
                            room.connectedRooms[i].tiles[j].objects.Count != 0)
                        {
                            foreach (GameObject c in room.connectedRooms[i].tiles[j].objects)
                            {
                                FlaureBehaviour flaureBehaviour = c.GetComponent<FlaureBehaviour>();
                                if (flaureBehaviour != null && flaureBehaviour.gameObject.activeInHierarchy && flaureBehaviour.isEdible)
                                {
                                    targets.Add(flaureBehaviour);
                                }
                            }
                        }
                    }
                }
            }
        }
        else if (range >= 2)
        {
            for (int r = 0; r < roomRange1.Count; r++)
            {
                for (int i = 0; i < roomRange1[r].connectedRooms.Count; i++)
                {
                    if (!roomRange2.Contains(roomRange1[r].connectedRooms[i]) && roomRange1[r].connectedRooms[i] != room)
                    {
                        roomRange2.Add(roomRange1[r].connectedRooms[i]);
                    }
                    if (range == 2)
                    {
                        for (int j = 0; j < roomRange1[r].connectedRooms[j].tiles.Count; j++)
                        {
                            TileInfo t = roomRange1[r].connectedRooms[i].tiles[j];
                            if (t.objects != null && t.objects.Count != 0)
                            {
                                foreach (GameObject c in t.objects)
                                {
                                    FlaureBehaviour flaureBehaviour = c.GetComponent<FlaureBehaviour>();
                                    if (flaureBehaviour != null && flaureBehaviour.gameObject.activeInHierarchy && flaureBehaviour.isEdible)
                                    {
                                        return flaureBehaviour;
                                    }
                                }
                            }

                        }
                    }
                }
            }
        }

        float minDistance = 1000f;
        for (int i = 0; i < targets.Count; i++)
        {
            float distanceFromTarget =
                Controller.GetPathDistance(
                    Controller.agent,
                    targets[i].transform.position);
            if (distanceFromTarget >= 0 && distanceFromTarget < minDistance)
            {
                minDistance = distanceFromTarget;
                flaure = targets[i];
            }
        }

        return flaure;
    }

    public RoomInfo GetRandomRoom(RoomInfo room, int range)
    {
        if (room == null)
        {
            return null;
        }
        List<RoomInfo> roomRange1 = new List<RoomInfo>();
        List<RoomInfo> roomRange2 = new List<RoomInfo>();
        if (range == 0)
        {
            return room;
        }
        else if (range == 1)
        {
            for (int i = 0; i < room.connectedRooms.Count; i++)
            {
                if (Controller.previousRoom != null && room.connectedRooms[i] != Controller.previousRoom)
                    roomRange1.Add(room.connectedRooms[i]);
            }
            if (roomRange1.Count == 0)
                return room; // fallback or return null
            return roomRange1[Random.Range(0, roomRange1.Count)];
        }
        else if (range == 2 && roomRange1.Count > 0)
        {
            for (int r = 0; r < roomRange1.Count; r++)
            {
                for (int i = 0; i < roomRange1[r].connectedRooms.Count; i++)
                {
                    if (!roomRange2.Contains(roomRange1[r].connectedRooms[i]) && roomRange1[r].connectedRooms[i] != room)
                    {
                        if (roomRange2.Count == 0)
                            return room; // fallback or return null
                        roomRange2.Add(roomRange1[r].connectedRooms[i]);
                    }
                }
            }
            return roomRange2[Random.Range(0, roomRange2.Count)];
        }

        return null;
    }


    public RoomInfo FindRoom(RoomInfo origin, int range, System.Func<RoomInfo, bool> predicate = null)
    {
        if (origin == null || range < 0)
            return null;

        HashSet<RoomInfo> visited = new() { origin };
        Queue<(RoomInfo room, int depth)> queue = new();
        List<RoomInfo> candidates = new();

        queue.Enqueue((origin, 0));

        while (queue.Count > 0)
        {
            var (room, depth) = queue.Dequeue();

            if (predicate == null || predicate(room))
            {
                candidates.Add(room);
            }

            if (depth < range)
            {
                foreach (var neighbor in room.connectedRooms)
                {
                    if (visited.Add(neighbor))
                    {
                        queue.Enqueue((neighbor, depth + 1));
                    }
                }
            }
        }

        return candidates.Count > 0 ? candidates[Random.Range(0, candidates.Count)] : null;
    }
}