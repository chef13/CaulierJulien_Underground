
using UnityEngine;
using System.Collections.Generic;

public class LezardFaction : FactionType
{

    List<CreatureController> membersFindHQ = new List<CreatureController>();
    public LezardFaction(FactionBehaviour gob) : base(gob)
    {

    }

    public override void Enter()
    {
    }
    public override void Exit()
    {

    }

    public override void Update()
    {
        goalCheckTime -= Time.deltaTime;
        
        if (goalCheckTime <= 0f)
        {
            goalCheckTime = 1f; // Reset the timer
            CheckGoals();
        }
    }

    protected override void CheckGoals()
    {
        if (faction.currentHQ.Count == 0 || faction.currentHQ.Count / faction.members.Count >= faction.factionData.maxMembersPerHQ)
        {
            FindHQ();
        }
     }


    public override bool PotencialHQ(RoomInfo room)
    {
        int waterCount = 0;


        foreach (TileInfo tile in room.tiles)
        {
            if (tile.isWater) waterCount++;
        }
        // Early return if both conditions are met
        if (waterCount >= 4)
            return true;


        return false;
    }
    
     public void FindHQ()
    {
        Debug.Log("GobFaction: Checking for available HQ");
      
            if (faction.currentHQ.Count == 0)
            {
                foreach (var member in faction.members)
                {
                    CreatureController creature = member.GetComponent<CreatureController>();
                    membersFindHQ.Add(creature);
                    Debug.Log($"GobFaction: {creature.name} is looking for HQ");
                    creature.currentGoal = "Finding HQ";
                }
            }
            // Loop through all known rooms

            if (faction.currentHQ.Count * faction.factionData.maxMembersPerHQ >= faction.members.Count)
                foreach (var member in  faction.members)
                {
                    CreatureController creature = member.GetComponent<CreatureController>();
                    if (creature.currentState is StateIdle)
                {
                    membersFindHQ.Add(creature);
                    Debug.Log($"GobFaction: {creature.name} is looking for HQ");
                    creature.currentGoal = "Finding HQ";
                }
                }
            foreach (var room in faction.knownRoomsDict.Values)
            {

                // Check if this room is a potential HQ
                if (PotencialHQ(room))
                {
                    faction.currentHQ.Add(room);
                    faction.numberOfHQ++;
                    Debug.Log($"HQ found at {room.index}");
                    foreach (var member in membersFindHQ)
                    {
                        CreatureController creature = member.GetComponent<CreatureController>();

                        creature.currentGoal = "no Goal";
                        
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

}
