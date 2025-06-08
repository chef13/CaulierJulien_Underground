
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.AI;

public class ChampiType : CreatureType
{
    

    public ChampiType(CreatureController creature) : base(creature)
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


    public override void IsEaten()
    {
        int randomChampi = Random.Range(2, 5);
        while (randomChampi != 0)
        {
            if (Controller.currentTile == null && Controller.currentTile.objects.Count < 4)
            FlaureSpawner.instance.spawnQueue.Enqueue((Controller.currentTile, FlaureSpawner.instance.champiData, Controller.currentTile.position + new Vector3(Random.Range(0.1f, 0.9f), Random.Range(0.1f, 0.9f), 0)));
            randomChampi--;

        }
        Destroy();
    }


    public override void OnDeath(CreatureController attacker)
    {
        if (Controller.currentFaction != null && attacker != null && attacker != null)
        {
            if (attacker != null && Controller.currentFaction.knownFactions.TryGetValue(attacker.currentFaction, out FactionBehaviour.FactionRelationship relationship))
            {
                relationship -= 2;
            }
        }
        Controller.StopAllCoroutine();
        Controller.StartCoroutine(DeathDecay());
        Controller.spriteRenderer.sprite = Controller.data.deadsprite;
        Controller.animator.enabled = false;
        Controller.isDead = true;
        Controller.isCorpse = true;
        Controller.agent.isStopped = true;
        Controller.agent.speed = 0f;
        Controller.currentFaction.members.Remove(Controller);
        CreatureSpawner.Instance.livingCreatures.Remove(Controller);
    }

    
    public override IEnumerator DeathDecay()
    {
        int decayDelay = Random.Range(60, 120);
        while (decayDelay > 0)
        {
            decayDelay--;
            yield return new WaitForSeconds(1f);
        }
        IsEaten();
        yield break;
    }

    public override void Destroy()
    {
        Controller.gameObject.SetActive(false);
        CreatureSpawner.Instance.creaturesGarbage.Add(Controller.gameObject);
    }
}
