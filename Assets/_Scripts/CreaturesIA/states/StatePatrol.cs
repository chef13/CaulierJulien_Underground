using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class StatePatrol : CreatureState
{
    private Queue<Vector2> patrolQueue = new Queue<Vector2>();
    private int currentIndex = 0;
    private Queue<Vector2> patrolQueueHQ = new Queue<Vector2>();
    private RoomInfo currentHQpatrol;

    public StatePatrol(CreatureAI creature) : base(creature)
    {

    }

    public override void Enter()
    {
        patrolQueue.Clear();
        patrolQueueHQ.Clear();

       
        RoomInfo closerHQ = GetCloserHQ();
        GetPatrolAroundHQ(closerHQ);

        if (patrolQueue.Count == 0)
            patrolQueue.Enqueue(Controller.currentRoom.tileCenter); // fallback

        Controller.SetDestination(patrolQueue.Dequeue());
    }

    public override void Update()
    {
        if (!Controller.hasDestination)
        {

            if (patrolQueue.Count > 0)
            {
                Controller.SetDestination(patrolQueue.Dequeue());
                return;
            }

            if (patrolQueue.Count == 0 && Controller.currentFaction.currentHQ.Count == 1)
            {
                GetPatrolAroundHQ(currentHQpatrol);
                return;
            }

            if (patrolQueue.Count == 0 && Controller.currentFaction.currentHQ.Count > 1)
            {
                foreach (var hq in Controller.currentFaction.currentHQ)
                {
                    if (hq != currentHQpatrol)
                    {
                        currentHQpatrol = hq;
                        GetPatrolAroundHQ(currentHQpatrol);
                        return;
                    }
                }
            }


            
            
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

    public void GetPatrolAroundHQ(RoomInfo HQ)
    {
        
        if (HQ == null || HQ.connectedRooms == null)
        return;

            for (int i = 0; i < HQ.connectedRooms.Count; i++)
        {
            Vector2 pos = HQ.connectedRooms[i].tileCenter;
            if (!patrolQueue.Contains(pos))
            {
                patrolQueue.Enqueue(pos);
            }
        }
        
    }


    
}
