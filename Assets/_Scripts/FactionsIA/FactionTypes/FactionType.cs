
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

public abstract class FactionType
{
    public FactionBehaviour faction;
    private Coroutine mainCoroutineForGoals;
    public Coroutine _mainCoroutineForGoals
    {
        get { return mainCoroutineForGoals; }
        set { mainCoroutineForGoals = value; }
    }

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
        for (int m = 0; m < faction.members.Count; m++)
        {
            CreatureController member = faction.members[m].GetComponent<CreatureController>();
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
    
    
}