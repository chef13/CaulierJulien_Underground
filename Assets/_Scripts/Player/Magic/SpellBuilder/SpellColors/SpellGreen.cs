using UnityEngine;
using System.Collections.Generic;
using System.Collections;
public class SpellGreen : SpellColor
{
    FlaureSpawner flaureSpawner => FlaureSpawner.instance;
    public SpellGreen(Spell spell) : base(spell)
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
        if (!tile.isNature)
        {
            tile.isNature = true;
            TilemapVisualizer.drawTile(TilemapVisualizer.natureTilemap, new Vector2Int(tile.position.x, tile.position.y), TilemapVisualizer.natureTile);
        }

        if (tile.objects.Count > 0 && tile.objects.Count < 4)
        {
            // Remove all objects on the tile
            for (int i = tile.objects.Count - 1; i >= 0; i--)
            {
                GameObject obj = tile.objects[i];
                if (obj == null) continue;

                // If the object is a FlaureBehaviour, increase its growth time
                FlaureBehaviour flaure = obj.GetComponent<FlaureBehaviour>();
                if (flaure != null)
                {
                    
                    if (flaure.currentFlaureType is ChampiFlaure && flaure.currentGrowthTime >= flaure.flaureData.growingTime)
                    {
                        // If the Flaure is a ChampiFlaure and has grown enough, turn it into a living creature
                        flaure.currentFlaureType.TurnLivingCreature(FactionSpawner.dungeonFaction);
                    }
                    else
                        flaure.currentFlaureType.Expand();
                }
            }
            
        }

        if (tile.objects.Count > 0 && tile.objects.Count < 4)
        {
            flaureSpawner.SpawnFlaure(tile, flaureSpawner.champiData, tile.position + new Vector3(Random.Range(0.1f, 0.9f), Random.Range(0.1f, 0.9f), 0));
        }
        else if (tile.objects.Count == 0)
        {

            flaureSpawner.SpawnFlaure(tile, flaureSpawner.joncData, tile.position + new Vector3(0.5f, 0.5f, 0));
        }
    }
    public override void EffectOnRoom(RoomInfo room) { }
    public override void EffectOnCreature(CreatureController creature) { }
    public override void EffectOnFaction(FactionBehaviour faction, float duration)
    {
        spell.SpellEffectCoroutine = spell.StartCoroutine(GreenBless(faction, duration));
    }
    public override void EffectOnBeam(Vector2Int beamTarget) { }
    public override void EffectOnDungeon(float duration) { }
    public override void EffectOnArea(List<TileInfo> area) { }

    public IEnumerator GreenBless(FactionBehaviour faction, float duration)
    {
        float timer = duration;
        faction.dungeonFav += spell.spellPower;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            for (int i = 0; i < faction.members.Count; i++)
            {
                CreatureController creature = faction.members[i];
                if (creature != null)
                {
                    if (creature.currentHP < creature.data.maxLife)
                    creature.currentHP += spell.spellPower;
                    if (creature.currentHunger < creature.data.maxHunger)
                    creature.currentHunger += spell.spellPower * 2;
                }
            }
            yield return new WaitForSeconds(1f);
        }
        yield break;

    }
}
