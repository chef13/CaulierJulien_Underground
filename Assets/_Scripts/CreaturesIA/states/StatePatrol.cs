using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Diagnostics;

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
        patrolQueue = new Queue<Vector2>();
        patrolQueueHQ = new Queue<Vector2>();
        RoomInfo closerHQ = GetCloserHQ();

        currentHQpatrol = closerHQ;

        GetPatrolAroundHQ(closerHQ);



        Controller.SetDestination(patrolQueue.Dequeue());
    }
    
    public override void Exit()
    {
        patrolQueue.Clear();
        patrolQueueHQ.Clear();
        currentHQpatrol = null;
        Controller.SetDestination(Controller.transform.position); // Stop moving
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

            if (Controller.currentFaction.currentHQ.Count == 0)
            {
                creature.SwitchState(new StateIdle(creature));
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
        bool foundNewHQ = false;

        if (HQ == null || HQ.connectedRooms == null)
            return;

        if (Controller.currentFaction.currentHQ.Count == 0)
        {
            currentHQpatrol = Controller.currentFaction.currentHQ[0];
            return;
        }
        RoomInfo randomHQPick = null;
        if (Controller.currentFaction.currentHQ.Count > 1)
        {
            randomHQPick = Controller.currentFaction.currentHQ[Random.Range(0, Controller.currentFaction.currentHQ.Count)];
        }
        

        if (HQ != randomHQPick)
        {
            currentHQpatrol = randomHQPick;
            GetPatrolAroundHQ(randomHQPick);
            foundNewHQ = true;
        }

        if (!foundNewHQ)
        {
            GetPatrolAroundHQ(currentHQpatrol);
        }
        
    }


    
}
