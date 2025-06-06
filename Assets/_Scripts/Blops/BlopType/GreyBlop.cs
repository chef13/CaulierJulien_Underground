using UnityEngine;

public class GreyBlop : BlopType
{
   private CreatureController blop;

    public GreyBlop(CreatureController blop) : base(blop)
    {
        this.blop = blop;
    }

      public override void Enter()
    {
        Color color= Color.grey;
        blop.GetComponent<SpriteRenderer>().color = color;
    }
}
