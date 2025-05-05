using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CorridorFirstDungeonGenerator : SimpleRandomWalkDungeonGenerator
{
    [SerializeField]
    private int dungeonWidth = 20, dungeonHeight = 20;
    [SerializeField]
    private int corridorLength = 14, corridorCount = 5;
    [SerializeField]
    [Range(0.1f,1)]
    private float roomPercent = 0.8f;

    protected override void RunProceduralGeneration()
    {
        CorridorFirstGeneration();
    }

    private void CorridorFirstGeneration()
    {
        HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
        HashSet<Vector2Int> potentialRoomPositions = new HashSet<Vector2Int>();

        CreateCorridors(floorPositions, potentialRoomPositions);

        HashSet<Vector2Int> roomPositions = CreateRooms(potentialRoomPositions);

        List<Vector2Int> deadEnds = FindAllDeadEnds(floorPositions);

        CreateRoomsAtDeadEnd(deadEnds, roomPositions);

        floorPositions.UnionWith(roomPositions);

        tilemapVisualizer.PaintFloorTiles(floorPositions);
        WallGenerator.CreateWalls(floorPositions, tilemapVisualizer);

    }

            private void CreateRoomsAtDeadEnd(List<Vector2Int> deadEnds, HashSet<Vector2Int> roomFloors)
        {
            foreach (var position in deadEnds)
            {
                if (!roomFloors.Contains(position) && IsWithinBounds(position))
                {
                    var room = RunRandomWalk(randomWalkParameters, position);
                    roomFloors.UnionWith(room.Where(IsWithinBounds));
                }
            }
        }

        private List<Vector2Int> FindAllDeadEnds(HashSet<Vector2Int> floorPositions)
        {
            List<Vector2Int> deadEnds = new List<Vector2Int>();
            foreach (var position in floorPositions)
            {
                int neighboursCount = 0;
                foreach (var direction in Direction2D.cardinalDirectionsList)
                {
                    var neighborPosition = position + direction;
                    if (floorPositions.Contains(neighborPosition) && IsWithinBounds(neighborPosition))
                        neighboursCount++;
                }
                if (neighboursCount == 1)
                    deadEnds.Add(position);
            }
            return deadEnds;
        }

    private HashSet<Vector2Int> CreateRooms(HashSet<Vector2Int> potentialRoomPositions)
    {
        
        HashSet<Vector2Int> roomPositions = new HashSet<Vector2Int>();
        int roomToCreateCount = Mathf.RoundToInt(potentialRoomPositions.Count * roomPercent);

        List<Vector2Int> roomsToCreate = potentialRoomPositions.OrderBy(x => Guid.NewGuid()).Take(roomToCreateCount).ToList();

        foreach (var roomPosition in roomsToCreate)
        {
            var roomFloor = RunRandomWalk(randomWalkParameters, roomPosition);
            roomPositions.UnionWith(roomFloor);
        }
        return roomPositions;
    }

            private void CreateCorridors(HashSet<Vector2Int> floorPositions, HashSet<Vector2Int> potentialRoomPositions)
        {
            var currentPosition = startPosition;
            for (int i = 0; i < corridorCount; i++)
            {
                var corridor = ProceduralGenerationAlgorithms.BinaryCorridorPartitioning(
                    currentPosition,
                    new BoundsInt((Vector3Int)startPosition, new Vector3Int(dungeonWidth, dungeonHeight, 0)),
                    corridorLength
                );

                currentPosition = new Vector2Int(
                    Mathf.Clamp(Mathf.RoundToInt(corridor[corridor.Count - 1].center.x), 0, dungeonWidth - 1),
                    Mathf.Clamp(Mathf.RoundToInt(corridor[corridor.Count - 1].center.y), 0, dungeonHeight - 1)
                );

                potentialRoomPositions.Add(currentPosition);
                floorPositions.UnionWith(corridor.SelectMany(bounds => ProceduralGenerationAlgorithms.GetPositionsInBounds(bounds).Where(IsWithinBounds)));
            }
        }

                private bool IsWithinBounds(Vector2Int position)
        {
            return position.x >= 0 && position.x < dungeonWidth && position.y >= 0 && position.y < dungeonHeight;
        }
}
