using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;
using UnityEngine.UIElements;
using UnityEngine.AI;
using NavMeshPlus.Components;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;

public class RoomFirstDungeonGenerator : SimpleRandomWalkDungeonGenerator
{
    public NavMeshSurface surface;
    [SerializeField]
    private int minRoomWidth = 4, minRoomHeight = 4;
    [SerializeField]
    private int dungeonWidth = 20, dungeonHeight = 20;
    [SerializeField]
    [Range(0, 10)]
    private int offset = 1;
    [SerializeField]
    private bool randomWalkRooms = false;
    public int deadEndLength = 5;
    public List<Vector2Int> deadEnds = new List<Vector2Int>();
    [SerializeField]
    public List<Vector2Int> deadEndsBorders = new List<Vector2Int>();
    public List<Vector2Int> floorList = new List<Vector2Int>();
    public HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> water = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> nature = new HashSet<Vector2Int>();
    public List<TileInfo> tilesInfos = new List<TileInfo>();
    public Dictionary<Vector3Int, TileInfo> tileInfoDict = new Dictionary<Vector3Int, TileInfo>();
    private List<RoomInfo> roomsInfos = new List<RoomInfo>();
     public List<GameObject> rooms = new List<GameObject>();

    [SerializeField]
    private GameObject spawnPointPrefab; // Assign a prefab in the Inspector
    [SerializeField]
    public List<GameObject> dgRoomslist = new List<GameObject>();
    public List<GameObject> dgTilesList = new List<GameObject>();
    public List<GameObject> spawnPoints = new List<GameObject>();
    public override void RunProceduralGeneration()
    {
        CreateRooms();
    }

    private void CreateRooms()

    {
        surface.RemoveData();
        ClearData();
        tilemapVisualizer.Clear();
        tilemapVisualizer.Clear();
        var roomsList = ProceduralGenerationAlgorithms.BinarySpacePartitioning(new BoundsInt((Vector3Int)startPosition, new Vector3Int(dungeonWidth, dungeonHeight, 1)), minRoomWidth, minRoomHeight);


        foreach (var room in roomsList)
        {
            foreach (var pos in room.allPositionsWithin)
            {
                Debug.Log($"Room position: {pos}");
            }
        }

        if (randomWalkRooms)
        {
            floor = CreateRoomsRandomly(roomsList);
        }
        else
        {
            floor = CreateSimpleRooms(roomsList);
        }


        List<Vector2Int> roomCenters = new List<Vector2Int>();
        foreach (var room in roomsList)
            roomCenters.Add((Vector2Int)Vector3Int.RoundToInt(room.center));

        List<Vector2Int> corridors = ConnectRooms(roomCenters);
        floor.UnionWith(corridors);

        RegisterFloors(floor);
        CropFloorOnBorder(floor);
        (deadEnds, deadEndsBorders) = FindAllDeadEnds(floor.ToList(), deadEndLength);
        CreateMainRoom();
        water = CreateWaterRandomly(roomsList);
        nature = CreateNatureRandomly(water.ToList());

        tilemapVisualizer.PaintWaterTiles(floor, water);
        tilemapVisualizer.PaintFloorTiles(floor);
        WallGenerator.CreateWalls(floor, tilemapVisualizer);
        NatureGenerator.CreateNature(nature, tilemapVisualizer);
        RegisterTileInfo();
        StartCoroutine(BuildNavMeshAfterDelay());
        RegisterSpawnPointsAndRooms(roomsList);
    }

    private IEnumerator BuildNavMeshAfterDelay()
    {
        yield return new WaitForSeconds(1); // Wait for one frame to ensure tilemap updates are complete
        surface.BuildNavMesh();
    }
    private HashSet<Vector2Int> CreateRoomsRandomly(List<BoundsInt> roomsList)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        for (int i = 0; i < roomsList.Count; i++)
        {
            var roomBounds = roomsList[i];
            var roomCenter = new Vector2Int(Mathf.RoundToInt(roomBounds.center.x), Mathf.RoundToInt(roomBounds.center.y));
            var roomFloor = RunRandomWalk(randomWalkParameters, roomCenter);
            foreach (var position in roomFloor)
            {
                if (position.x >= (roomBounds.xMin + offset) && position.x <= (roomBounds.xMax - offset) && position.y >= (roomBounds.yMin - offset) && position.y <= (roomBounds.yMax - offset))
                {
                    floor.Add(position);
                }
            }
        }
        return floor;
    }

    private HashSet<Vector2Int> CreateWaterRandomly(List<BoundsInt> roomsList)
    {

        for (int i = 0; i < roomsList.Count / 4; i++)
        {
            var roomBounds = roomsList[Random.Range(0, roomsList.Count)];
            var roomCenter = new Vector2Int(Mathf.RoundToInt(roomBounds.center.x), Mathf.RoundToInt(roomBounds.center.y));
            var waterFloor = ProceduralGenerationAlgorithms.SimpleRandomWalk(roomCenter, Random.Range(0, 30));
            foreach (var position in waterFloor)
            {
                if (floor.Contains(position))
                {
                    water.Add(position);
                }
            }
        }
        return water;
    }

    private HashSet<Vector2Int> CreateNatureRandomly(List<Vector2Int> water)
    {
        HashSet<Vector2Int> nature = new HashSet<Vector2Int>();
        foreach (var waterFloor in water)
        {
            var roomCenter = new Vector2Int(Mathf.RoundToInt(waterFloor.x), Mathf.RoundToInt(waterFloor.y));
            var natureFloor = ProceduralGenerationAlgorithms.SimpleRandomWalk(roomCenter, Random.Range(0, 30));
            foreach (var position in natureFloor)
            {
                if (floor.Contains(position) && !water.Contains(position))
                {
                    nature.Add(position);
                }
            }
        }
        return nature;
    }
    private List<Vector2Int> ConnectRooms(List<Vector2Int> roomCenters)
    {
        HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();
        var currentRoomCenter = roomCenters[Random.Range(0, roomCenters.Count)];
        roomCenters.Remove(currentRoomCenter);

        while (roomCenters.Count > 0)
        {
            Vector2Int closest = FindClosestPointTo(currentRoomCenter, roomCenters);
            roomCenters.Remove(closest);
            HashSet<Vector2Int> newCorridor = new HashSet<Vector2Int>(CreateCorridor(currentRoomCenter, closest));
            currentRoomCenter = closest;
            corridors.UnionWith(newCorridor);
        }
        return corridors.ToList();
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

    private Vector2Int FindClosestPointTo(Vector2Int currentRoomCenter, List<Vector2Int> roomCenters)
    {
        Vector2Int closest = Vector2Int.zero;
        float distance = float.MaxValue;
        foreach (var position in roomCenters)
        {
            float currentDistance = Vector2.Distance(position, currentRoomCenter);
            if (currentDistance < distance)
            {
                distance = currentDistance;
                closest = position;
            }
        }
        return closest;
    }

    private HashSet<Vector2Int> CreateSimpleRooms(List<BoundsInt> roomsList)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        foreach (var room in roomsList)
        {
            for (int col = offset; col < room.size.x - offset; col++)
            {
                for (int row = offset; row < room.size.y - offset; row++)
                {
                    Vector2Int position = (Vector2Int)room.min + new Vector2Int(col, row);
                    floor.Add(position);
                }
            }
        }
        return floor;
    }

    private (List<Vector2Int> deadEnds, List<Vector2Int> deadEndsBorders) FindAllDeadEnds(List<Vector2Int> floorPositions, int minLength = 5)
    {
        List<Vector2Int> deadEnds = new List<Vector2Int>();
        List<Vector2Int> deadEndsBorders = new List<Vector2Int>();

        foreach (var position in floorPositions)
        {
            int neighboursCount = 0;
            Vector2Int nextPosition = Vector2Int.zero;

            // Count neighbors and find the direction of the dead end
            foreach (var direction in Direction2D.cardinalDirectionsList)
            {
                var neighborPosition = position + direction;
                if (floorPositions.Contains(neighborPosition))
                {
                    neighboursCount++;
                    nextPosition = neighborPosition; // Store the neighbor position
                }
            }

            // If the position has exactly one neighbor, it might be a dead end
            if (neighboursCount == 1)
            {
                List<Vector2Int> deadEndPath = new List<Vector2Int> { position };
                Vector2Int currentPosition = nextPosition;

                // Trace the dead end path
                while (true)
                {
                    deadEndPath.Add(currentPosition);
                    int currentNeighboursCount = 0;
                    Vector2Int nextStep = Vector2Int.zero;

                    foreach (var direction in Direction2D.cardinalDirectionsList)
                    {
                        var neighbor = currentPosition + direction;
                        if (floorPositions.Contains(neighbor) && !deadEndPath.Contains(neighbor))
                        {
                            currentNeighboursCount++;
                            nextStep = neighbor;
                        }
                    }

                    // If the path continues, move to the next step
                    if (currentNeighboursCount == 1)
                    {
                        currentPosition = nextStep;
                    }
                    else
                    {
                        break; // Stop tracing when the path ends or splits
                    }
                }

                // Check if the dead end path meets the minimum length
                if (deadEndPath.Count >= minLength)
                {
                    if (IsOutOfBounds(position, 10))
                    {
                        deadEndsBorders.Add(position); // Add to border dead ends if out of bounds
                    }
                    else
                    {
                        deadEnds.Add(position); // Add to regular dead ends
                    }
                }
            }
        }

        return (deadEnds, deadEndsBorders);
    }

    private bool IsOutOfBounds(Vector2Int position, int magnitude)
    {
        bool isOutOfBounds = position.x < 0 - magnitude || position.x > dungeonWidth + magnitude || position.y < 0 - magnitude || position.y > dungeonHeight + magnitude;
        return isOutOfBounds;
    }

    private void RegisterFloors(HashSet<Vector2Int> floorPositions)
    {
        

        foreach (var position in floorPositions)
        {
            if (!floorList.Contains(position))
            {
                floorList.Add(position);
            }
        }

    }

    private void CreateMainRoom()
    {
        var room = RunRandomWalk(randomWalkParametersMainRooms, new Vector2Int(dungeonWidth / 2, dungeonHeight / 2));
        room.AddRange(RunRandomWalk(randomWalkParametersMainRooms, new Vector2Int(dungeonWidth / 2, dungeonHeight / 2)));
        room.AddRange(RunRandomWalk(randomWalkParametersMainRooms, new Vector2Int(dungeonWidth / 4, dungeonHeight / 4)));
        room.AddRange(RunRandomWalk(randomWalkParametersMainRooms, new Vector2Int(dungeonWidth * 3 / 4, dungeonHeight * 3 / 4)));
        room.AddRange(RunRandomWalk(randomWalkParametersMainRooms, new Vector2Int(dungeonWidth / 4, dungeonHeight * 3 / 4)));
        room.AddRange(RunRandomWalk(randomWalkParametersMainRooms, new Vector2Int(dungeonWidth * 3 / 4, dungeonHeight / 4)));
        foreach (var pos in room)
        {
            if (!floor.Contains(pos))
            {
                floor.Add(pos);
            }
        }

        var waterfloor = RunRandomWalk(randomWalkParameters, new Vector2Int(dungeonWidth * 3 / 4, dungeonHeight / 4));
        waterfloor.AddRange(RunRandomWalk(randomWalkParameters, new Vector2Int(dungeonWidth / 4, dungeonHeight / 4)));
        waterfloor.AddRange(RunRandomWalk(randomWalkParameters, new Vector2Int(dungeonWidth * 3 / 4, dungeonHeight * 3 / 4)));
        waterfloor.AddRange(RunRandomWalk(randomWalkParameters, new Vector2Int(dungeonWidth / 4, dungeonHeight * 3 / 4)));
        foreach (var pos in waterfloor)
        {
            if (floor.Contains(pos))
            {
                water.Add(pos);
            }
        }
    }

    private void CropFloorOnBorder(HashSet<Vector2Int> floorPositions)
    {
        for (int i = floorPositions.Count - 1; i >= 0; i--)
        {
            var position = floorPositions.ElementAt(i);
            if (IsOutOfBounds(position, 15))
            {
                floor.Remove(position);
            }
        }
    }

    public void RegisterSpawnPointsAndRooms(List<BoundsInt> roomsList)
    {


        foreach (var position in deadEndsBorders)
        {
            // Convert Vector2Int to Vector3 for world position
            Vector3 worldPosition = new Vector2(position.x + 0.5f, position.y + 0.5f);

            // Instantiate the spawn point prefab at the position
            GameObject spawnPoint = Instantiate(spawnPointPrefab, worldPosition, Quaternion.identity);
            spawnPoint.transform.SetParent(transform); // Set the parent to the current object for organization
            spawnPoint.transform.localScale = new Vector3(1, 1, 1); // Reset scale to 1,1,1
                                                                    // Add the Transform of the spawn point to the list
            spawnPoints.Add(spawnPoint);
        }
        for (int i = 0; i < roomsList.Count; i++)
        {
            var roomBounds = roomsList[i];
            List<TileInfo> roomTilesInfos = new List<TileInfo>();
            foreach (Vector3Int pos in roomBounds.allPositionsWithin)
            {

                if (tileInfoDict.TryGetValue(pos, out TileInfo tileInfo))
                {
                    roomTilesInfos.Add(tileInfo);

                }
            }
            var roomInfo = new RoomInfo(new Vector3Int(0,0,0), roomTilesInfos);
            roomsInfos.Add(roomInfo);
            Vector2 roomCenter = new Vector2(roomBounds.position.x + roomBounds.size.x / 2, roomBounds.position.y + roomBounds.size.y / 2);
            GameObject roomObject = new GameObject("Room");
            roomObject.transform.position = new Vector3(roomCenter.x, roomCenter.y, 0);
            rooms.Add(roomObject);
            var roomComponent = roomObject.AddComponent<RoomComponent>();
            roomComponent.roomInfo = roomInfo;

            dgRoomslist.Add(roomObject);
            roomObject.transform.SetParent(transform); // Set the parent to the current object for organization

            foreach (Vector3Int pos in roomBounds.allPositionsWithin)
            {
                if (!tileInfoDict.ContainsKey(pos))
                {
                    Debug.LogWarning($"❌ No TileInfo at room cell {pos}");
                }
            }
        }
    }

    public void RegisterTileInfo()
    {
        

        Tilemap groundTilemap = tilemapVisualizer.floorTilemap;
        BoundsInt bounds = groundTilemap.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            TileBase tile = groundTilemap.GetTile(pos);
            if (tile == null) continue; // Only register real ground tiles

            Vector2Int pos2D = new Vector2Int(pos.x, pos.y); // for containment checks
            bool isFloor = true;
            bool isWater = water.Contains(pos2D);
            bool isNature = nature.Contains(pos2D);
            bool isDeadEnd = deadEnds.Contains(pos2D);
            RoomInfo room = null;

            GameObject tileObject = new GameObject("Tile");
            tileObject.transform.position = groundTilemap.CellToWorld(pos) + new Vector3(0.5f, 0.5f, 0);
            tileObject.transform.SetParent(transform);
            tileObject.layer = 15;

            TileInfo info = new TileInfo(pos, tileObject, room, isFloor, isWater, isNature, isDeadEnd);
            tileObject.AddComponent<TileComponent>().tileInfo = info;
            tileObject.AddComponent<BoxCollider2D>().isTrigger = true;

            tileInfoDict[pos] = info;
            tilesInfos.Add(info);
            dgTilesList.Add(tileObject);

            Debug.Log($"Registered TileInfo at {pos}");
        }

        Debug.Log($"✅ Registered {tileInfoDict.Count} ground tiles.");

        if (tileInfoDict.ContainsKey(new Vector3Int(67, 65, 0)))
        {
            Debug.Log("✅ tileInfoDict contains (67, 65, 0)");
        }
        else
        {
            Debug.LogWarning("❌ tileInfoDict does NOT contain (67, 65, 0)");
        }
    }

    private void ClearData()
    {
        for (int i = dgTilesList.Count - 1; i >= 0; i--)
        {
            DestroyImmediate(dgTilesList[i].gameObject);
        }
        tileInfoDict.Clear();
        tilesInfos.Clear();
        dgTilesList.Clear();

        for (int i = spawnPoints.Count - 1; i >= 0; i--)
        {
            DestroyImmediate(spawnPoints[i].gameObject);
        }
        for (int i = dgRoomslist.Count - 1; i >= 0; i--)
        {
            DestroyImmediate(dgRoomslist[i].gameObject);
        }
        spawnPoints.Clear();
        dgRoomslist.Clear();
        
        floorList.Clear();
    }
}