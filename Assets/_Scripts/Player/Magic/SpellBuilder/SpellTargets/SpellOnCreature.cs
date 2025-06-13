using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;
using CrashKonijn.Goap.Runtime;
public class SpellOnCreature : SpellTarget
{
    private static readonly List<Vector2Int> cardinalDirs = Direction2D.cardinalDirectionsList;
    public TileInfo targetTile;
    List<CreatureController> ennemisCreatures = new List<CreatureController>();
    List<CreatureController> alliesCreatures = new List<CreatureController>();
    List<CreatureController> neutralCreatures = new List<CreatureController>();
    public SpellOnCreature(Spell spell) : base(spell)
    {
    }
    public override IEnumerator CastingSpell(Spell spell)
    {
        var dungeon = DungeonGenerator.Instance;
        var creatureSpawner = CreatureSpawner.Instance;
        var manaCore = spell.manaCore;
        List<CreatureController> CreaturesInRange = new List<CreatureController>();
        List<CreatureController> targetedCreatures = new List<CreatureController>();
        int creatureToTarget = spell.spellPower;

        Vector3 origine = new Vector2(0, 0);
        if (manaCore.currentAvatar != null)
        {
            origine = manaCore.currentAvatar.transform.position;
        }
        else
        {
            origine = manaCore.spellOrigin.position;
        }
        while (true)
        {
            UnhighlightCreature(targetedCreatures);
            CreaturesInRange.Clear();
            targetedCreatures.Clear();
            if (manaCore.avatarController != null)
                manaCore.avatarController.casting = true;
            var moussePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            var distance = Vector2.ClampMagnitude(moussePosition - origine, spell.beamLength * (spell.spellRange / 2f));
            TileInfo tile = dungeon.dungeonMap.TryGetValue(new Vector3Int(Mathf.RoundToInt(origine.x + distance.x), Mathf.RoundToInt(origine.y + distance.y), 0), out tile) ? tile : null;
            if (tile != null)
            {
                targetTile = tile;
                foreach (var creature in tile.creatures)
                {
                    if (creature != null)
                    {
                        CreaturesInRange.Add(creature);
                    }
                }
            }
            for (int h = -spell.spellRange; h <= spell.spellRange; h++)
            {
                for (int v = -spell.spellRange; v <= spell.spellRange; v++)
                {
                    Vector3Int offset = new Vector3Int(h, v, 0);
                    Vector3Int position = tile.position + offset;
                    if (dungeon.dungeonMap.TryGetValue(position, out tile))
                    {
                        foreach (var creature in tile.creatures)
                        {
                            if (creature != null && !CreaturesInRange.Contains(creature))
                            {
                                CreaturesInRange.Add(creature);
                            }
                        }
                    }
                }
            }


            if (CreaturesInRange.Count > 0)
            {
                while (targetedCreatures.Count != creatureToTarget)
                {
                    if (targetedCreatures.Count >= CreaturesInRange.Count)
                    {
                        break; // No more creatures to target
                    }

                    float closestDistance = spell.spellRange * 4f;
                    CreatureController closestCreature = null;
                    foreach (var creature in CreaturesInRange)
                    {
                        if (creature.isDead || targetedCreatures.Contains(creature))
                        {
                            continue; // Skip if creature is null or already targeted
                        }

                        float distanceToCreature = Vector2.Distance(origine, creature.transform.position);
                        if (distanceToCreature < closestDistance)
                        {
                            closestDistance = distanceToCreature;
                            closestCreature = creature;
                        }
                    }
                    targetedCreatures.Add(closestCreature);
                }
            }
             HighlightCreature(targetedCreatures);
            if (Input.GetMouseButtonDown(0))
            {
                if (targetedCreatures.Count == 0)
                {
                    if (manaCore.avatarController != null)
                        manaCore.avatarController.casting = false;
                    manaCore.castingCoroutine = null;
                    yield break; // No creatures targeted, exit the coroutine
                }


                UnhighlightCreature(targetedCreatures);
                if (manaCore.avatarController != null)
                    manaCore.avatarController.casting = false;


                spell.targetedCreatures = targetedCreatures;
                spell.Cast();

                manaCore.castingCoroutine = null;
                 yield break;
            }
            if (Input.GetMouseButtonDown(1))
                {
                    if (targetedCreatures.Count > 0)
                    UnhighlightCreature(targetedCreatures);
                    if (manaCore.avatarController != null)
                    manaCore.avatarController.casting = false;
                    manaCore.castingCoroutine = null;
                    yield break;
                }
            yield return null; 
        }

        
        yield break;
    }

    private void HighlightCreature(List<CreatureController> targetedCreatures)
    {
        foreach (var creature in targetedCreatures)
        {
            if (creature != null)
            {
                if (creature.currentFaction == FactionSpawner.instance.dungeonFaction)
                {
                    Material mat = creature.GetComponent<Renderer>().material;
                    mat.SetColor("_Color", Color.lightGreen);
                    alliesCreatures.Add(creature);
                }
                else if (creature.currentFaction != FactionSpawner.instance.dungeonFaction && creature.currentFaction.dungeonFav > 5)
                {
                    Material mat = creature.GetComponent<Renderer>().material;
                    mat.SetColor("_Color", Color.lightGreen);
                    alliesCreatures.Add(creature);
                }
                else if (creature.currentFaction != FactionSpawner.instance.dungeonFaction && creature.currentFaction.dungeonFav < 5 && creature.currentFaction.dungeonFav > -5)
                {
                    Material mat = creature.GetComponent<Renderer>().material;
                    mat.SetColor("_Color", Color.yellow);
                    neutralCreatures.Add(creature);
                }
                else if (creature.currentFaction != FactionSpawner.instance.dungeonFaction && creature.currentFaction.dungeonFav <= -5)
                {
                    Material mat = creature.GetComponent<Renderer>().material;
                    mat.SetColor("_Color", Color.red);
                    ennemisCreatures.Add(creature);
                }
            }
        }
    }
    

    private void UnhighlightCreature(List<CreatureController> targetedCreatures)
    {
        foreach (var creature in targetedCreatures)
        {
            if (creature != null)
            {
                Material mat = creature.GetComponent<Renderer>().material;
                mat.SetColor("_Color", Color.black);
            }
        }
        ennemisCreatures.Clear();
        alliesCreatures.Clear();
        neutralCreatures.Clear();
    }
}
