
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

using UnityEngine;
using UnityEngine.AI;

public class LezardFaction : FactionType
{
    //public new List<Coroutine> GoalsCoroutines = new List<Coroutine>();



    private Coroutine FindHQCoroutine;
    //List<CreatureController> membersFindHQ = new List<CreatureController>();
    private Coroutine RecoltFoodCoroutine;
    //List<CreatureController> membersRecoltFood = new List<CreatureController>();
    private Coroutine WanderCoroutine;

    private Coroutine PatrolerCoroutine;
    public LezardFaction(FactionBehaviour gob) : base(gob)
    {

    }

    private void Start()
    {
        //CheckGoals();
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
            if (FindHQCoroutine == null)
                FindHQCoroutine = faction.StartCoroutine(FindHQ());
        }
        if (faction.currentHQ.Count == 0 || faction.currentHQ.Count * faction.members.Count > faction.currentHQ.Count * faction.factionData.maxMembersPerHQ)
        {
            if (FindHQCoroutine != null)
            {
                faction.StopCoroutine(FindHQCoroutine);
                FindHQCoroutine = null;
            }
        }
        if (faction.currentHQ.Count != 0)
        {
            if (RecoltFoodCoroutine == null)
                RecoltFoodCoroutine = faction.StartCoroutine(RecoltFoodgoal());
        }

        if (faction.currentHQ.Count != 0)
        {
            if (WanderCoroutine == null)
                WanderCoroutine = faction.StartCoroutine(WandererCoroutine());

            if (PatrolerCoroutine == null)
                PatrolerCoroutine = faction.StartCoroutine(PatrolCoroutine());
        }
                if (faction.currentHQ.Count != 0)
        {
            if (checkDungeonRelationshipCoroutine == null)
            {
                checkDungeonRelationshipCoroutine = faction.StartCoroutine(CheckDungeonRelationshipCoroutine());
            }
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
       // Debug.Log($"Checking room {room.index}: nature={natureCount}, water={waterCount}");
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
           // Debug.Log($"Checking connected room {connectedroom.index}: nature={natureCount}, water={waterCount}");
            if (waterCount <= 2)
                return true;
        }
        return false;
    }
    

}
