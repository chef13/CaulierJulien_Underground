
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using UnityEngine.Events;
public abstract class CreatureType
{
    protected CreatureController Controller;
    public string creatureName;
    public Coroutine attackCoroutine;

    public UnityEvent OnDeathEvent = new UnityEvent();

    public CreatureType(CreatureController creature)
    {
        this.Controller = creature;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update() { }


    private IEnumerator Attack(CreatureController target)
    {
        if (target.transform.position.x < Controller.transform.position.x)
        {
            Controller.spriteRenderer.flipX = true;
        }
        else
        {
            Controller.spriteRenderer.flipX = false;
        }
        Controller.animator.SetTrigger("Attack");
        Controller.agent.isStopped = true; // Stop moving while attacking
        Controller.ChangeEnergy(Controller.data.attackPower / 5);

        target.OnHit(Controller, Controller.data.attackPower);
        Controller.attackTimer = Controller.data.attackSpeed;


        yield return new WaitForSeconds(Controller.animator.GetCurrentAnimatorStateInfo(0).length);
        Controller.agent.isStopped = false; // Resume moving after attack
        if (target.isDead)
        {
            target = null; // Clear target if it's dead
        }
        if (attackCoroutine != null)
        {
            Controller.StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
    }



    public virtual void IsEaten()
    {
        Controller.isCorpse = false;
        Controller.spriteRenderer.sprite = Controller.data.skeletonSprite;
        if (Controller.gameObject.activeInHierarchy)
            Controller.StartCoroutine(DeathDecay());
    }


    public virtual void OnDeath(CreatureController attacker, bool spell = false)
    {
        if (Controller.currentFaction != FactionSpawner.instance.dungeonFaction && attacker != null)
        {
                Controller.currentFaction.dungeonFav --;
        }
        Controller.StopAllCoroutine();
        Controller.spriteRenderer.sprite = Controller.data.deadsprite;
        Controller.animator.enabled = false;
        Controller.isDead = true;
        Controller.isCorpse = true;
        Controller.agent.isStopped = true;
        Controller.agent.speed = 0f;
        Controller.currentFaction.UnassigneCreatreAtDeath(Controller);
        OnDeathEvent.Invoke();
    }


    public virtual IEnumerator DeathDecay()
    {
        int decayDelay = Random.Range(10, 20);
        while (decayDelay > 0)
        {
            decayDelay--;
            yield return new WaitForSeconds(1f);
        }
        Destroy();
        yield break;
    }

    public virtual void Destroy()
    {
        Controller.gameObject.SetActive(false);
        CreatureSpawner.Instance.creaturesGarbage.Add(Controller.gameObject);
    }
    


    public IEnumerator AttackCore(ManaCore target)
    {
        if (target.transform.position.x < Controller.transform.position.x)
        {
            Controller.spriteRenderer.flipX = true;
        }
        else
        {
            Controller.spriteRenderer.flipX = false;
        }
        Controller.animator.SetTrigger("Attack");
        Controller.agent.isStopped = true; // Stop moving while attacking
        Controller.ChangeEnergy(Controller.data.attackPower / 5);

        target.OnHit(null, Controller.data.attackPower);
        Controller.attackTimer = Controller.data.attackSpeed;


        yield return new WaitForSeconds(Controller.animator.GetCurrentAnimatorStateInfo(0).length);
        Controller.agent.isStopped = false; // Resume moving after attack
        if (attackCoroutine != null)
        {
            Controller.StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
    }
}