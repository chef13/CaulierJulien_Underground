
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using Unity.VisualScripting;

public abstract class FactionType
{
    [SerializeField] protected float coroutineDelay = 2f;
    public enum FactionGoal
    {
        None,
        RecoltFood,
        FindHQ,
        Wander,
        Patrol
    }
    public FactionBehaviour faction;
    public Coroutine mainCoroutineForGoals;
    public Coroutine findHQCoroutine;
    public Coroutine recoltFoodCoroutine;
    public Coroutine wanderingGoalCoroutine;
    public Coroutine patrolCoroutine;
    /*public List<CreatureController> membersPatrol = new List<CreatureController>();
    public List<CreatureController> membersWander = new List<CreatureController>();
    public List<CreatureController> membersRecoltFood = new List<CreatureController>();
    public List<CreatureController> membersFindHQ = new List<CreatureController>();*/
    //protected float goalCheckTime = 1f;


    public FactionType(FactionBehaviour faction)
    {
        this.faction = faction;
    }

    public virtual void Enter()
    {
        //mainCoroutineForGoals = faction.StartCoroutine(MainCoroutineForGoals());
    }
    public virtual void Exit()
    {

    }
    public virtual void Update()
    {

    }

    public virtual void AskForState()
    {

    }

    public IEnumerator MainCoroutineForGoals()
    {
        while (faction != null && faction.isActiveAndEnabled)
        {
            CheckGoals();
            yield return new WaitForSeconds(1f); // Adjust the wait time as needed
        }
    }

    public virtual void MainCheckForGoals()
    {

    }

    protected virtual void CheckGoals() { }

    protected void AssigningMember(List<CreatureController> GoalMembers, FactionGoal goal, int goalPriority, int maxMember)
    {
        if (faction.members == null || faction.members.Count == 0)
            return;

        // Find all eligible members not already assigned to this goal
        var eligible = new List<CreatureController>();
        foreach (var member in faction.members)
        {
            if (member == null || member.isDead || !member.isActiveAndEnabled)
                break;
            if (member.currentGoalPriority < goalPriority && !GoalMembers.Contains(member))
                    eligible.Add(member);
        }

        // Shuffle eligible list for randomness
        for (int i = eligible.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var temp = eligible[i];
            eligible[i] = eligible[j];
            eligible[j] = temp;
        }

        // Assign up to maxMember
        int toAssign = Mathf.Min(maxMember - GoalMembers.Count, eligible.Count);
        for (int i = 0; i < toAssign; i++)
        {
            var member = eligible[i];
            var previousGoal = member.currentGoal;
            switch (previousGoal)
            {
                case CreatureController.CreatureGoal.RecoltFood:
                    faction.membersRecoltFood.Remove(member);
                    break;
                case CreatureController.CreatureGoal.FindHQ:
                    faction.membersFindHQ.Remove(member);
                    break;
                case CreatureController.CreatureGoal.Wander:
                    faction.membersWander.Remove(member);
                    break;
                case CreatureController.CreatureGoal.Patrol:
                    faction.membersPatrol.Remove(member);
                    break;
            }
            if (!member.basicNeed
                && member.currentState is not StateNeedFood 
                 && member.currentState is not StateNeedRest 
                && member.currentState is not StateAttack 
                && member.currentState is not StateFlee)
            {
                switch (goal)
                {
                    case FactionGoal.RecoltFood:
                        member.creatureAI.SwitchState(new StateRecolt(member.creatureAI));
                        break;
                    case FactionGoal.FindHQ:
                        member.creatureAI.SwitchState(new StateExplore(member.creatureAI));
                        break;
                    case FactionGoal.Wander:
                        member.creatureAI.SwitchState(new StateWander(member.creatureAI));
                        break;
                    case FactionGoal.Patrol:
                        member.creatureAI.SwitchState(new StatePatrol(member.creatureAI));
                        break;
                }
            }
            member.currentGoal = (CreatureController.CreatureGoal)goal;
            member.currentGoalPriority = goalPriority;
            GoalMembers.Add(member);
        }
    }

    protected void UnAssigningMember(List<CreatureController> GoalMembers, CreatureController GoalMember, FactionGoal goal)
    {
        if (GoalMembers.Contains(GoalMember))
        {
            switch (goal)
            {
                case FactionGoal.RecoltFood:
                    faction.membersRecoltFood.Remove(GoalMember);
                    break;
                case FactionGoal.FindHQ:
                    faction.membersFindHQ.Remove(GoalMember);
                    break;
                case FactionGoal.Wander:
                    faction.membersWander.Remove(GoalMember);
                    break;
                case FactionGoal.Patrol:
                    faction.membersPatrol.Remove(GoalMember);
                    break;
            }
            GoalMember.currentGoal = (CreatureController.CreatureGoal)FactionGoal.None;
            GoalMember.currentGoalPriority = 0;
            if (GoalMembers.Contains(GoalMember))
            GoalMembers.Remove(GoalMember);
            
        }
        else
        {
            Debug.LogWarning($"Creature {GoalMember.name} is not assigned to goal {goal}.");
        }
    }

    public virtual bool PotencialHQ(RoomInfo room) { return false; }





    public IEnumerator RecoltFoodgoal()
    {
        while (true)
        {
            int baseNeededMembers = 2;
            int basePriority = 1;
            int foodNeeded = faction.members.Count * 25; // Assuming each member needs 2 food units
            int maxMembers = faction.currentHQ.Count * 4;


            if (faction.membersRecoltFood.Count <= faction.currentHQ.Count)
            {
                AssigningMember(faction.membersRecoltFood, FactionGoal.RecoltFood, 5, faction.currentHQ.Count);
                yield return new WaitForSeconds(coroutineDelay);
            }

            if (faction.foodResources == 0)
            {
                baseNeededMembers += faction.members.Count; // Ensure at least one member is assigned
                basePriority += 3;
            }

            if (faction.foodResources < Mathf.RoundToInt(foodNeeded * 0.8f))
            {
                baseNeededMembers++; // Increase the number of members needed
                basePriority++;
            }

            if (faction.foodResources < Mathf.RoundToInt(foodNeeded))
            {
                baseNeededMembers++; // Increase the number of members needed
                basePriority++;
            }

            if (faction.membersRecoltFood.Count < baseNeededMembers)
            {
                AssigningMember(faction.membersRecoltFood, FactionGoal.RecoltFood, basePriority, baseNeededMembers);
                yield return new WaitForSeconds(coroutineDelay);
            }



            if (faction.membersRecoltFood.Count == 0)
            {
                Debug.Log("GobFaction: No members assigned to Recolt Food");
            }


            if (faction.membersRecoltFood.Count > baseNeededMembers)
            {
                int excessMembers = faction.membersRecoltFood.Count - baseNeededMembers;

                if (faction.membersRecoltFood.Count > baseNeededMembers)
                {
                    List<CreatureController> excessMembersList = new List<CreatureController>();
                    for (int i = faction.membersRecoltFood.Count -1; i >= 0; i--)
                    {
                        var excessmember = faction.membersRecoltFood[i];
                        if (
                            excessmember.currentRoom != null &&
                            excessmember.currentRoom.faction != null &&
                            excessmember.currentRoom.faction == excessmember.currentFaction
                            )
                        {
                            excessMembersList.Add(excessmember);
                        }
                    }
                    for (int i = 0;  i < excessMembersList.Count; i++)
                    {
                        CreatureController excessMember = excessMembersList[i];

                        UnAssigningMember(faction.membersRecoltFood, excessMember, FactionGoal.RecoltFood);

                    }


                }
            }


            for (int m = 0; m < faction.membersRecoltFood.Count; m++)
            {
                CreatureController member = faction.membersRecoltFood[m];

                if (!member.basicNeed
                        && member.currentState is not StateNeedFood
                        && member.currentState is not StateNeedRest
                        && member.currentState is not StateAttack
                        && member.currentState is not StateFlee
                        && member != null && !member.isDead && member.isActiveAndEnabled)
                {
                    member.creatureAI.SwitchState(new StateRecolt(member.creatureAI));
                }
                    if (member.currentGoal != CreatureController.CreatureGoal.RecoltFood)
                {
                    UnAssigningMember(faction.membersRecoltFood, member, FactionGoal.RecoltFood);
                }
            }

            yield return new WaitForSeconds(coroutineDelay); // Adjust the wait time as needed
        }

    }



    public IEnumerator FindHQ()
    {
        while (true)
        {
            int baseNeededMembers = 1;
            baseNeededMembers += faction.currentHQ.Count;
            int basePriority = 1;
            basePriority += baseNeededMembers - faction. membersFindHQ.Count;
            //Debug.Log("GobFaction: Checking for available HQ");

            if (faction.currentHQ.Count == 0 && faction.membersFindHQ.Count == 0)
            {
                AssigningMember(faction.membersFindHQ, FactionGoal.FindHQ, 3, faction.members.Count);
                yield return new WaitForSeconds(coroutineDelay); // Wait a bit before starting the search
            }
            // Loop through all known rooms

            if (faction.currentHQ.Count * faction.members.Count > faction.currentHQ.Count * faction.factionData.maxMembersPerHQ
            && faction.membersFindHQ.Count < 3)
            {
                AssigningMember(faction.membersFindHQ, FactionGoal.FindHQ, 3, faction.members.Count / 2);
                //Debug.Log("GobFaction: Members are looking for HQ");
            }

            if (faction.currentHQ.Count * faction.members.Count < faction.currentHQ.Count * faction.factionData.maxMembersPerHQ)
            {
                /// Debug.Log("GobFaction: HQ already found, stopping search.");
                yield return new WaitForSeconds(coroutineDelay); // Stop searching if HQ is already found
            }

            if (faction.currentHQ.Count == 0 || faction.currentHQ.Count * faction.members.Count > faction.currentHQ.Count * faction.factionData.maxMembersPerHQ)
            {
                List<RoomInfo> roomsToAdd = new List<RoomInfo>();
                foreach (var room in faction.knownRoomsDict.Values)
                {

                    // Check if this room is a potential HQ
                    if (PotencialHQ(room) && !faction.currentHQ.Contains(room) && room.faction == null)
                    {

                        roomsToAdd.Add(room);

                    }
                }
                if (roomsToAdd.Count > 0)
                {
                    RoomInfo roomToAdd = roomsToAdd[Random.Range(0, roomsToAdd.Count)];
                
                    faction.currentHQ.Add(roomToAdd);
                    roomToAdd.faction = faction; // Assign the faction to the room
                    foreach (TileInfo tile in roomToAdd.tiles)
                    {
                        tile.faction = faction; // Assign the faction to the tiles
                    }
                    faction.numberOfHQ++;
                    //Debug.Log($"HQ found at {room.index}");
                    for (int i = faction.membersFindHQ.Count - 1; i >= 0; i--)
                    {
                        UnAssigningMember(faction.membersFindHQ, faction.membersFindHQ[i], FactionGoal.FindHQ);
                    }
                }

                yield return new WaitForSeconds(coroutineDelay);

            }

            foreach (var member in faction.membersFindHQ)
            {


                if (!member.basicNeed
                        && member.currentState is not StateNeedFood
                        && member.currentState is not StateNeedRest
                        && member.currentState is not StateAttack
                        && member.currentState is not StateFlee
                        && member != null && !member.isDead && member.isActiveAndEnabled)
                {
                    // If the member is assigned to FindHQ, switch to Explore state
                    member.creatureAI.SwitchState(new StateExplore(member.creatureAI));
                }
                
                if (member.currentGoal != CreatureController.CreatureGoal.FindHQ)
                {
                    UnAssigningMember(faction.membersFindHQ, member, FactionGoal.FindHQ);
                }
            }
            yield return new WaitForSeconds(coroutineDelay);
        }

    }

    public virtual IEnumerator WandererCoroutine()
    {
        while (true)
        {
            int baseNeededMembers = 0;
            baseNeededMembers += faction.currentHQ.Count;
            int basePriority = 0;
            basePriority += baseNeededMembers -faction. membersWander.Count;

            if (faction.membersWander.Count < baseNeededMembers)
            {
                AssigningMember(faction.membersWander, FactionGoal.Wander, 1, baseNeededMembers);
                yield return new WaitForSeconds(coroutineDelay);
            }

            if (faction.membersWander.Count > baseNeededMembers)
            {
                CreatureController excessMember = faction.membersWander[Random.Range(0, faction.membersWander.Count)];
                UnAssigningMember(faction.membersWander, excessMember, FactionGoal.Wander);
            }

            for (int m = 0; m < faction.membersWander.Count; m++)
            {
                CreatureController member = faction.membersWander[m];


                if (!member.basicNeed
                        && member.currentState is not StateNeedFood
                        && member.currentState is not StateNeedRest
                        && member.currentState is not StateAttack
                        && member.currentState is not StateFlee
                        && member != null && !member.isDead && member.isActiveAndEnabled)
                {
                    member.creatureAI.SwitchState(new StateWander(member.creatureAI));
                }
                
                if (member.currentGoal != CreatureController.CreatureGoal.Wander)
                {
                    UnAssigningMember(faction.membersWander, member, FactionGoal.Wander);
                }
            }
            yield return new WaitForSeconds(coroutineDelay); // Adjust the wait time as needed
        }
    }

    public virtual IEnumerator PatrolCoroutine()
    {
        while (true)
        {
            int baseNeededMembers = 1;
            baseNeededMembers += faction.currentHQ.Count * 2;
            int basePriority = 1;
            basePriority += (baseNeededMembers * 2) - faction.membersPatrol.Count;

            if (faction.membersPatrol.Count < baseNeededMembers)
            {
                AssigningMember(faction.membersPatrol, FactionGoal.Patrol, 1, baseNeededMembers);
                yield return new WaitForSeconds(coroutineDelay);
            }

            if (faction.membersPatrol.Count > baseNeededMembers)
            {
                CreatureController excessMember = faction.membersPatrol[Random.Range(0, faction.membersPatrol.Count)];
                UnAssigningMember(faction.membersPatrol, excessMember, FactionGoal.Patrol);
            }

            for (int m = 0; m < faction.membersPatrol.Count; m++)
            {
                CreatureController member = faction.membersPatrol[m];
                
                if (!member.basicNeed
                        && member.currentState is not StateNeedFood
                        && member.currentState is not StateNeedRest
                        && member.currentState is not StateAttack
                        && member.currentState is not StateFlee
                        && member != null && !member.isDead && member.isActiveAndEnabled)
                {
                    member.creatureAI.SwitchState(new StatePatrol(member.creatureAI));
                }
                
                if (member.currentGoal != CreatureController.CreatureGoal.Patrol)
                {
                    UnAssigningMember(faction.membersPatrol, member, FactionGoal.Patrol);
                }
            }
            yield return new WaitForSeconds(coroutineDelay); // Adjust the wait time as needed
        }
    }

}