using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class StateAttackCore : CreatureState
{
    public ManaCore manaCore;
    bool closeToCore = false;
    public Coroutine attackCoroutine;
    public StateAttackCore(CreatureAI creature) : base(creature)
    {

    }

    public override void Enter()
    {
        manaCore = ManaCore.Instance;

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
            Vector2 directionToTarget = (manaCore.transform.position - creature.transform.position).normalized;
                if (Controller.attackTimer <= 0)
            {
                if (distance > Controller.data.attackRange)
                {
                    // Move closer to get in range
                    Controller.SetDestination((Vector2)manaCore.transform.position);
                }
                else if (attackCoroutine == null)
                {
                    Controller.attackCoreRoutine = creature.StartCoroutine(Controller.currentCreatureType.AttackCore(manaCore));
                }
            }
            else // on cooldown
            {
                if (distance < Controller.data.attackRange * 0.9f)
                {
                    // Too close, back up a little
                    Vector2 retreatPos = (Vector2)creature.transform.position - directionToTarget * 1.5f;
                    Controller.SetDestination(retreatPos);
                }
                else if (distance > Controller.data.attackRange * 1.2f)
                {
                    // Too far, move a bit closer
                    Vector2 advancePos = (Vector2)creature.transform.position + directionToTarget * 1.5f;
                    Controller.SetDestination(advancePos);
                }
                else
                {
                    // Already in reasonable range — maybe strafe?
                    Vector2 strafeDir = Vector2.Perpendicular(directionToTarget).normalized;
                    Vector2 strafePos = (Vector2)creature.transform.position + strafeDir * 1f;
                    Controller.SetDestination(strafePos);
                }
            }
            
        }
      


            
            
        

    }

    
}
