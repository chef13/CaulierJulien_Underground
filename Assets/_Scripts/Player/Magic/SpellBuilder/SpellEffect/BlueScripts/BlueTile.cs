using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AI;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Magic/Effects/BlueTileEffect")]
public class BlueTileEffectSO : SpellEffectSO
{

    private static readonly List<Vector2Int> cardinalDirs = Direction2D.cardinalDirectionsList;

    public override void ApplyEffect(Spell caster, GameObject target = null, int rangelevel = 1, int powerlevel = 1, int durationlevel = 1)
    {

        caster.manaCore.UseMana(caster.manaCost);
        var dungeon = DungeonGenerator.Instance;
        var visualizer = TilemapVisualizer.Instance;

        //Vector3Int baseCell = Vector3Int.FloorToInt(caster.targetTile);
        caster.colorHandler.EffectOnTile(caster.targetTile);

        // Add more walls based on power level (random adjacent tiles)
        for (int i = 0; i < powerlevel - 1; i++)
        {
            Vector2Int randomDir = cardinalDirs[Random.Range(0, cardinalDirs.Count)];
            Vector3Int neighborPos = caster.targetTile.position + new Vector3Int(randomDir.x, randomDir.y, 0);
            dungeon.dungeonMap.TryGetValue(neighborPos, out TileInfo neighborCell);
            caster.colorHandler.EffectOnTile(neighborCell);
        }

}

}
