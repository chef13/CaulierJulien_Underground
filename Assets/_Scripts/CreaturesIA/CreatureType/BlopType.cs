
using System.Collections;
using UnityEngine;
public class BlopType : CreatureType
{
    //public new List<Coroutine> GoalsCoroutines = new List<Coroutine>();



    private Coroutine FindHQCoroutine;
    //List<CreatureController> membersFindHQ = new List<CreatureController>();
    private Coroutine RecoltFoodCoroutine;
    //List<CreatureController> membersRecoltFood = new List<CreatureController>();
    private IEnumerator LaborCoroutine;
    private IEnumerator GuardHQCoroutine;
    private IEnumerator PatrolCoroutine;
    private IEnumerator InvadeCoroutine;
    private IEnumerator ExploreCoroutine;

    public BlopType(CreatureController gob) : base(gob)
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
        
    }

    
    

}
