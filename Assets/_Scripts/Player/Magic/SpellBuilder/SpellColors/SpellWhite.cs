using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Collections;
public class SpellWhite : SpellColor
{
    FactionSpawner FactionSpawner => FactionSpawner.instance;
    public SpellWhite(Spell spell) : base(spell)
    {
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
                FlaureBehaviour flaure = obj.GetComponent<FlaureBehaviour>();
                if (flaure != null)
                {
                    flaure.currentGrowthTime += spell.spellPower * 5;
                    if (flaure.currentFlaureType is ChampiFlaure && flaure.currentGrowthTime >= flaure.flaureData.growingTime)
                    {
                        // If the Flaure is a ChampiFlaure and has grown enough, turn it into a living creature
                        flaure.currentFlaureType.TurnLivingCreature(FactionSpawner.dungeonFaction);
                    }
                }
            }

        }

    }
    public override void EffectOnRoom(RoomInfo room) { }
    public override void EffectOnCreature(CreatureController creature)
    {

    }
    public override void EffectOnFaction(FactionBehaviour faction, float duration)
    {
        spell.SpellEffectCoroutine = spell.StartCoroutine(BlessFaction(faction, duration));
    }
    public override void EffectOnBeam(Vector2Int beamTarget) { }
    public override void EffectOnDungeon( float duration) { }
    public override void EffectOnArea(List<TileInfo> area) { }


    public IEnumerator BlessFaction(FactionBehaviour faction, float duration)
    {
        float timer = duration;
        faction.dungeonFav += spell.spellPower*3 + spell.spellPower;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            for (int i = 0; i < faction.members.Count; i++)
            {
                CreatureController creature = faction.members[i];
                if (creature.currentHP < creature.data.maxLife)
                {
                    creature.currentHP += spell.spellPower *2;
                }
            }
            yield return new WaitForSeconds(1f);
        }
        faction.dungeonFav -= spell.spellPower*3;
        yield break;
    }
}
