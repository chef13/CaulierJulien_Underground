
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
public abstract class CreatureType
{
    protected CreatureController creature;
    public string creatureName;



    public CreatureType(CreatureController creature)
    {
        this.creature = creature;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update() { }

}