using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using NavMeshPlus.Components;

public class DungeonGenerator : SimpleRandomWalkDungeonGenerator
{
    public GameObject prefabFaction;
    public static DungeonGenerator Instance;
    public bool genReady;
    public bool navReady;
    public NavMeshSurface surface;

    public Dictionary<Vector3Int, TileInfo> dungeonMap = new();
    public Dictionary<Vector2Int, RoomInfo> roomsMap = new();
    public Dictionary<(Vector2Int from, Vector2Int to), CorridorInfo> corridorsMap = new();

    [SerializeField, Range(3, 10)] private int roomGrid = 5;
    [SerializeField] public int dungeonWidth = 100;
    [SerializeField] public int dungeonHeight = 100;
    [SerializeField] private int minRoomWidth = 4;
    [SerializeField] private int minRoomHeight = 4;
    [SerializeField] private int mainRoomWidth = 10;
    [SerializeField] private int mainRoomHeight = 10;
    [SerializeField, Range(0, 10)] private int offset = 1;

    public float floorcount, wallcount, corridorcount;

    private void Start()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        RunProceduralGeneration();
    }

    public override void RunProceduralGeneration()
    {
        CreateRooms();
    }

    private void CreateRooms()
    {
        genReady = false;
        floorcount = wallcount = corridorcount = 0;

        dungeonMap.Clear();
        roomsMap.Clear();

        // Initialize dungeon map grid
        for (int x = 0; x < dungeonWidth; x++)
        {
            for (int y = 0; y < dungeonHeight; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                dungeonMap[pos] = new TileInfo
                {
                    position = pos,
                    tile = null,
                    isWater = false,
                    isNature = false,
                    isDeadEnd = false
                };
            }
        }

        BoundsInt bounds = new BoundsInt(Vector3Int.zero, new Vector3Int(dungeonWidth, dungeonHeight, 1));
        var roomsList = ProceduralGenerationAlgorithms.CreateUniformGridRooms(roomGrid, roomGrid, dungeonWidth, dungeonHeight);

        CreateRoomsGridAligned(roomsList);
        ConnectRooms();

        var floorPositions = dungeonMap
            .Where(kvp => kvp.Value.isFloor)
            .Select(kvp => new Vector2Int(kvp.Key.x, kvp.Key.y));

        tilemapVisualizer.PaintFloorTiles(floorPositions);

        CreateWaterRandomly(roomsList);
        CreateNatureRandomly();

        NatureDrawer.CreateNature(dungeonMap, tilemapVisualizer);
        tilemapVisualizer.PaintWaterTiles2(dungeonMap);
        WallGen.CreateWalls(dungeonMap, tilemapVisualizer);

        StartCoroutine(BuildNavMeshAfterDelay());
    }

    private void CreateRoomsGridAligned(List<BoundsInt> roomsList)
    {
        int gridCols = roomGrid, gridRows = roomGrid;
        Vector2Int cellSize = new Vector2Int(dungeonWidth / gridCols, dungeonHeight / gridRows);
        roomsMap.Clear();

        foreach (var roomBounds in roomsList)
        {
           
            Vector2Int roomCenter = Vector2Int.RoundToInt(roomBounds.center);
            Vector2Int gridIndex = new Vector2Int(
                Mathf.Clamp(roomCenter.x / cellSize.x, 0, gridCols ),
                Mathf.Clamp(roomCenter.y / cellSize.y, 0, gridRows )
            );

            if (roomsMap.ContainsKey(gridIndex))
                continue;

            List<TileInfo> roomTiles = new();
            HashSet<Vector2Int> randomPath = RunRandomWalk(randomWalkParameters, roomCenter);

            for (int x = roomBounds.xMin + offset; x < roomBounds.xMax - offset; x++)
            {
                for (int y = roomBounds.yMin + offset; y < roomBounds.yMax - offset; y++)
                {
                    Vector3Int pos3D = new Vector3Int(x, y, 0);
                    Vector2Int pos2D = new Vector2Int(x, y);
                    if (randomPath.Contains(pos2D) && dungeonMap.TryGetValue(pos3D, out TileInfo tile))
                    {
                        tile.isFloor = true;
                        roomTiles.Add(tile);
                    }
                }
            }

             RoomInfo room = new RoomInfo(roomBounds);
            roomsMap[gridIndex] = room;
            room.tiles = roomTiles;
            room.index = gridIndex;

            foreach (var tile in roomTiles)
                tile.room = room;

            Debug.Log($"📦 Room at grid {gridIndex}, tiles: {roomTiles.Count}");
        }
    }

    /*private void ConnectRooms()
    {
        HashSet<Vector2Int> corridors = new();
        List<Vector2Int> keys = roomsMap.Keys.ToList();
        Vector2Int current = keys[Random.Range(0, keys.Count)];
        keys.Remove(current);

        while (keys.Count > 0)
        {
            Vector2Int next = FindClosestRoom(current, keys);
            keys.Remove(next);

            List<Vector2Int> corridor = CreateCorridor(current, next);
            foreach (var c in corridor) corridors.Add(c);

            if (roomsMap.TryGetValue(current, out RoomInfo roomA) &&
                roomsMap.TryGetValue(next, out RoomInfo roomB))
            {
                roomA.connectedRooms.Add(roomB);
                roomB.connectedRooms.Add(roomA);
                Debug.Log($"🔗 Connected room {current} <--> {next}");
            }

            current = next;
        }

        foreach (var c in corridors)
        {
            Vector3Int pos = new Vector3Int(c.x, c.y, 0);
            if (dungeonMap.TryGetValue(pos, out TileInfo tile))
            {
                tile.isFloor = true;
                if (tile.room == null) corridorcount++;
            }
        }

        Debug.Log($"🛤️ Corridors marked: {corridorcount}");
    }*/
    private void ConnectRooms()
    {
        HashSet<Vector2Int> allCorridorTiles = new();
        corridorsMap.Clear();

        List<Vector2Int> keys = new List<Vector2Int>(roomsMap.Keys);
        Vector2Int current = keys[Random.Range(0, keys.Count)];
        keys.Remove(current);

        while (keys.Count > 0)
        {
            List<RoomInfo> neighbores = new();
            foreach (var card in Direction2D.cardinalDirectionsList)
                {
                    if (roomsMap.ContainsKey(current + card) && !roomsMap[current + card].connectedRooms.Contains(roomsMap[current]))
                    {
                        neighbores.Add(roomsMap[current + card]);
                    }
                }
             if (neighbores.Count > 0)
            {
                Vector2Int next = neighbores[Random.Range(0, neighbores.Count)].index;
                keys.Remove(next);


                // Get center world positions of the rooms
                Vector2Int start = Vector2Int.RoundToInt(roomsMap[current].roomBounds.center);
                Vector2Int end = Vector2Int.RoundToInt(roomsMap[next].roomBounds.center);

                // Use random walk corridor to create a more organic shape
                Vector2Int direction = Direction2D.GetCardinalDirection(start, end);
                int length = Vector2Int.Distance(start, end) > 0 ? Mathf.RoundToInt(Vector2Int.Distance(start, end)) : 5;
                List<Vector2Int> corridorPath = ProceduralGenerationAlgorithms.RandomWalkCorridor(start, end, length, direction);
                //corridorPath.AddRange(ProceduralGenerationAlgorithms.RandomWalkCorridor2(start, length, direction).corridor);
                CorridorInfo linkingCorridor = new();
                foreach (var tile in corridorPath)
                {
                     var tilePos = new Vector3Int(tile.x, tile.y, 0);
                    if (dungeonMap.TryGetValue(tilePos, out TileInfo corridorTile))
                    {
                        corridorTile.isFloor = true;
                        if (corridorTile.room == null)
                        {
                            linkingCorridor.tiles.Add(corridorTile);
                            corridorTile.corridor = linkingCorridor;
                        }
                    }
                }
                if (roomsMap.TryGetValue(current, out RoomInfo roomA) &&
                    roomsMap.TryGetValue(next, out RoomInfo roomB))
                {
                    roomA.connectedRooms.Add(roomB);
                    roomB.connectedRooms.Add(roomA);
                    Debug.Log($"🔗 Connected room {current} <--> {next}");
                }

                current = next;
            }
            else
            {
                // No unconnected neighbors: pick a new current from keys to continue
                if (keys.Count > 0)
                {
                    current = keys[Random.Range(0, keys.Count)];
                    keys.Remove(current);
                }
            }

            foreach (var room in roomsMap)
            {
                if (room.Value.connectedRooms.Count == 0)
                {
                    List<RoomInfo> neighbors = new();
                    foreach (var card in Direction2D.cardinalDirectionsList)
                    {
                        if (roomsMap.ContainsKey(room.Value.index + card))
                        {
                            neighbors.Add(roomsMap[room.Value.index + card]);
                        }
                    }
                    if (neighbors.Count > 0)
                    {
                        RoomInfo neighbor = neighbors[Random.Range(0, neighbors.Count)];
                        room.Value.connectedRooms.Add(neighbor);
                        neighbor.connectedRooms.Add(room.Value);

                        // Optionally, create a corridor here as well
                        Vector2Int start = Vector2Int.RoundToInt(room.Value.roomBounds.center);
                        Vector2Int end = Vector2Int.RoundToInt(neighbor.roomBounds.center);
                        Vector2Int direction = Direction2D.GetCardinalDirection(start, end);
                        int length = Mathf.RoundToInt(Vector2Int.Distance(start, end));
                        List<Vector2Int> corridorPath = ProceduralGenerationAlgorithms.RandomWalkCorridor(start, end, length, direction);
                        // corridorPath.AddRange(ProceduralGenerationAlgorithms.RandomWalkCorridor2(start, length, direction).corridor);
                        foreach (var tile in corridorPath)
                        {
                            var tilePos = new Vector3Int(tile.x, tile.y, 0);
                            if (dungeonMap.TryGetValue(tilePos, out TileInfo corridorTile))
                            {
                                corridorTile.isFloor = true;
                                if (corridorTile.room == null)
                                {
                                    corridorTile.corridor = new CorridorInfo();
                                    corridorTile.corridor.tiles.Add(corridorTile);
                                }
                            }
                        }
                        Debug.Log($"🔗 Extra connection for room {room.Value.index} to {neighbor.index}");
                    }
                }
            }



            // Apply corridors to dungeon map

                foreach (var pos in allCorridorTiles)
                {
                    var pos3D = new Vector3Int(pos.x, pos.y, 0);
                    if (dungeonMap.TryGetValue(pos3D, out TileInfo tile))
                    {
                        tile.isFloor = true;
                        if (tile.room == null)
                            corridorcount++;
                    }
                }
        }
    }


    private Vector2Int FindClosestRoom(Vector2Int from, List<Vector2Int> others)
    {
        float minDist = float.MaxValue;
        Vector2Int closest = from;

        foreach (var pos in others)
        {
            float dist = Vector2Int.Distance(from, pos);
            if (dist < minDist)
            {
                minDist = dist;
                closest = pos;
            }
        }

        return closest;
    }

    private List<Vector2Int> CreateCorridor(Vector2Int start, Vector2Int end)
    {
        List<Vector2Int> corridor = new();
        Vector2Int dir = end - start;
        Vector2Int pos = start;
        corridor.Add(pos);

        var steps = Mathf.CeilToInt(dir.magnitude * 2);

        corridor.AddRange(ProceduralGenerationAlgorithms.RandomWalkCorridor2(pos, steps, Vector2Int.right).corridor);
        corridor.AddRange(ProceduralGenerationAlgorithms.RandomWalkCorridor2(pos, steps, Vector2Int.left).corridor);
        corridor.AddRange(ProceduralGenerationAlgorithms.RandomWalkCorridor2(pos, steps, Vector2Int.up).corridor);
        corridor.AddRange(ProceduralGenerationAlgorithms.RandomWalkCorridor2(pos, steps, Vector2Int.down).corridor);

        return corridor;
    }

    private void CreateWaterRandomly(List<BoundsInt> roomsList)
    {
        for (int i = 0; i < roomsList.Count / 4; i++)
        {
            var roomBounds = roomsList[Random.Range(0, roomsList.Count)];
            var center = Vector2Int.RoundToInt(roomBounds.center);
            var waterTiles = ProceduralGenerationAlgorithms.SimpleRandomWalk(center, Random.Range(10, 30));

            foreach (var pos in waterTiles)
            {
                Vector3Int p3 = new Vector3Int(pos.x, pos.y, 0);
                if (dungeonMap.TryGetValue(p3, out TileInfo tile) && tile.isFloor)
                    tile.isWater = true;
            }
        }
    }

    private void CreateNatureRandomly()
    {
        var waterTiles = dungeonMap.Where(t => t.Value.isWater).Select(t => t.Key).ToList();

        foreach (var pos in waterTiles)
        {
            var natureFloor = ProceduralGenerationAlgorithms.SimpleRandomWalk(new Vector2Int(pos.x, pos.y), Random.Range(10, 30));
            foreach (var p in natureFloor)
            {
                Vector3Int p3 = new Vector3Int(p.x, p.y, 0);
                if (dungeonMap.TryGetValue(p3, out TileInfo tile) && tile.isFloor && !tile.isWater)
                    tile.isNature = true;
            }
        }
    }

    private IEnumerator BuildNavMeshAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        surface.BuildNavMesh();
        genReady = true;

        yield return new WaitForSeconds(1f);
        RoomInfo room = GetRandomRoom();
        if (room != null && room.tiles.Count > 0)
        {
            TileInfo tile = room.tiles[Random.Range(0, room.tiles.Count)];
            Vector3 pos = new Vector3(tile.position.x + 0.5f, tile.position.y + 0.5f, 0f);
            Instantiate(prefabFaction, pos, Quaternion.identity);
        }

        yield return new WaitForSeconds(1f);
        navReady = true;
    }

    private RoomInfo GetRandomRoom()
    {
        if (roomsMap.Count == 0) return null;
        return roomsMap.Values.ElementAt(Random.Range(0, roomsMap.Count));
    }
}
