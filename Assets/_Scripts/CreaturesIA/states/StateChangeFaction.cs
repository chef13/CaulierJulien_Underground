using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class StateChangeFaction : CreatureState
{
    public ManaCore manaCore;
    public FactionSpawner factionSpawner;
    bool closeToCore = false;
    public StateChangeFaction(CreatureAI creature) : base(creature)
    {

    }

    public override void Enter()
    {
        manaCore = ManaCore.Instance;
        factionSpawner = FactionSpawner.instance;



        Controller.SetDestination(manaCore.transform.position);
    }

    public override void Update()
    {

        float distance = Vector3.Distance(Controller.transform.position, manaCore.transform.position);
        closeToCore = distance < Controller.data.attackRange * 4;
        if (!Controller.hasDestination && !closeToCore)
        {
            Controller.SetDestination(manaCore.transform.position);
        }
        else if (closeToCore)
        {
            Controller.currentFaction.members.Remove(Controller);
            Controller.currentFaction = factionSpawner.dungeonFaction;
            Controller.currentFaction.members.Add(Controller);
            creature.SwitchState(new StateIdle(creature));
            Controller.transform.SetParent(Controller.currentFaction.transform);
        }
            
        

    }




    
}
