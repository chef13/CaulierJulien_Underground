
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
        float? minDistance = null;
        RoomInfo closerHQ = null;
        for (int i = 0; i < controller.currentFaction.currentHQ.Count; i++)
        {
            float distanceFromHQ =
                controller.GetPathDistance(
                    controller.agent,
                    controller.currentFaction.currentHQ[i].tiles
                    [Random.Range(0, controller.currentFaction.currentHQ[i].tiles.Count)].position);
            if (minDistance == null || distanceFromHQ < minDistance)
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
                        if (c.isDead && c.isCorpse || !c.isDead && c.currentFaction != Controller.currentFaction)
                        {
                            targets.Add(c);
                        }
                    }
                }

            }
        }
        else if (range == 1)
        {
            for (int i = 0; i <room.connectedRooms.Count; i++)
            {
                for (int j = 0; j < room.connectedRooms[i].tiles.Count; j++)
                {
                    if (room.connectedRooms[i].tiles[j].creatures != null &&
                    room.connectedRooms[i].tiles[j].creatures.Count != 0)
                    {
                        foreach (CreatureController c in room.connectedRooms[i].tiles[j].creatures)
                        {
                            if (c.isDead && c.isCorpse || !c.isDead && c.currentFaction != Controller.currentFaction)
                            {
                                targets.Add(c);
                            }
                        }

                    }

                }
            }
        }
        else if (range >= 2)
        {
            for (int i = -range; i < range; i++)
            {
                for (int j = -range; j < range; j++)
                {
                    if (DungeonGenerator.Instance.roomsMap.TryGetValue(new Vector2Int(room.index.x + i,room.index.y + j), out RoomInfo roomInRange))
                    {
                        foreach (TileInfo t in roomInRange.tiles)
                        {
                            if (t.creatures != null && t.creatures.Count != 0)
                            {
                                foreach (CreatureController c in t.creatures)
                                {
                                    if (c.isDead && c.isCorpse || !c.isDead && c.currentFaction != Controller.currentFaction)
                                    {
                                        targets.Add(c);
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
        if (Controller.currentRoom == null)
        {
            room = Controller.currentTile.corridor.connectedRooms[0];
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
                        if (flaureBehaviour != null && flaureBehaviour.isEdible)
                        {
                            targets.Add(flaureBehaviour);
                        }
                    }
                }

            }
        }
        else if (range == 1)
        {
            for (int i = 0; i < room.connectedRooms.Count; i++)
            {
                for (int j = 0; j < room.connectedRooms[i].tiles.Count; j++)
                {
                    if (room.connectedRooms[i].tiles[j].objects != null &&
                    room.connectedRooms[i].tiles[j].objects.Count != 0)
                    {
                        foreach (GameObject c in room.connectedRooms[i].tiles[j].objects)
                        {
                            FlaureBehaviour flaureBehaviour = c.GetComponent<FlaureBehaviour>();
                            if (flaureBehaviour != null && flaureBehaviour.isEdible)
                            {
                                targets.Add(flaureBehaviour);
                            }
                        }

                    }

                }
            }
        }
        /*else if (range >= 2)
        {
            for (int i = -range; i < range; i++)
            {
                for (int j = -range; j < range; j++)
                {
                    if (DungeonGenerator.Instance.roomsMap.TryGetValue(new Vector2Int(room.index.x + i, Controller.currentRoom.index.y + j), out RoomInfo roomInRange))
                    {
                        foreach (TileInfo t in roomInRange.tiles)
                        {
                            if (t.objects != null && t.objects.Count != 0)
                            {
                                foreach (GameObject c in t.objects)
                                {
                                    FlaureBehaviour flaureBehaviour = c.GetComponent<FlaureBehaviour>();
                                    if (flaureBehaviour != null && flaureBehaviour.isEdible)
                                    {
                                        targets.Add(flaureBehaviour);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }*/

        float minDistance = 1000f;
        for (int i = 0; i < targets.Count; i++)
        {
            float distanceFromTarget =
                Controller.GetPathDistance(
                    Controller.agent,
                    targets[i].transform.position);
            if (distanceFromTarget < minDistance)
            {
                minDistance = distanceFromTarget;
                flaure = targets[i];
            }
        }

        return flaure;
    }

}