
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

public abstract class FactionType
{
    public FactionBehaviour faction;
    public GameObject unitsPrefab;
    private Coroutine mainCoroutineForGoals;
    public Coroutine _mainCoroutineForGoals
    {
        get { return mainCoroutineForGoals; }
        set { mainCoroutineForGoals = value; }
    }

    private List<Coroutine> goalsCoroutines = new List<Coroutine>();
    public List<Coroutine> GoalsCoroutines
    {
        get { return goalsCoroutines; }
        set { goalsCoroutines = value; }
    }

    protected float goalCheckTime = 1f;


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
    public virtual void Update() { }

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

    public virtual void NeedHQ()
    {
        // Default implementation (can be empty or log a warning)
    }
    public virtual bool PotencialHQ(RoomInfo room) { return false; }
    
    
    
}