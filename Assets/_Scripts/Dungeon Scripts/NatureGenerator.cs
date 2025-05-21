using System;
using System.Collections.Generic;
using UnityEngine;

public class NatureGenerator 
{
    public static void CreateNature(HashSet<Vector2Int> naturePositions, TilemapVisualizer tilemapVisualizer)
    {
        var (simpleTiles, cornerTiles) = FindNatureSurrounding(naturePositions, Direction2D.eightDirectionsList);    
        CreateBasicNature(tilemapVisualizer, simpleTiles, naturePositions);
        CreateCornerNature(tilemapVisualizer, cornerTiles, naturePositions);
    }

    private static void CreateCornerNature(TilemapVisualizer tilemapVisualizer, HashSet<Vector2Int> cornerNaturePositions, HashSet<Vector2Int> naturePositions)
    {
        foreach (var position in cornerNaturePositions)
        {
            string neighboursBinaryType = "";
            foreach (var direction in Direction2D.eightDirectionsList)
            {
                var neighbourPosition = position + direction;
                if (naturePositions.Contains(neighbourPosition))
                {
                    neighboursBinaryType += "1";
                }
                else
                {
                    neighboursBinaryType += "0";
                }
            }
            tilemapVisualizer.PaintSingleCornerNature(position, neighboursBinaryType);
        }
    }

    private static void CreateBasicNature(TilemapVisualizer tilemapVisualizer, HashSet<Vector2Int> simpleTiles, HashSet<Vector2Int> naturePositions)
    {
        foreach (var position in simpleTiles)
        {
            string neighboursBinaryType = "";
            foreach (var direction in Direction2D.cardinalDirectionsList)
            {
                var neighbourPosition = position + direction;
                if (naturePositions.Contains(neighbourPosition))
                {
                    neighboursBinaryType += "1";
                }
                else
                {
                    neighboursBinaryType += "0";
                }
            }
            tilemapVisualizer.PaintSingleBasicNature(position, neighboursBinaryType);
        }
    }

    private static (HashSet<Vector2Int> simpleTiles, HashSet<Vector2Int> borderTiles) FindNatureSurrounding(
        HashSet<Vector2Int> naturePositions, List<Vector2Int> directionList)
    {
        HashSet<Vector2Int> simpleTiles = new ();
        
        HashSet<Vector2Int> cornerTiles = new();
        foreach (var position in naturePositions)
        {
            foreach (var direction in directionList)
            {
                var neighbourPosition = position + direction;

                // If the neighbor is not part of the nature positions, it's a border
                if (!naturePositions.Contains(neighbourPosition))
                {
                    cornerTiles.Add(neighbourPosition);
                }
                // Only add to simpleTiles if it's not already in cornerTiles
                else if (!cornerTiles.Contains(neighbourPosition))
                {
                    simpleTiles.Add(neighbourPosition);
                }
            }
        }

        return (simpleTiles, cornerTiles);
    }
}
