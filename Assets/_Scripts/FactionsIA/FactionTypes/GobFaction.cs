
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;

public class GobFaction : FactionType
{
    public new List<Coroutine> GoalsCoroutines;



    private IEnumerator FindHQCoroutine;
    List<CreatureController> membersFindHQ = new List<CreatureController>();
    private IEnumerator RecoltFoodCoroutine;
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

    private void CheckForAvaibleMember(CreatureState state)
    {
        // TODO: Implement logic for checking available member with the given state
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
