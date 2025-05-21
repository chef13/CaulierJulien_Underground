using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class NatureDrawer
{
    public static void CreateNature(Dictionary<Vector3Int, TileInfo> dungeonMap, TilemapVisualizer tilemapVisualizer)
    {
        // Get all nature tile positions as Vector2Int
        HashSet<Vector2Int> naturePositions = dungeonMap
            .Where(kvp => kvp.Value.isNature)
            .Select(kvp => new Vector2Int(kvp.Key.x, kvp.Key.y))
            .ToHashSet();

        foreach (var position in naturePositions)
        {
            string cardinalMask = GetNeighborMask(position, naturePositions, Direction2D.cardinalDirectionsList);
            string cornerMask = GetNeighborMask(position, naturePositions, Direction2D.diagonalDirectionsList);

            tilemapVisualizer.PaintSingleBasicNature(position, cardinalMask);
            tilemapVisualizer.PaintSingleCornerNature(position, cornerMask);
        }
    }

    private static string GetNeighborMask(Vector2Int pos, HashSet<Vector2Int> natureSet, List<Vector2Int> directions)
    {
        string mask = "";

        foreach (var dir in directions)
        {
            Vector2Int neighbor = pos + dir;
            mask += natureSet.Contains(neighbor) ? "1" : "0";
        }

        return mask;
    }


    
}