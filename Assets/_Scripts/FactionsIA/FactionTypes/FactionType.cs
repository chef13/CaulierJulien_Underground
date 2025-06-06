
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using Unity.VisualScripting;

public abstract class FactionType
{
    [SerializeField] protected float coroutineDelay = 2f;
    public FactionBehaviour faction;
    public Coroutine mainCoroutineForGoals;
    public Coroutine findHQCoroutine;
    public Coroutine recoltFoodCoroutine;
    public Coroutine wanderingGoalCoroutine;
    public Coroutine patrolCoroutine;
    public List<CreatureController> membersPatrol = new List<CreatureController>();
    public List<CreatureController> membersWander = new List<CreatureController>();
    public List<CreatureController> membersRecoltFood = new List<CreatureController>();
    public List<CreatureController> membersFindHQ = new List<CreatureController>();
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

    protected virtual void CheckGoals() { }

    protected void AssigningMember(List<CreatureController> GoalMembers, string goal, int goalPriority, int maxMember)
    {
        if (faction.members != null && faction.members.Count > 0)
        {
            CreatureController member = faction.members[Random.Range(0, faction.members.Count)].GetComponent<CreatureController>();
            if ((member.currentGoal == null || member.currentGoalPriority < goalPriority) && !GoalMembers.Contains(member))
            {
                member.currentGoal = goal;
                member.currentGoalPriority = goalPriority;
                GoalMembers.Add(member);
            }
        }

    }

    protected void UnAssigningMember(List<CreatureController> GoalMembers, CreatureController GoalMember, string goal)
    {
        if (GoalMembers.Contains(GoalMember))
        {
            GoalMember.currentGoal = null;
            GoalMember.currentGoalPriority = 0;
            GoalMembers.Remove(GoalMember);
            if (GoalMember.creatureAI.currentState is not StateAttack && GoalMember.creatureAI.currentState is not StateFlee)
            {
                GoalMember.creatureAI.SwitchState(new StateIdle(GoalMember.creatureAI));
            }
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


            if (membersRecoltFood.Count <= faction.currentHQ.Count)
            {
                AssigningMember(membersRecoltFood, "Recolt Food", 5, faction.currentHQ.Count);
                yield return new WaitForSeconds(coroutineDelay);
            }

            if (faction.foodResources == 0)
            {
                baseNeededMembers += Mathf.RoundToInt(faction.members.Count * 0.5f); // Ensure at least one member is assigned
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

            if (membersRecoltFood.Count < baseNeededMembers)
            {
                AssigningMember(membersRecoltFood, "Recolt Food", basePriority, baseNeededMembers);
                yield return new WaitForSeconds(coroutineDelay);
            }



            if (membersRecoltFood.Count == 0)
            {
                Debug.Log("GobFaction: No members assigned to Recolt Food");
            }


            if (membersRecoltFood.Count > baseNeededMembers)
            {
                int excessMembers = membersRecoltFood.Count - baseNeededMembers;

                if (membersRecoltFood.Count > baseNeededMembers)
                {
                    List<CreatureController> excessMembersList = new List<CreatureController>();
                    foreach (CreatureController excessmember in membersRecoltFood)
                    {
                        if (
                            excessmember.currentRoom != null &&
                            excessmember.currentRoom.faction != null &&
                            excessmember.currentRoom.faction == excessmember.currentFaction
                            )
                        {
                            excessMembersList.Add(excessmember);
                        }
                    }
                    for (int i = excessMembersList.Count - 1; i >= 0; i--)
                    {
                        CreatureController excessMember = excessMembersList[i];

                        UnAssigningMember(membersRecoltFood, excessMember, "Recolt Food");

                    }


                }
            }


            for (int m = 0; m < membersRecoltFood.Count; m++)
            {
                CreatureController member = membersRecoltFood[m];
                if (!member.basicNeed && member.currentState is not StateAttack && member.currentState is not StateFlee)
                {
                    member.creatureAI.SwitchState(new StateRecolt(member.creatureAI));
                }
            }

            yield return new WaitForSeconds(coroutineDelay); // Adjust the wait time as needed
        }

    }



    public IEnumerator FindHQ()
    {
        while (true)
        {
            //Debug.Log("GobFaction: Checking for available HQ");

            if (faction.currentHQ.Count == 0 && membersFindHQ.Count == 0)
            {
                AssigningMember(membersFindHQ, "Finding HQ", 3, faction.members.Count);
                yield return new WaitForSeconds(coroutineDelay); // Wait a bit before starting the search
            }
            // Loop through all known rooms

            if (faction.currentHQ.Count * faction.members.Count > faction.currentHQ.Count * faction.factionData.maxMembersPerHQ
            && membersFindHQ.Count < 3)
            {
                AssigningMember(membersFindHQ, "Finding HQ", 3, faction.members.Count / 2);
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
                foreach (var room in roomsToAdd)
                {                    // If a potential HQ is found, assign it to the faction
                    faction.currentHQ.Add(room);
                    room.faction = faction; // Assign the faction to the room
                    foreach (TileInfo tile in room.tiles)
                    {
                        tile.faction = faction; // Assign the faction to the tiles
                    }
                    faction.numberOfHQ++;
                    //Debug.Log($"HQ found at {room.index}");
                    for (int i = membersFindHQ.Count - 1; i >= 0; i--)
                    {
                        UnAssigningMember(membersFindHQ, membersFindHQ[i], "Finding HQ");
                    }
                }

                yield return new WaitForSeconds(coroutineDelay);

            }

            foreach (var member in membersFindHQ)
            {
                // If no potential HQ found, switch member state to explore
                if (member.currentState is StateIdle)
                {
                    member.creatureAI.SwitchState(new StateExplore(member.creatureAI));
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
            baseNeededMembers = faction.currentHQ.Count;
            int basePriority = 1;
            basePriority += baseNeededMembers - membersWander.Count;

            if (membersWander.Count < baseNeededMembers)
            {
                AssigningMember(membersWander, "Wandering", 1, baseNeededMembers);
                yield return new WaitForSeconds(coroutineDelay);
            }

            if (membersWander.Count > baseNeededMembers)
            {
                CreatureController excessMember = membersWander[Random.Range(0, membersWander.Count)];
                UnAssigningMember(membersWander, excessMember, "Wandering");
            }

            for (int m = 0; m < membersWander.Count; m++)
            {
                CreatureController member = membersWander[m];
                if (!member.basicNeed && member.currentState is not StateAttack && member.currentState is not StateFlee)
                {
                    member.creatureAI.SwitchState(new StateWander(member.creatureAI));
                }
            }
            yield return new WaitForSeconds(coroutineDelay); // Adjust the wait time as needed
        }
    }

    public virtual IEnumerator PatrolCoroutine()
    {
        while (true)
        {
            int baseNeededMembers = 0;
            baseNeededMembers = faction.currentHQ.Count * 2;
            int basePriority = 1;
            basePriority += (baseNeededMembers * 2) - membersPatrol.Count;

            if (membersPatrol.Count < baseNeededMembers)
            {
                AssigningMember(membersPatrol, "Patrolling", 1, baseNeededMembers);
                yield return new WaitForSeconds(coroutineDelay);
            }

            if (membersPatrol.Count > baseNeededMembers)
            {
                CreatureController excessMember = membersPatrol[Random.Range(0, membersPatrol.Count)];
                UnAssigningMember(membersPatrol, excessMember, "Patrolling");
            }

            for (int m = 0; m < membersPatrol.Count; m++)
            {
                CreatureController member = membersPatrol[m];
                if (!member.basicNeed && member.currentState is not StateAttack && member.currentState is not StateFlee)
                {
                    member.creatureAI.SwitchState(new StatePatrol(member.creatureAI));
                }
            }
            yield return new WaitForSeconds(coroutineDelay); // Adjust the wait time as needed
        }
    }

}