
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.AI;

public class WandererFaction : FactionType
{
    //public new List<Coroutine> GoalsCoroutines = new List<Coroutine>();

    FactionBehaviour faction;
    public List<CreatureController> wanderingCreatures = new List<CreatureController>();
    private Coroutine wandererCoroutine;

    public WandererFaction(FactionBehaviour faction) : base(faction)
    {
        this.faction = faction;
        //goalCheckTime = 1f; // Initialize the timer
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
        if (wandererCoroutine == null)
        {
            wandererCoroutine = faction.StartCoroutine(WandererCoroutine());
        }
    }

    protected override void CheckGoals()
    {
       

    }

    public override IEnumerator WandererCoroutine()
    {
        AssigningMember(wanderingCreatures, FactionGoal.Wander, 5, 10);
        yield return null; // Placeholder for coroutine logic
    }

    
    

}
