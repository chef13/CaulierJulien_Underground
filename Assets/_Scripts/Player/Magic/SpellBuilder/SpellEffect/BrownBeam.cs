using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AI;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Magic/Effects/BrownBeamEffect")]
public class BrownBeamEffectSO : SpellEffectSO
{
    public float beamLength = 8f;
    private static HashSet<TileInfo> affectedTiles = new();

    public override void ApplyEffect(Spell caster, GameObject target = null, int rangelevel = 1, int powerlevel = 1, int durationlevel = 1)
    {
        affectedTiles.Clear();

        var dungeon = DungeonGenerator.Instance;
        var tilemapVisualizer = TilemapVisualizer.Instance;
        var navSurface = dungeon.surface;

        Vector3 origin = caster.transform.position;
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Vector2 direction = (mouseWorld - origin).normalized;
        float actualLength = beamLength * (rangelevel/2f);
        float stepSize = 1f;

        int steps = Mathf.FloorToInt(actualLength / stepSize);
        int dugCount = 0;

        for (int i = 0; i <= steps && dugCount < powerlevel; i++)
        {
            Vector3 point = origin + (Vector3)(direction * (i * stepSize));
            Vector3Int cell = tilemapVisualizer.floorTilemap.WorldToCell(point);

            if (dungeon.dungeonMap.TryGetValue(cell, out TileInfo floorTile) && floorTile.isFloor)
            {
                foreach (var creature in floorTile.creatures)
                {
                    if (creature != null && !creature.isDead)
                    {
                        creature.OnHit(null, 10 * powerlevel);
                    }
                }
            }

            if (dungeon.dungeonMap.TryGetValue(cell, out TileInfo tile) && !tile.isFloor)
                {
                    tile.isFloor = true;
                    affectedTiles.Add(tile);
                    dugCount++;
                }
        }

        if (affectedTiles.Count == 0)
        {
            Debug.Log("🟤 BrownBeam: No diggable wall found.");
            return;
        }

        // Apply updates to modified tiles
        foreach (var tile in affectedTiles)
        {
            dungeon.ModifieTileRunTime(tile);
            tilemapVisualizer.EraseWallTile(new Vector2Int(tile.position.x, tile.position.y));
        }

        tilemapVisualizer.PaintFloorTiles(affectedTiles.Select(t => new Vector2Int(t.position.x, t.position.y)));
        WallGen.CreateWalls(dungeon.dungeonMap, tilemapVisualizer);
        NatureDrawer.CreateNature(dungeon.dungeonMap, tilemapVisualizer);
        tilemapVisualizer.PaintWaterTiles2(dungeon.dungeonMap);

        navSurface.BuildNavMesh();
        Debug.Log($"🟤 BrownBeam dug {affectedTiles.Count} tiles.");
    }
}
