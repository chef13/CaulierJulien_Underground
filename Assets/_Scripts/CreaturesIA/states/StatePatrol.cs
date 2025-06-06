using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
public class StatePatrol : CreatureState
{
    private Queue<Vector2> patrolQueue = new Queue<Vector2>();
    private int currentIndex = 0;
    private Queue<Vector2> patrolQueueHQ = new Queue<Vector2>();

    bool patrolAroundHQ = false;
    bool patrolToOtherHQ = false;

    public StatePatrol(CreatureAI creature) : base(creature)
    {

    }

    public override void Enter()
    {
        if (Controller.currentRoom == null || Controller.currentRoom != null && Controller.currentRoom.faction != Controller.currentFaction)
        {
            RoomInfo closerHQ = GetCloserHQ();
            Controller.SetDestination(closerHQ.tileCenter);
        }
        else if (Controller.currentRoom != null && Controller.currentRoom.faction == Controller.currentFaction)
        {
           
                patrolAroundHQ = true;
                GetPatrolAroundHQ();
                Vector2 nextPosition = patrolQueue.Dequeue();
                Controller.SetDestination(nextPosition);
            
        }
    }

    public override void Update()
    {
        if (!Controller.hasDestination && patrolAroundHQ)
        {
            if (patrolQueue.Count > 0)
            {
                Vector2 nextPosition = patrolQueue.Dequeue();
                Controller.SetDestination(nextPosition);
            }
            else
            {
                patrolAroundHQ = false;
                if (patrolQueueHQ.Count > 0)
                {
                    Vector2 nextPosition = patrolQueueHQ.Dequeue();
                    Controller.SetDestination(nextPosition);
                    patrolToOtherHQ = true;
                }
                else
                {
                    GetHQpos();
                    if (patrolQueueHQ.Count > 0)
                    {
                        Vector2 nextPosition = patrolQueueHQ.Dequeue();
                        Controller.SetDestination(nextPosition);
                    }
                }
            }
            
        }

        if (!Controller.hasDestination && patrolToOtherHQ)
        {
            GetPatrolAroundHQ();
            patrolToOtherHQ = false;
            patrolAroundHQ = true;
            if (patrolQueue.Count > 0)
            {
                Vector2 nextPosition = patrolQueue.Dequeue();
                Controller.SetDestination(nextPosition);
            }
            // else: optionally handle idle or fallback
        }

    }

    public void GetHQpos()
    {
        for (int i = 0; i < Controller.currentFaction.currentHQ.Count; i++)
        {
            Vector2Int hqPos = Controller.currentFaction.currentHQ[i].tileCenter;
            if (!patrolQueueHQ.Contains(hqPos))
            {
                patrolQueueHQ.Enqueue(hqPos);
            }
        }
    }

    public void GetPatrolAroundHQ()
    {
        if (Controller.currentRoom != null && Controller.currentRoom.faction == Controller.currentFaction)
        {
            for (int i = 0; i < Controller.currentRoom.connectedRooms.Count; i++)
            {
                Vector2 pos = Controller.currentRoom.connectedRooms[i].tileCenter;
                if (!patrolQueue.Contains(pos))
                {
                    patrolQueue.Enqueue(pos);
                }
            }
        }
    }


    
}
