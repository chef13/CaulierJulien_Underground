
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class FactionType
{
    protected FactionBehaviour faction;
    public string factionName;
    public GameObject unitsPrefab;

    
    
    public FactionType(FactionBehaviour faction)
    {
        this.faction = faction;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update() { }
    public virtual bool PotencialHQ(GameObject room)
    {
        return false;
    }

    public virtual void AskForState()
    {
        
    }

    public virtual bool PotencialHQ(RoomInfo room) { return false; }
}