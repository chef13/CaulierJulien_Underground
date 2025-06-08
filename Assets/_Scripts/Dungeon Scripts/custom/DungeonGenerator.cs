using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using NavMeshPlus.Components;
using Unity.VisualScripting;

public class DungeonGenerator : SimpleRandomWalkDungeonGenerator
{
    [SerializeField] private GameObject prefabFactionGenerator, prefabManaCore;
    [SerializeField] private GameObject prefabFlaureSpawner, prefabCreatureSpawner;
    public static DungeonGenerator Instance;
    public bool genReady;
    public bool navReady;
    public NavMeshSurface surface;
    public int waterFactor = 3;

    public Dictionary<Vector3Int, TileInfo> dungeonMap = new();
    public List<TileInfo> natureTiles = new();
    public List<TileInfo> waterTiles = new();
    public List<TileInfo> deadEndTiles = new();

    public Dictionary<Vector2Int, RoomInfo> roomsMap = new();
    public HashSet<CorridorInfo> corridorsMap = new();

    [Range(3, 10)] public int roomGrid = 5;
    [Range(1, 8)] public int startingfactionCount = 4;
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
                    objects = new List<GameObject>(),
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
        RegisterDeadEndTiles();

        var floorPositions = dungeonMap
            .Where(kvp => kvp.Value.isFloor)
            .Select(kvp => new Vector2Int(kvp.Key.x, kvp.Key.y));

        tilemapVisualizer.PaintFloorTiles(floorPositions);

        CreateWaterRandomly(roomsList);
        CreateNatureRandomly(roomsList);

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
                Mathf.Clamp(roomCenter.x / cellSize.x, 0, gridCols),
                Mathf.Clamp(roomCenter.y / cellSize.y, 0, gridRows)
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
                    else if (!randomPath.Contains(pos2D) && dungeonMap.TryGetValue(pos3D, out TileInfo walltile))
                    {
                        walltile.isFloor = false;
                        roomTiles.Add(walltile);
                    }
                }
            }

            RoomInfo room = new RoomInfo();
            room.roomBounds = roomBounds;
            roomsMap[gridIndex] = room;
            room.tiles = roomTiles;
            room.index = gridIndex;

            foreach (var tile in roomTiles)
                tile.room = room;



            //Debug.Log($"📦 Room at grid {gridIndex}, tiles: {roomTiles.Count}");
        }
    }

    private void RegisterDeadEndTiles()
    {
        for (int x = 0; x < roomGrid; x++)
        {
            for (int y = 0; y < roomGrid; y++)
            {
                RoomInfo currentRoom = roomsMap.TryGetValue(new Vector2Int(x, y), out RoomInfo room) ? room : null;
                for (int i = 0; i < currentRoom.tiles.Count; i++)
                {
                    TileInfo currentTile = currentRoom.tiles[i];
                    List<TileInfo> neighborPos = new List<TileInfo>();
                    for (int h = -1; h <= 1; h += 1)
                    {

                        Vector3Int neighborPos3DY = new Vector3Int(currentRoom.tiles[i].position.x, currentRoom.tiles[i].position.y + h, 0);
                        if (neighborPos3DY != currentTile.position && dungeonMap.TryGetValue(neighborPos3DY, out TileInfo neighborTile))
                        {
                            if (neighborTile.isFloor)
                            {
                                neighborPos.Add(neighborTile);
                            }
                        }


                    }
                    for (int w = -1; w <= 1; w += 1)
                    {
                        Vector3Int neighborPos3DX = new Vector3Int(currentRoom.tiles[i].position.x + w, currentRoom.tiles[i].position.y, 0);
                        if (neighborPos3DX != currentTile.position && dungeonMap.TryGetValue(neighborPos3DX, out TileInfo neighborTile))
                        {
                            if (neighborTile.isFloor)
                            {
                                neighborPos.Add(neighborTile);
                            }
                        }
                    }
                    if (neighborPos.Count == 1 && currentTile.isFloor)
                    {
                        currentRoom.tiles[i].isDeadEnd = true;
                    }
                }
            }
        }
    }
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

                // Determine the direction of the corridor
                bool horizontal = false;
                if (roomsMap[current].index.x == roomsMap[next].index.x)
                    horizontal = false;
                else if (roomsMap[current].index.y == roomsMap[next].index.y)
                    horizontal = true;
                // Randomize position offset for corridor
                int posRandomizer = 0;
                if (horizontal)
                {
                    int halfHeight = roomsMap[current].roomBounds.size.y / 3;
                    posRandomizer = Random.Range(-halfHeight, halfHeight);
                }
                else
                {
                    int halfWidth = roomsMap[current].roomBounds.size.x / 3;
                    posRandomizer = Random.Range(-halfWidth, halfWidth);
                }
                // Get center world positions of the rooms
                Vector2Int start = Vector2Int.RoundToInt(roomsMap[current].roomBounds.center);
                Vector2Int end = Vector2Int.RoundToInt(roomsMap[next].roomBounds.center);
                if (horizontal)
                {
                    start.y += posRandomizer;
                    end.y += posRandomizer;
                }
                else
                {
                    start.x += posRandomizer;
                    end.x += posRandomizer;
                }

                // Use random walk corridor to create a more organic shape
                Vector2Int direction = Direction2D.GetCardinalDirection(start, end);
                //int length = Vector2Int.Distance(start, end) > 0 ? Mathf.RoundToInt(Vector2Int.Distance(start, end)) : 5;
                int length = Mathf.Max(minRoomHeight * 2, Mathf.RoundToInt(Vector2Int.Distance(start, end)));
                List<Vector2Int> corridorPath = ProceduralGenerationAlgorithms.RandomWalkCorridor(start, length, direction);
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
                    corridorsMap.Add(linkingCorridor);
                    linkingCorridor.connectedRooms.Add(roomA);
                    linkingCorridor.connectedRooms.Add(roomB);
                    //Debug.Log($"🔗 Connected room {current} <--> {next}");
                }

                current = next;
            }
            else if (keys.Count > 0)
            {
                // No unconnected neighbors: pick a new current from keys to continue
                current = keys[Random.Range(0, keys.Count)];
                keys.Remove(current);

            }

            foreach (var room in roomsMap)
            {
                if (room.Value.connectedRooms.Count == 0)
                {
                    List<RoomInfo> neighbors = new();
                    //RoomInfo neighbor = null;
                    foreach (var card in Direction2D.cardinalDirectionsList)
                    {
                        if (roomsMap.TryGetValue(room.Value.index + card, out RoomInfo roomB))
                        {
                            neighbors.Add(roomB);
                        }
                    }

                    RoomInfo neighbor = neighbors[Random.Range(0, neighbors.Count)];
                    room.Value.connectedRooms.Add(neighbor);
                    neighbor.connectedRooms.Add(room.Value);
                    CorridorInfo linkingCorridor = new();
                    corridorsMap.Add(linkingCorridor);
                    linkingCorridor.connectedRooms.Add(room.Value);
                    linkingCorridor.connectedRooms.Add(neighbor);
                    // Optionally, create a corridor here as well
                    Vector2Int start = Vector2Int.RoundToInt(room.Value.roomBounds.center);
                    Vector2Int end = Vector2Int.RoundToInt(neighbor.roomBounds.center);
                    Vector2Int direction = Direction2D.GetCardinalDirection(start, end);
                    int length = Mathf.RoundToInt(Vector2Int.Distance(start, end));
                    List<Vector2Int> corridorPath = ProceduralGenerationAlgorithms.RandomWalkCorridor(start, length, direction);
                    // corridorPath.AddRange(ProceduralGenerationAlgorithms.RandomWalkCorridor2(start, length, direction).corridor);
                    foreach (var pos in corridorPath)
                    {
                        var tilePos = new Vector3Int(pos.x, pos.y, 0);
                        if (dungeonMap.TryGetValue(tilePos, out TileInfo corridorTile))
                        {
                            corridorTile.isFloor = true;
                            if (corridorTile.room == null)
                            {
                                corridorTile.corridor = linkingCorridor;
                                linkingCorridor.tiles.Add(corridorTile);
                            }
                        }
                    }
                    //Debug.Log($"🔗 Extra connection for room {room.Value.index} to {neighbor.index}");

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
        for (int i = 0; i < roomsList.Count / waterFactor; i++)
        {
            var roomBounds = roomsList[Random.Range(0, roomsList.Count)];
            var center = Vector2Int.RoundToInt(roomBounds.center);
            var waterTiles = ProceduralGenerationAlgorithms.SimpleRandomWalk(center, Random.Range(10, 30));

            foreach (var pos in waterTiles)
            {
                Vector3Int p3 = new Vector3Int(pos.x, pos.y, 0);
                if (dungeonMap.TryGetValue(p3, out TileInfo tile) && tile.isFloor)
                    tile.isWater = true;
                Instance.waterTiles.Add(tile);
            }
        }

        int cornerHQs = 4;
        while (cornerHQs > 0)
        {
            RoomInfo room = null;
            Vector3Int HQroomCenter = Vector3Int.zero;
            HashSet<Vector2Int> HQwaterTiles = new HashSet<Vector2Int>();
            switch (cornerHQs)
            {
                case 4:
                    roomsMap.TryGetValue(new Vector2Int(1, 1), out room);
                    Debug.Log($"Creating water HQ in room {room.index}");
                    HQroomCenter = room.tiles[Random.Range(0, room.tiles.Count)].position;
                    HQwaterTiles = ProceduralGenerationAlgorithms.SimpleRandomWalk(new Vector2Int(HQroomCenter.x, HQroomCenter.y), Random.Range(10, 30));
                    foreach (var pos in HQwaterTiles)
                    {
                        Vector3Int p3 = new Vector3Int(pos.x, pos.y, 0);
                        if (dungeonMap.TryGetValue(p3, out TileInfo tile) && tile.isFloor)
                            tile.isWater = true;
                        Instance.waterTiles.Add(tile);
                    }
                    cornerHQs--;
                    break;
                case 3:
                    roomsMap.TryGetValue(new Vector2Int(1, roomGrid - 2), out room);
                    Debug.Log($"Creating water HQ in room {room.index}");
                    HQroomCenter = room.tiles[Random.Range(0, room.tiles.Count)].position;
                    HQwaterTiles = ProceduralGenerationAlgorithms.SimpleRandomWalk(new Vector2Int(HQroomCenter.x, HQroomCenter.y), Random.Range(10, 30));
                    foreach (var pos in HQwaterTiles)

                    {
                        Vector3Int p3 = new Vector3Int(pos.x, pos.y, 0);
                        if (dungeonMap.TryGetValue(p3, out TileInfo tile) && tile.isFloor)
                            tile.isWater = true;
                        Instance.waterTiles.Add(tile);
                    }
                    cornerHQs--;
                    break;
                case 2:
                    roomsMap.TryGetValue(new Vector2Int(roomGrid - 2, 1), out room);
                    Debug.Log($"Creating water HQ in room {room.index}");
                    HQroomCenter = room.tiles[Random.Range(0, room.tiles.Count)].position;
                    HQwaterTiles = ProceduralGenerationAlgorithms.SimpleRandomWalk(new Vector2Int(HQroomCenter.x, HQroomCenter.y), Random.Range(10, 30));
                    foreach (var pos in HQwaterTiles)
                    {
                        Vector3Int p3 = new Vector3Int(pos.x, pos.y, 0);
                        if (dungeonMap.TryGetValue(p3, out TileInfo tile) && tile.isFloor)
                            tile.isWater = true;
                        Instance.waterTiles.Add(tile);
                    }
                    cornerHQs--;
                    break;
                case 1:
                    roomsMap.TryGetValue(new Vector2Int(roomGrid - 2, roomGrid - 2), out room);
                    Debug.Log($"Creating water HQ in room {room.index}");
                    HQroomCenter = room.tiles[Random.Range(0, room.tiles.Count)].position;
                    HQwaterTiles = ProceduralGenerationAlgorithms.SimpleRandomWalk(new Vector2Int(HQroomCenter.x, HQroomCenter.y), Random.Range(10, 30));
                    foreach (var pos in HQwaterTiles)
                    {
                        Vector3Int p3 = new Vector3Int(pos.x, pos.y, 0);
                        if (dungeonMap.TryGetValue(p3, out TileInfo tile) && tile.isFloor)
                            tile.isWater = true;
                        Instance.waterTiles.Add(tile);
                    }
                    cornerHQs--;
                    break;
            }
        }
    }

    private void CreateNatureRandomly(List<BoundsInt> roomsList)
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
                Instance.natureTiles.Add(tile);
            }
        }

        foreach (var kvp in roomsMap)
        {
            RoomInfo room = kvp.Value;
            bool hasNature = false;
            foreach (var tile in room.tiles)
            {
                if (tile.isNature)
                {
                    hasNature = true;
                    break;
                }
            }
            if (!hasNature)
            {
                var center = Vector2Int.RoundToInt(room.tileCenter);
                var natureFloor = ProceduralGenerationAlgorithms.SimpleRandomWalk(center, Random.Range(10, 30));
                foreach (var pos in natureFloor)
                {
                    Vector3Int p3 = new Vector3Int(pos.x, pos.y, 0);
                    if (dungeonMap.TryGetValue(p3, out TileInfo tile) && tile.isFloor)
                        tile.isNature = true;
                    Instance.natureTiles.Add(tile);
                }
            }
        }
    }

    private IEnumerator BuildNavMeshAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        surface.BuildNavMesh();
        genReady = true;

        /*yield return new WaitForSeconds(1f);
        RoomInfo room = GetRandomRoom();
        if (room != null && room.tiles.Count > 0)
        {
            TileInfo tile = room.tiles[Random.Range(0, room.tiles.Count)];
            Vector3 pos = new Vector3(tile.position.x + 0.5f, tile.position.y + 0.5f, 0f);
            Instantiate(prefabFaction, pos, Quaternion.identity);
        }*/

        yield return new WaitForSeconds(1f);
        navReady = true;

        yield return new WaitForSeconds(1f);
        GameObject flaurSpawner = Instantiate(prefabFlaureSpawner, transform.position, Quaternion.identity);
        flaurSpawner.name = "Flaure Spawner";
        GameObject factionSpawner = Instantiate(prefabFactionGenerator, transform.position, Quaternion.identity);
        factionSpawner.name = "Faction Spawner";

        GameObject CreatureSpawner = Instantiate(prefabCreatureSpawner, transform.position, Quaternion.identity);
        CreatureSpawner.name = "Creature Spawner";

        GameObject manaCore = Instantiate(prefabManaCore, new Vector2(dungeonHeight / 2, dungeonWidth / 2), Quaternion.identity);
        manaCore.name = "Mana Core";
    }

    private RoomInfo GetRandomRoom()
    {
        if (roomsMap.Count == 0) return null;
        return roomsMap.Values.ElementAt(Random.Range(0, roomsMap.Count));
    }


    public void ModifieTileRunTime(TileInfo tile)
    {
        


            List<CorridorInfo> nearbyCorridors = new();
            RoomInfo nearbyRoom = null;
            int numberOfTilesAround = 0;
            foreach (var dir in Direction2D.cardinalDirectionsList)
            {
                
                Vector3Int neighborPos = tile.position + new Vector3Int(dir.x, dir.y, 0);
                if (dungeonMap.TryGetValue(neighborPos, out TileInfo neighborTile) && neighborTile.isFloor)
                {
                    numberOfTilesAround++;
                    if (neighborTile.room != null)
                    nearbyRoom = neighborTile.room;

                    if (neighborTile.corridor != null && !nearbyCorridors.Contains(neighborTile.corridor))
                        nearbyCorridors.Add(neighborTile.corridor);
                }
            }
            if (numberOfTilesAround < 2)
            {
                tile.isDeadEnd = true; // Mark as dead end if less than 2 neighbors
            }
            else
            {
                tile.isDeadEnd = false; // Reset if more than 2 neighbors
            }

            if (tile.room != null)
            return; // Already in a room — skip

            CorridorInfo corridorToUse = null;

            if (nearbyCorridors.Count == 0)
            {
                // No corridor around — create new one
                corridorToUse = new CorridorInfo();
                corridorsMap.Add(corridorToUse); // Replace logic later if needed
            }
            else
            {
                corridorToUse = nearbyCorridors[0];
                // Merge additional corridors
                for (int i = 1; i < nearbyCorridors.Count; i++)
                {
                    foreach (var t in nearbyCorridors[i].tiles)
                    {
                        t.corridor = corridorToUse;
                        corridorToUse.tiles.Add(t);
                    }
                }
            }

            // Final link and register
            tile.corridor = corridorToUse;
            corridorToUse.tiles.Add(tile);

            if (nearbyRoom != null && !corridorToUse.connectedRooms.Contains(nearbyRoom))
            {
                corridorToUse.connectedRooms.Add(nearbyRoom);
                nearbyRoom.corridors.Add(corridorToUse);
            }
    }

}
