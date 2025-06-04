
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.AI;

public class GobFaction : FactionType
{
    //public new List<Coroutine> GoalsCoroutines = new List<Coroutine>();



    private Coroutine FindHQCoroutine;
    List<CreatureController> membersFindHQ = new List<CreatureController>();
    private Coroutine RecoltFoodCoroutine;
    List<CreatureController> membersRecoltFood = new List<CreatureController>();
    private IEnumerator LaborCoroutine;
    private IEnumerator GuardHQCoroutine;
    private IEnumerator PatrolCoroutine;
    private IEnumerator InvadeCoroutine;
    private IEnumerator ExploreCoroutine;

    public GobFaction(FactionBehaviour gob) : base(gob)
    {

    }

    private void Start()
    {
        CheckGoals();
    }

    public override void Enter()
    {
    }
    public override void Exit()
    {

    }


    public override void Update()
    {
        /*goalCheckTime -= Time.deltaTime;
        
        if (goalCheckTime <= 0f)
        {
            goalCheckTime = 1f; // Reset the timer
            CheckGoals();
        }*/
    }

    protected override void CheckGoals()
    {
        if (faction.currentHQ.Count == 0 || faction.currentHQ.Count * faction.members.Count <= faction.currentHQ.Count * faction.factionData.maxMembersPerHQ)
        {
            //FindHQ();
            if (FindHQCoroutine == null)
                FindHQCoroutine = faction.StartCoroutine(FindHQgoal());
        }

        if (faction.currentHQ.Count != 0)
        {
            if (RecoltFoodCoroutine == null)
                RecoltFoodCoroutine = faction.StartCoroutine(RecoltFoodgoal());
        }
    }



    public override bool PotencialHQ(RoomInfo room)
    {
        int natureCount = 0;
        int waterCount = 0;

        foreach (TileInfo tile in room.tiles)
        {
            if (tile.isNature) natureCount++;
            if (tile.isWater) waterCount++;
        }
        Debug.Log($"Checking room {room.index}: nature={natureCount}, water={waterCount}");
        if (natureCount >= 3 && waterCount >= 2)
            return true;

        // Optionally check connected rooms as well
        foreach (var connectedroom in room.connectedRooms)
        {
            natureCount = 0;
            waterCount = 0;
            foreach (TileInfo tile in connectedroom.tiles)
            {
                if (tile.isNature) natureCount++;
                if (tile.isWater) waterCount++;
            }
            Debug.Log($"Checking connected room {connectedroom.index}: nature={natureCount}, water={waterCount}");
            if (natureCount >= 3 && waterCount >= 2)
                return true;
        }
        return false;
    }


    public IEnumerator FindHQgoal()
    {
        while (true)
        {

            FindHQ();

            yield return new WaitForSeconds(1f); // Adjust the wait time as needed
        }
    }
    public void FindHQ()
    {
        Debug.Log("GobFaction: Checking for available HQ");

        if (faction.currentHQ.Count == 0)
        {
            AssigningMember(membersFindHQ, "Finding HQ", 3, faction.members.Count);
        }
        // Loop through all known rooms

        if (faction.currentHQ.Count * faction.members.Count >= faction.currentHQ.Count * faction.factionData.maxMembersPerHQ)
        {
            AssigningMember(membersFindHQ, "Finding HQ", 3, faction.members.Count / 2);
            //Debug.Log("GobFaction: Members are looking for HQ");
        }

        if (faction.currentHQ.Count * faction.members.Count < faction.currentHQ.Count * faction.factionData.maxMembersPerHQ)
        {
            Debug.Log("GobFaction: HQ already found, stopping search.");
            return; // Stop searching if HQ is already found
        }
        foreach (var room in faction.knownRoomsDict.Values)
        {

            // Check if this room is a potential HQ
            if (PotencialHQ(room) && !faction.currentHQ.Contains(room))
            {
                faction.currentHQ.Add(room);
                room.faction = faction; // Assign the faction to the room
                foreach (TileInfo tile in room.tiles)
                {
                    tile.faction = faction; // Assign the faction to the tiles
                }
                faction.numberOfHQ++;
                Debug.Log($"HQ found at {room.index}");
                for (int i = membersFindHQ.Count - 1; i >= 0; i--)
                {
                    UnAssigningMember(membersFindHQ, membersFindHQ[i], "Finding HQ");
                }


                return; // Stop coroutine when HQ is found
            }
        }

        foreach (var member in membersFindHQ)
        {
            // If no potential HQ found, switch member state to explore
            if (member.currentState is StateIdle)
            {
                member.creatureAI.SwitchState(new StateExplore(member.creatureAI));
            }
        }

    }
    
    public IEnumerator RecoltFoodgoal()
    {
        while (true)
        {
            int baseNeededMembers = 2;
            int basePriority = 1;
            int foodNeeded = faction.members.Count * 25; // Assuming each member needs 2 food units

            if (membersRecoltFood.Count <= faction.currentHQ.Count)
            {
                AssigningMember(membersRecoltFood, "Recolt Food", basePriority * 5, faction.currentHQ.Count);
            }

            if (faction.foodResources == 0)
            {
                baseNeededMembers +=  Mathf.RoundToInt(faction.members.Count * 0.5f); // Ensure at least one member is assigned
                basePriority += 3;
            }

            if (faction.foodResources <  Mathf.RoundToInt(foodNeeded * 0.4f))
            {
                baseNeededMembers ++; // Increase the number of members needed
                basePriority ++;
            }
            
            if (faction.foodResources <  Mathf.RoundToInt(foodNeeded * 0.8f))
            {
                baseNeededMembers++; // Increase the number of members needed
                basePriority++;
            }

            if (membersRecoltFood.Count < baseNeededMembers)
            {
                AssigningMember(membersRecoltFood, "Recolt Food", basePriority, baseNeededMembers);
            }



            if (membersRecoltFood.Count == 0)
                {
                    Debug.Log("GobFaction: No members assigned to Recolt Food");
                }

            
                if (membersRecoltFood.Count > baseNeededMembers)
                {
                    int  excessMembers = membersRecoltFood.Count - baseNeededMembers;

                    if (membersRecoltFood.Count > baseNeededMembers)
                    {
                        List<CreatureController> excessMembersList = new List<CreatureController>();
                        foreach (CreatureController excessmember in membersRecoltFood)
                        {
                            if (
                                excessmember.currentRoom != null &&
                                excessmember.currentRoom.faction != null &&
                                excessmember.currentFaction != null &&
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
                if (member.currentRoom != null && member.currentRoom.faction == member.currentFaction && member.currentResources != 0)
                {
                    faction.foodResources += member.currentResources;
                    member.currentResources = 0;
                }
                
                    if (!member.basicNeed && member.currentState is not StateAttack && member.currentState is not StateFlee)
                {
                    member.creatureAI.SwitchState(new StateRecolt(member.creatureAI));
                }
            }
            
            yield return new WaitForSeconds(1f); // Adjust the wait time as needed
            }

        }
    

}
