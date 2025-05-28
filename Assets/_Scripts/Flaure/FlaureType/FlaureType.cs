
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
public abstract class FlaureType
{
    protected FlaureBehaviour flaure;
    public string flaureName;



    public FlaureType(FlaureBehaviour flaure)
    {
        this.flaure = flaure;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update() { }

    public virtual IEnumerator Grow()
    {
        yield return null;
    }

    public virtual void Expand()
    {

    }

}