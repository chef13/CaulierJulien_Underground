using System.Collections;
using CrashKonijn.Agent.Core;
using UnityEngine;

public class StateAttack : CreatureState
{
    private CreatureController target;

    private Coroutine attackCoroutine;

    public StateAttack(CreatureAI creature, CreatureController target) : base(creature)
    {
        this.target = target;
    }

    public override void Update()
    {
        if (creature == null || creature.transform == null ||
            target == null || target.transform == null ||
            !target.gameObject.activeInHierarchy || target.isDead)
        {
            target = null;
            if (creature != null)
                creature.SwitchState(new StateIdle(creature));
            return;
        }

        float distance = Vector2.Distance(Controller.transform.position, target.transform.position);
        Vector2 directionToTarget = (target.transform.position - Controller.transform.position).normalized;

        if (Controller.attackTimer <= 0)
        {
            if (distance > Controller.data.attackRange)
            {
                // Move closer to get in range
                Controller.SetDestination((Vector2)target.transform.position);
            }
            else if (attackCoroutine == null)
            {
                attackCoroutine = creature.StartCoroutine(Attack(target));
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

    private IEnumerator Attack(CreatureController target)
    {
            if (target.transform.position.x < creature.transform.position.x)
            {
                Controller.spriteRenderer.flipX = true;
            }
            else
            {
                Controller.spriteRenderer.flipX = false;
            }
            Controller.animator.SetTrigger("Attack");
            Controller.agent.isStopped = true; // Stop moving while attacking
            Controller.currentEnergy -= Controller.data.attackPower / 5;

            target.OnHit( Controller, Controller.data.attackPower);
            Controller.attackTimer = Controller.data.attackSpeed;
            

            yield return new WaitForSeconds(Controller.animator.GetCurrentAnimatorStateInfo(0).length);
            Controller.agent.isStopped = false; // Resume moving after attack
            if (target.isDead)
            {
                target = null; // Clear target if it's dead
            }
            attackCoroutine = null;
    }

    
}
