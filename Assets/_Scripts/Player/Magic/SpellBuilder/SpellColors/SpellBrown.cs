using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class SpellBrown : SpellColor
{
    public SpellBrown(Spell spell) : base(spell)
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
    public override void EffectOnTile(TileInfo tile) { }
    public override void EffectOnRoom(RoomInfo room) { }
    public override void EffectOnCreature(CreatureController creature) { }
    public override void EffectOnFaction(FactionBehaviour faction, float duration)
    {
        spell.SpellEffectCoroutine = spell.StartCoroutine(BrownBless(faction, duration));
    }
    public override void EffectOnBeam(Vector2Int beamTarget) { }
    public override void EffectOnDungeon( float duration) { }
    public override void EffectOnArea(List<TileInfo> area) { }
    

    public IEnumerator BrownBless(FactionBehaviour faction, float duration)
    {
        float timer = duration;
        
        faction.dungeonFav += spell.spellPower;
        for (int i = 0; i < faction.members.Count; i++)
        {
            CreatureController creature = faction.members[i];
            if (creature != null && !creature.isDead)
            {
                creature.agent.speed -= (float)spell.spellPower * 0.1f;
            }
        }
        while (timer > 0)
        {
            timer -= Time.deltaTime;
                    for (int i = 0; i < faction.members.Count; i++)
            {
                CreatureController creature = faction.members[i];
                if (creature != null && !creature.isDead && creature.currentEnergy < creature.data.maxEnergy)
                {
                    creature.currentEnergy += spell.spellPower; // Increase energy by spell power
                }
            }
            yield return null;
        }
        for (int i = 0; i < faction.members.Count; i++)
        {
            CreatureController creature = faction.members[i];
            if (creature != null && !creature.isDead)
            {
                creature.agent.speed = creature.data.speed; // Reset speed to original value
            }
        }
        yield break;
    }
}
