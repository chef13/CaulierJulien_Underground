using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AI;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Magic/Effects/BlueBeamEffect")]
public class BlueBeamEffectSO : SpellEffectSO
{
    //public float beamLength = 8f;

    public float waterChance = 0.01f;

    private static HashSet<TileInfo> waterAffectedTiles = new();

public override void ApplyEffect(Spell caster, GameObject target = null, int rangelevel = 1, int powerlevel = 1, int durationlevel = 1)
{
    caster.manaCore.UseMana(caster.manaCost);
    waterAffectedTiles.Clear();

    var dungeon = DungeonGenerator.Instance;
    var tilemapVisualizer = TilemapVisualizer.Instance;

    Vector3 origin = caster.transform.position;
    Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    mouseWorld.z = 0f;

    Vector2 direction = (mouseWorld - origin).normalized;
    float actualLength = caster.beamLength * (rangelevel/2f);

    // Sample along the beam line every 0.5 units
    float stepSize = 1f;
    int steps = Mathf.FloorToInt(actualLength / stepSize);

    HashSet<Vector3Int> visitedCells = new();

    for (int i = 0; i <= steps; i++)
    {
        Vector3 samplePoint = origin + (Vector3)(direction * (i * stepSize));
        Vector3Int cell = tilemapVisualizer.floorTilemap.WorldToCell(samplePoint);

        if (visitedCells.Contains(cell))
            continue;

        visitedCells.Add(cell);

        if (dungeon.dungeonMap.TryGetValue(cell, out TileInfo tile) && tile.isFloor && !tile.isWater)
        {
            float chance = Mathf.Clamp01(waterChance + 0.01f * (powerlevel - 1));
            if (Random.value <= chance)
            {
                caster.colorHandler.EffectOnTile(tile);
            }
        }
    }


    //foreach (var tile in waterAffectedTiles)
    //    dungeon.ModifieTileRunTime(tile);

    tilemapVisualizer.PaintWaterTiles2(dungeon.dungeonMap);
    Debug.Log($"🔵 BlueBeam applied to {waterAffectedTiles.Count} tiles.");
}

}
