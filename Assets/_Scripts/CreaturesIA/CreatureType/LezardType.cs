
using UnityEngine;
using UnityEngine.AI;

public class LezardType : CreatureType
{
    

    public LezardType(CreatureController creature) : base(creature)
    {

    }

    private void Start()
    {
    }

    public override void Enter()
    {
    }
    public override void Exit()
    {

    }


    public override void Update()
    {
        /*goalCheckTime -= Time.deltaTime;
        
        if (goalCheckTime <= 0f)
        {
            goalCheckTime = 1f; // Reset the timer
            CheckGoals();
        }*/
    }

    
    

}
