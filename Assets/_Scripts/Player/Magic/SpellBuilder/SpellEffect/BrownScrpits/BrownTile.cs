using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Alchemy.Samples;

[CreateAssetMenu(menuName = "Magic/Effects/BrownTileEffect")]
public class BrownTileEffectSO : SpellEffectSO
{
    private static readonly List<Vector2Int> cardinalDirs = Direction2D.cardinalDirectionsList;
    private static HashSet<TileInfo> affectedTiles = new();

    public override void ApplyEffect(Spell caster, GameObject target = null, int rangelevel = 1, int powerlevel = 1, int durationlevel = 1)
    {
        caster.manaCore.UseMana(caster.manaCost);
        affectedTiles.Clear();
        var dungeon = DungeonGenerator.Instance;
        var visualizer = TilemapVisualizer.Instance;

        //Vector3Int baseCell = Vector3Int.FloorToInt(caster.targetTile);
        TryBuildWall(caster.targetTile, caster, powerlevel);

        // Add more walls based on power level (random adjacent tiles)
        for (int i = 0; i < powerlevel - 1; i++)
        {
            Vector2Int randomDir = cardinalDirs[Random.Range(0, cardinalDirs.Count)];
            Vector3Int neighborPos = caster.targetTile.position + new Vector3Int(randomDir.x, randomDir.y, 0);
            dungeon.dungeonMap.TryGetValue(neighborPos, out TileInfo neighborCell);
            TryBuildWall(neighborCell, caster, powerlevel);
        }

        // Repaint surrounding visuals and rebuild navmesh

        visualizer.PaintFloorTiles(affectedTiles.Select(t => new Vector2Int(t.position.x, t.position.y)));
        WallGen.CreateWalls(dungeon.dungeonMap, visualizer);
        dungeon.surface.BuildNavMesh();
    }

    private void TryBuildWall(TileInfo tile, Spell caster, int powerlevel = 1)
    {
        var dungeon = DungeonGenerator.Instance;
        //if (!dungeon.dungeonMap.TryGetValue(cell, out TileInfo tile) || !tile.isFloor) return;
        if (tile == null || !tile.isFloor) return;
        tile.isFloor = false;
        tile.isDeadEnd = false;
        tile.isWater = false;
        tile.isNature = false;
        affectedTiles.Add(tile);

        // Detach from corridor
        if (tile.corridor != null)
        {
            CorridorInfo corridor = tile.corridor;
            corridor.tiles.Remove(tile);
            tile.corridor = null;

            foreach (var room in corridor.connectedRooms.ToList())
            {
                bool stillConnected = corridor.tiles.Any(t =>
                    Direction2D.cardinalDirectionsList.Any(dir =>
                    {
                        Vector3Int neighborPos = t.position + new Vector3Int(dir.x, dir.y, 0);
                        return dungeon.dungeonMap.TryGetValue(neighborPos, out TileInfo nTile) && nTile.room == room;
                    })
                );

                if (!stillConnected)
                {
                    corridor.connectedRooms.Remove(room);
                    room.corridors.Remove(corridor);
                }
            }
        }

        // Detach from creatures, apply push or kill
        if (tile.creatures != null && tile.creatures.Count > 0)
        {
            foreach (var creature in tile.creatures.ToList())
            {
                Vector2 pushDir = (creature.transform.position - tile.position).normalized;
                Vector3 pushTarget = creature.transform.position + (Vector3)(pushDir * 0.5f);
                Vector3Int pushCell = Vector3Int.FloorToInt(pushTarget);

                if (dungeon.dungeonMap.TryGetValue(pushCell, out TileInfo t) && t.isFloor)
                {
                    creature.transform.position = pushTarget;

                    if (creature != null)
                    {
                        ApplyDamage(caster, creature, 15 * powerlevel);
                    }
                }
                else
                {
                    if (creature != null)
                    {
                        ApplyDamage(caster, creature, creature.currentHP);
                    }
                    
                }
            }
        }

        // Do NOT detach from Room (room bounds management assumed)
    }
}
