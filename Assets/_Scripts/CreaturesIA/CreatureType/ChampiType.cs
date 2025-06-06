
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
        int randomChampi = Random.Range(0, 3);
        while (randomChampi != 0)
        {
            TileInfo tile = DungeonGenerator.Instance.dungeonMap.TryGetValue(new Vector3Int(
            Controller.currentTile.position.x + Random.Range(-1, 2), Controller.currentTile.position.y + Random.Range(-1, 2), 0), out TileInfo tileInfo) ? tileInfo : null;
            FlaureSpawner.instance.spawnQueue.Enqueue((tile, FlaureSpawner.instance.champiData, tile.position + new Vector3(Random.Range(0.1f, 0.9f), Random.Range(0.1f, 0.9f), 0)));
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
        int decayDelay = Random.Range(10, 20);
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
