using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class BlueBlop : BlopType
{
    private CreatureController blop;

    public BlueBlop(CreatureController blop) : base(blop)
    {
        this.blop = blop;
    }

      public override void Enter()
    {
        Color color= Color.blue;
        blop.GetComponent<SpriteRenderer>().color = color;
    }
    public override void Exit()
    {
        
    }
    
}
