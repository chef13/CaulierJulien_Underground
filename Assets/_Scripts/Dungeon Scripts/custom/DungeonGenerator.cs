using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;
using UnityEngine.UIElements;
using UnityEngine.AI;
using NavMeshPlus.Components;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class DungeonGenerator : SimpleRandomWalkDungeonGenerator
{
    public static DungeonGenerator Instance;
    [HideInInspector] public bool genReady;
    public NavMeshSurface surface;
    public Dictionary<Vector3Int, TileInfo> dungeonMap = new Dictionary<Vector3Int, TileInfo>();
    public Dictionary<Vector3Int, RoomInfo> roomsMap = new Dictionary<Vector3Int, RoomInfo>();
    [SerializeField] private int dungeonWidth = 20, dungeonHeight = 20;
    [SerializeField] private int minRoomWidth = 4, minRoomHeight = 4;

    [SerializeField] private int mainRoomWidth = 10, mainRoomHeight = 10;
    [SerializeField][Range(0, 10)] private int offset = 1;

    public float floorcount = 0;
    public float tileinfocount = 0;
    public float wallcount = 0;
    public float corridorcount = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void RunProceduralGeneration()
    {
        CreateRooms();
    }
    // Update is called once per frame
    private void CreateRooms()
    {
        genReady = false;
        floorcount = 0;
        tileinfocount = 0;
        wallcount = 0;
        corridorcount = 0;
        dungeonMap.Clear();
        roomsMap.Clear();
        for (int x = 0; x < dungeonWidth; x++)
        {
            for (int y = 0; y < dungeonHeight; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                dungeonMap[pos] = new TileInfo
                {
                    position = new Vector3Int(x, y, 0),
                    tile = null,
                    isWater = false,
                    isNature = false,
                    isDeadEnd = false
                };
            }
        }
        Debug.Log(dungeonMap.Count);

        var roomsList = ProceduralGenerationAlgorithms.BinarySpacePartitioning(new BoundsInt((Vector3Int)startPosition, new Vector3Int(dungeonWidth, dungeonHeight, 1)), minRoomWidth, minRoomHeight);

        CreateRoomsRandomly(roomsList);

        ConnectRooms();

        var floorPositions = dungeonMap
        .Where(kvp => kvp.Value.isFloor)
        .Select(kvp => new Vector2Int(kvp.Key.x, kvp.Key.y));

        tilemapVisualizer.PaintFloorTiles(floorPositions);

        foreach (var c in dungeonMap)
        {
            Vector3Int pos3D = c.Key;
            if (dungeonMap.TryGetValue(pos3D, out TileInfo tile))
            {
                if (tile != null)
                    tileinfocount++;
            }

        }
        Debug.Log($"🟩 Total TileInfo cout is {tileinfocount}");

        CreateWaterRandomly(roomsList);
        CreateNatureRandomly();

        NatureDrawer.CreateNature(dungeonMap, tilemapVisualizer);
        tilemapVisualizer.PaintWaterTiles2(dungeonMap);

        WallGen.CreateWalls(dungeonMap, tilemapVisualizer);
        StartCoroutine(BuildNavMeshAfterDelay());
    }

    private IEnumerator BuildNavMeshAfterDelay()
    {
        yield return new WaitForSeconds(1); // Wait for one frame to ensure tilemap updates are complete
        surface.BuildNavMesh();
        genReady = true;
    }

    private void CreateRoomsRandomly(List<BoundsInt> roomsList)
    {
        // Reserve a central 10x10 area (you can tweak size)

        Vector3Int center = new Vector3Int(dungeonWidth / 2 - mainRoomWidth / 2, dungeonHeight / 2 - mainRoomHeight / 2, 0);
        BoundsInt mainRoomBounds = new BoundsInt(center, new Vector3Int(mainRoomWidth, mainRoomHeight, 1));

        for (int i = 0; i < roomsList.Count; i++)
        {
            var roomBounds = roomsList[i];

            // Skip rooms that overlap the main room area
            if (BoundsOverlap(roomBounds, mainRoomBounds))
                continue;

            var roomCenter = Vector2Int.RoundToInt(roomBounds.center);
            Vector3Int roomCenter3D = new Vector3Int(roomCenter.x, roomCenter.y, 0);
            var roomFloor = RunRandomWalk(randomWalkParameters, roomCenter);
            List<TileInfo> roomTiles = new();

            foreach (var position in roomFloor)
            {
                if (position.x >= roomBounds.xMin + offset &&
                    position.x <= roomBounds.xMax - offset &&
                    position.y >= roomBounds.yMin + offset &&
                    position.y <= roomBounds.yMax - offset)
                {
                    Vector3Int pos3D = new Vector3Int(position.x, position.y, 0);
                    if (dungeonMap.TryGetValue(pos3D, out TileInfo tile))
                    {
                        tile.isFloor = true;
                        roomTiles.Add(tile);
                    }
                }
            }

            RoomInfo roomInfo = new RoomInfo(new Vector3Int(roomCenter.x, roomCenter.y, 0), roomTiles);
            roomsMap[roomCenter3D] = roomInfo;

            foreach (var tile in roomTiles)
            {
                tile.room = roomInfo;
                roomInfo.tiles.Add(tile);
            }

            Debug.Log($"Created room at {roomCenter} with {roomTiles.Count} tiles.");
        }

        CreateMainRoom(mainRoomBounds); // 👈 Create the special main room last

        Debug.Log($"Created {roomsMap.Count} rooms total.");

        int floorCount = 0, wallCount = 0;
        foreach (var tile in dungeonMap.Values)
        {
            if (tile.isFloor) floorCount++;
            else wallCount++;
        }

        Debug.Log($"🟩 Floor Tile count: {floorCount}");
        Debug.Log($"🧱 Wall Tile count: {wallCount}");
    }

    private void CreateMainRoom(BoundsInt mainRoomBounds)
    {
        List<TileInfo> mainRoomTiles = new();

        for (int x = mainRoomBounds.xMin; x < mainRoomBounds.xMax; x++)
        {
            for (int y = mainRoomBounds.yMin; y < mainRoomBounds.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                if (dungeonMap.TryGetValue(pos, out TileInfo tile))
                {
                    tile.isFloor = true;
                    mainRoomTiles.Add(tile);
                }
            }
        }

        RoomInfo mainRoom = new RoomInfo(mainRoomBounds.position, mainRoomTiles);
        roomsMap[new Vector3Int(0, 0, 0 )] = mainRoom;

        foreach (var tile in mainRoomTiles)
        {
            tile.room = mainRoom;
            mainRoom.tiles.Add(tile);
        }

        Debug.Log($"🏰 Main room created at center map with {mainRoom.tiles.Count} tiles.");
    }

    private void ConnectRooms()
    {
        HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();

        List<Vector3Int> roomCenters = new List<Vector3Int>(roomsMap.Keys);
        Vector3Int current = roomCenters[Random.Range(0, roomCenters.Count)];
        roomCenters.Remove(current);

        while (roomCenters.Count > 0)
        {
            Vector3Int next = FindClosestPointTo(current, roomCenters);
            roomCenters.Remove(next);

            List<Vector2Int> corridor = CreateCorridor(new Vector2Int(current.x, current.y), new Vector2Int(next.x, next.y));
            foreach (var tile in corridor)
            {
                corridors.Add(tile);
            }

            // Link rooms in graph
            if (roomsMap.TryGetValue(current, out RoomInfo roomA) &&
                roomsMap.TryGetValue(next, out RoomInfo roomB))
            {
                roomA.connectedRooms.Add(roomB);
                roomB.connectedRooms.Add(roomA);

                if (roomsMap.TryGetValue(current, out roomA) && roomsMap.TryGetValue(next, out roomB))
                {
                    Debug.Log($"🔗 Connected Room at {roomA.position} <--> {roomB.position}");
                }
            }

            current = next;
        }

        // Apply floor flag to corridor tiles
        foreach (var c in corridors)
        {
            Vector3Int pos3D = new Vector3Int(c.x, c.y, 0);
            if (dungeonMap.TryGetValue(pos3D, out TileInfo tile))
            {
                tile.isFloor = true;
                if (tile.room == null)
                    corridorcount++;
            }

        }
        Debug.Log($"🟩 corridor Tile cout is {corridorcount}");
    }

    private Vector3Int FindClosestPointTo(Vector3Int currentRoomCenter, List<Vector3Int> roomCenters)
    {
        Vector3Int closest = Vector3Int.zero;
        float distance = float.MaxValue;
        foreach (var position in roomCenters)
        {
            float currentDistance = Vector3.Distance(position, currentRoomCenter);
            if (currentDistance < distance)
            {
                distance = currentDistance;
                closest = position;
            }
        }
        return closest;
    }

    private List<Vector2Int> CreateCorridor(Vector2Int currentRoomCenter, Vector2Int destination)
    {
        List<Vector2Int> corridor = new List<Vector2Int>();
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int distance = destination - currentRoomCenter;
        var position = currentRoomCenter;
        corridor.Add(position);


        path.AddRange(ProceduralGenerationAlgorithms.RandomWalkCorridor2(position, (int)(distance.magnitude * 2), Vector2Int.right).corridor);
        path.AddRange(ProceduralGenerationAlgorithms.RandomWalkCorridor2(position, (int)(distance.magnitude * 2), Vector2Int.left).corridor);
        path.AddRange(ProceduralGenerationAlgorithms.RandomWalkCorridor2(position, (int)(distance.magnitude * 2), Vector2Int.down).corridor);
        path.AddRange(ProceduralGenerationAlgorithms.RandomWalkCorridor2(position, (int)(distance.magnitude * 2), Vector2Int.up).corridor);
        position = path.Last();
        corridor.AddRange(path);

        return corridor;
    }

    private void CreateWaterRandomly(List<BoundsInt> roomsList)
    {
        for (int i = 0; i < roomsList.Count / 4; i++)
        {
            var roomBounds = roomsList[Random.Range(0, roomsList.Count)];
            var roomCenter = new Vector2Int(Mathf.RoundToInt(roomBounds.center.x), Mathf.RoundToInt(roomBounds.center.y));
            var waterFloor = ProceduralGenerationAlgorithms.SimpleRandomWalk(roomCenter, Random.Range(10, 30));

            foreach (var pos in waterFloor)
            {
                Vector3Int pos3D = new Vector3Int(pos.x, pos.y, 0);
                if (dungeonMap.TryGetValue(pos3D, out TileInfo tile) && tile.isFloor)
                {
                    tile.isWater = true;
                }
            }
        }
    }

    private void CreateNatureRandomly()
    {
        // Get all current water tiles from the dungeon map
        var waterTiles = dungeonMap
            .Where(kvp => kvp.Value.isWater)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var pos in waterTiles)
        {
            var center2D = new Vector2Int(pos.x, pos.y);
            var natureFloor = ProceduralGenerationAlgorithms.SimpleRandomWalk(center2D, Random.Range(10, 30));

            foreach (var p in natureFloor)
            {
                Vector3Int pos3D = new Vector3Int(p.x, p.y, 0);
                if (dungeonMap.TryGetValue(pos3D, out TileInfo tile) &&
                    tile.isFloor && !tile.isWater)
                {
                    tile.isNature = true;
                }
            }
        }
    }


private bool BoundsOverlap(BoundsInt a, BoundsInt b)
{
    return a.xMin < b.xMax && a.xMax > b.xMin &&
           a.yMin < b.yMax && a.yMax > b.yMin;
}

}
