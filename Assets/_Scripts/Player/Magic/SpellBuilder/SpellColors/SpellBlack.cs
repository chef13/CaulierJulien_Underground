using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using UnityEngine.AI;
using System.Collections;
using System.Linq;
public class SpellBlack : SpellColor
{
    public SpellBlack(Spell spell) : base(spell)
    {
        this.spell = spell;
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public override void EffectOnTile(TileInfo tile)
    {
        if (tile.objects.Count > 0)
        {
            // Remove all objects on the tile
            for (int i = tile.objects.Count - 1; i >= 0; i--)
            {
                GameObject obj = tile.objects[i];
                if (obj == null) continue;

                // If the object is a FlaureBehaviour, reduce its growth time
                // and gain mana based on the spell power
                {
                    FlaureBehaviour flaure = obj.GetComponent<FlaureBehaviour>();
                    if (flaure != null)
                    {
                        flaure.currentGrowthTime -= spell.spellPower * 5;
                        ManaCore.GainMana(spell.spellPower * 5);
                        if (flaure.currentGrowthTime <= 0)
                        {
                            flaure.IsEaten();
                        }
                    }

                }
            }
        }
        else if (tile.isNature)
        {
            int runRandomAbsorb = Random.Range(0, 20);
            if (runRandomAbsorb < 5)
            {
                tile.isNature = false;
                TilemapVisualizer.eraseTile(TilemapVisualizer.natureTilemap, new Vector2Int(tile.position.x, tile.position.y));
                ManaCore.GainMana(spell.spellPower * 5);
            }
        }
    }
    public override void EffectOnRoom(RoomInfo room) { }
    public override void EffectOnCreature(CreatureController creature)
    {
        if (creature != null)
        {
            if (ManaCore.Instance.currentAvatar != null)
            {
                var attacker = ManaCore.currentAvatar.GetComponent<CreatureController>();
                creature.OnHit(attacker, spell.spellPower * 10, false);
                ManaCore.GainMana(spell.spellPower * 5);
            }
            else
            {
                creature.OnHit(null, spell.spellPower * 10, false);
                ManaCore.GainMana(spell.spellPower * 5);
            }
            creature.currentFaction.dungeonFav--;
        }
    }

    public override void EffectOnAlliedCreature(CreatureController creature)    {    }
    public override void EffectOnEnnemisCreature(CreatureController creature)    {    }
    public override void EffectOnFaction(FactionBehaviour faction, float duration)
    {
        spell.SpellEffectCoroutine = spell.StartCoroutine(Sacrifying(faction, duration));
    }
    public override void EffectOnBeam(Vector2Int beamTarget) { }
    public override void EffectOnDungeon(float duration)
    { 
        spell.SpellEffectCoroutine = spell.StartCoroutine(DoomDungeon( duration));
    }
    public override void EffectOnArea(List<TileInfo> area) { }
    
    private IEnumerator DoomDungeon(float duration)
    {
        float timer = duration;
        foreach (var faction in FactionSpawner.factionsIA)
        {
            if (faction != null)
            {
                faction.dungeonFav -= spell.spellPower * 2;
            }
        }   
        Debug.Log($"Dooming the dungeon for {spell.spellDuration} seconds to gain mana.");
        while (timer > 0)
        {
            timer -= Time.deltaTime;

            var randomCreature = CreatureSpawner.Instance.livingCreatures[Random.Range(0, CreatureSpawner.Instance.livingCreatures.Count)];
            if (randomCreature != null)
            {
                if (!randomCreature.isDead)
                {
                    randomCreature.OnHit(null, spell.spellPower * 10, true);
                    ManaCore.GainMana(spell.spellPower * 5);
                }
                else if (randomCreature.isDead && randomCreature.isCorpse)
                {
                    randomCreature.currentCreatureType.IsEaten();
                    ManaCore.GainMana(spell.spellPower * 5);
                }
                else if (randomCreature.isDead && !randomCreature.isCorpse)
                {
                    randomCreature.currentCreatureType.Destroy();
                    ManaCore.GainMana(spell.spellPower * 5);
                }
            }
            yield return new WaitForSeconds(1f);
        }
        yield break;
    }

    private IEnumerator Sacrifying(FactionBehaviour faction, float duration)
    {
        int spellPower = spell.spellPower;
        faction.dungeonFav -= spell.spellPower * 2;
        List<CreatureController> targetedSacrifices = new List<CreatureController>();
        FactionBehaviour currentFaction = faction;
        float timer = spell.spellDuration * duration; //
        Debug.Log($"Sacrifying {spellPower} creatures from {faction.name} to gain mana for {spell.spellDuration} seconds.");
        while (spellPower > 0)
        {
            var creatureToSacrify = faction.members[Random.Range(0, faction.members.Count)];
            if (!targetedSacrifices.Contains(creatureToSacrify))
            {
                targetedSacrifices.Add(creatureToSacrify);
                creatureToSacrify.currentFaction = dungeonFaction;
                spellPower--;
                Debug.Log($"Sacrificing {creatureToSacrify.name} from {faction.name}. Remaining sacrifices: {spellPower}");
            }
        }
        while (targetedSacrifices.Count > 0 || timer > 0)
        {
            for (int i = targetedSacrifices.Count - 1; i >= 0; i--)
            {
                var creature = targetedSacrifices[i];
                if (creature == null || creature.isDead)
                {
                    targetedSacrifices.RemoveAt(i);
                    continue;
                }

                float distance = Vector2.Distance(creature.transform.position, ManaCore.Instance.transform.position);

                if (creature.agent.enabled && creature.agent.isOnNavMesh)
                {
                    creature.SetDestination(ManaCore.Instance.transform.position);
                }

                if (distance <= 1f)
                {
                    creature.OnHit(null, creature.data.maxLife, true);
                    ManaCore.Instance.maxMana += creature.data.maxLife;
                    targetedSacrifices.RemoveAt(i);
                }
            }

            timer -= Time.deltaTime;

            if (timer <= 0)
            {
                foreach (var creature in targetedSacrifices)
                {
                    if (creature != null && !creature.isDead)
                    {
                        creature.currentFaction = currentFaction;
                        creature.SetDestination(creature.transform.position);
                    }
                }
                break;
            }

            yield return new WaitForSeconds(1f);
        }


        yield break;
    }
}
