
using UnityEngine;
using UnityEngine.AI;

public class GobelinType : CreatureType
{
    

    public GobelinType(CreatureController creature) : base(creature)
    {

    }

    private void Start()
    {
        
    }

    public override void Enter()
    {
        Controller.currentFaction.gobelins.Add(Controller);
    }
    public override void Exit()
    {

    }


    public override void Update()
    {
      
    }

   
    

}
