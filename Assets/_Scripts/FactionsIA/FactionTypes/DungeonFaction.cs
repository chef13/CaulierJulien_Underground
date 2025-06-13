
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

public class DungeonFaction : FactionType
{
    public CreaturesBook creaturesBook;

    public DungeonFaction(FactionBehaviour faction) : base(faction)
    {
        this.faction = faction;
        creaturesBook = CreaturesBook.Instance;
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

    }

    protected override void CheckGoals()
    {
        foreach (var member in faction.members)
        {
            if (member.currentCreatureType is GobelinType && !faction.gobelins.Contains(member))
            {
                faction.gobelins.Add(member);
            }
            if (member.currentCreatureType is LezardType && !faction.Lezards.Contains(member))
            {
                faction.Lezards.Add(member);
            }
            if (member.currentCreatureType is ChampiType && !faction.Champs.Contains(member))
            {
                faction.Champs.Add(member);
                if (member.currentGoal != CreatureController.CreatureGoal.Escort)
                {
                    member.currentGoal = CreatureController.CreatureGoal.Escort;
                }
            }
        }
        if (faction.membersEscort.Count < creaturesBook.escorterGobsCount)
        {
            AssignGoblinToEscorter();
        }
        if (faction.membersRecoltFood.Count < creaturesBook.recolterGobsCount)
        {
            AssignGoblinToRecolter();
        }
        if (faction.membersPatrol.Count < creaturesBook.patrolerGobsCount)
        {
            AssignGoblinToPatroler();
        }

        foreach (var member in faction.members)
        {
            var currentGoal = member.currentGoal;
            if (currentGoal != CreatureController.CreatureGoal.None)
            {
                switch (currentGoal)
                {
                    case CreatureController.CreatureGoal.RecoltFood:
                        member.creatureAI.SwitchState(new StateRecolt(member.creatureAI));
                        break;
                    case CreatureController.CreatureGoal.FindHQ:
                        member.creatureAI.SwitchState(new StateExplore(member.creatureAI));
                        break;
                    case CreatureController.CreatureGoal.Wander:
                        member.creatureAI.SwitchState(new StateWander(member.creatureAI));
                        break;
                    case CreatureController.CreatureGoal.Patrol:
                        member.creatureAI.SwitchState(new StatePatrol(member.creatureAI));
                        break;
                    case CreatureController.CreatureGoal.Escort:
                        member.creatureAI.SwitchState(new StateEscort(member.creatureAI));
                        break;
                }
            }
        }

    }

    public override void AssignGoblinToEscorter()
    {
        Debug.Log("Looking for goblin to escorter");
        foreach (CreatureController goblin in faction.gobelins)
        {
            if (goblin.currentGoal == CreatureController.CreatureGoal.None)
            {
                Debug.Log("Assigning goblin to escorter");
                faction.membersEscort.Add(goblin);
                goblin.currentGoal = CreatureController.CreatureGoal.Escort;
                return;
            }
        }
    }

    public override void AssignChampToEscorter()
    {
        Debug.Log("Looking for champi to escorter");
        foreach (CreatureController champ in faction.Champs)
        {
            if (champ.currentGoal == CreatureController.CreatureGoal.None)
            {
                Debug.Log("Assigning champi to escorter");
                faction.membersEscort.Add(champ);
                champ.currentGoal = CreatureController.CreatureGoal.Escort;
                return;
            }
        }
    }

    public override void UnAssignGoblinToEscorter()
    {
        foreach (CreatureController goblin in faction.gobelins)
        {
            if (goblin.currentGoal == CreatureController.CreatureGoal.Escort)
            {
                UnAssigningMember(faction.membersEscort, goblin, FactionGoal.Escort);
                return;
            }
        }
    }

    public override void AssignGoblinToRecolter()
    {
        foreach (CreatureController goblin in faction.gobelins)
        {
            if (goblin.currentGoal == CreatureController.CreatureGoal.None)
            {
                faction.membersRecoltFood.Add(goblin);
                goblin.currentGoal = CreatureController.CreatureGoal.RecoltFood;
                return;
            }
        }
    }

    public override void UnAssignGoblinToRecolter()
    {
        foreach (CreatureController goblin in faction.gobelins)
        {
            if (goblin.currentGoal == CreatureController.CreatureGoal.RecoltFood)
            {
                UnAssigningMember(faction.membersRecoltFood, goblin, FactionGoal.RecoltFood);
                return;
            }
        }
    }

    public override void AssignGoblinToPatroler()
    {
        foreach (CreatureController goblin in faction.gobelins)
        {
            if (goblin.currentGoal == CreatureController.CreatureGoal.None)
            {
                faction.membersPatrol.Add(goblin);
                goblin.currentGoal = CreatureController.CreatureGoal.Patrol;
                return;
            }
        }
    }

    public override void UnAssignGoblinToPatroler()
    {
        foreach (CreatureController goblin in faction.gobelins)
        {   
            if (goblin.currentGoal == CreatureController.CreatureGoal.Patrol)
            {
                UnAssigningMember(faction.membersPatrol, goblin, FactionGoal.Patrol);
                return;
            }
        }
    }
}
