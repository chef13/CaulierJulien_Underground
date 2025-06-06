
using UnityEngine;

public class GreenBlop : BlopType
{
    private CreatureController blop;
    

    public GreenBlop(CreatureController blop) : base(blop)
    {
        this.blop = blop;
    }

      public override void Enter()
    {
        Color color= Color.green;
        blop.GetComponent<SpriteRenderer>().color = color;
    }
}
