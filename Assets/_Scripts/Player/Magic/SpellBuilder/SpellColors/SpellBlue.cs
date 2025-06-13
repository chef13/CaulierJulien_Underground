using UnityEngine;
using System.Collections.Generic;
using System.Collections;
public class SpellBlue : SpellColor
{
    public SpellBlue(Spell spell) : base(spell)
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
            foreach (var obj in tile.objects)
            {
                FlaureBehaviour flaure = obj.GetComponent<FlaureBehaviour>();
                if (flaure != null)
                {
                    flaure.currentGrowthTime += spell.spellPower * 5;
                }

            }
        }

        if (!tile.isWater)
        {

            tile.isWater = true;
            TilemapVisualizer.eraseTile(TilemapVisualizer.natureTilemap, new Vector2Int(tile.position.x, tile.position.y));
            TilemapVisualizer.drawTile(TilemapVisualizer.waterTilemap, new Vector2Int(tile.position.x, tile.position.y), TilemapVisualizer.water);
        }
    }
    public override void EffectOnRoom(RoomInfo room) { }
    public override void EffectOnCreature(CreatureController creature) { }
    public override void EffectOnFaction(FactionBehaviour faction, float duration)
    { 
        spell.SpellEffectCoroutine = spell.StartCoroutine(BlueBless(faction, duration));
    }
    public override void EffectOnBeam(Vector2Int beamTarget) { }
    public override void EffectOnDungeon( float duration) { }
    public override void EffectOnArea(List<TileInfo> area) { }
    
    public IEnumerator BlueBless(FactionBehaviour faction, float duration)
    {
        float timer = duration;
        faction.dungeonFav += spell.spellPower;
        for (int i = 0; i < faction.members.Count; i++)
            {
                CreatureController creature = faction.members[i];
                if (creature != null)
                {
                    creature.agent.speed += (creature.agent.speed/2) * spell.spellPower;
                }
            }
        while (timer > 0)
        {
            timer -= Time.deltaTime;

            yield return new WaitForSeconds(1f);
        }
                for (int i = 0; i < faction.members.Count; i++)
            {
                CreatureController creature = faction.members[i];
                if (creature != null)
                {
                    creature.agent.speed += creature.data.speed;
                }
            }
        yield break;
    }
}
