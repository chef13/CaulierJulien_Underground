using UnityEngine;
using System.Collections.Generic;
using System.Data;

using System;
public class TileInfo
{
    [HideInInspector]public Vector3Int position;
    [HideInInspector]public List<GameObject> objects = new List<GameObject>();
    [HideInInspector]public List<CreatureController> creatures = new List<CreatureController>();
    [HideInInspector]public RoomInfo room;
    [HideInInspector]public CorridorInfo corridor;
    [HideInInspector]public bool isFloor;
    [HideInInspector]public bool isWater;
    [HideInInspector]public bool isNature;
    [HideInInspector]public bool isDeadEnd;
   
    [HideInInspector]public FactionBehaviour faction;


    public TileInfo() { }
    public TileInfo(Vector3Int pos, RoomInfo room, bool isFloor, bool isWater, bool isNature, bool isDeadEnd)
    {
        this.position = pos;
        objects = new List<GameObject>();
        creatures = new List<CreatureController>();
        this.room = room;
        this.isFloor = isFloor;
        this.isWater = isWater;
        this.isNature = isNature;
        this.isDeadEnd = isDeadEnd;

    }
}
public class RoomInfo
{
    [HideInInspector] public BoundsInt roomBounds;
    [HideInInspector] public Vector2Int index;
    [HideInInspector] public List<TileInfo> tiles;
    [HideInInspector] public FactionBehaviour faction;
    [HideInInspector] public List<CorridorInfo> corridors = new List<CorridorInfo>();

    [HideInInspector] public List<RoomInfo> connectedRooms = new List<RoomInfo>();
    public Vector2Int tileCenter    
    {
        get
        {
            if (tiles.Count == 0) return Vector2Int.zero;
            int x = 0, y = 0;
            foreach (TileInfo tile in tiles)
            {
                x += tile.position.x;
                y += tile.position.y;
            }
            return new Vector2Int(x / tiles.Count, y / tiles.Count);
        }
    }

    public RoomInfo()
    {

        this.tiles = new List<TileInfo>();
        this.connectedRooms = new List<RoomInfo>();
    }
}
public class CorridorInfo
{
    [HideInInspector]public List<TileInfo> tiles;
    [HideInInspector]public List<RoomInfo> connectedRooms = new List<RoomInfo>();
    [HideInInspector]public bool connecting;
    [HideInInspector]public bool deadend;


    public CorridorInfo()
    {
        this.tiles = tiles ?? new List<TileInfo>();
        this.connectedRooms = connectedRooms ?? new List<RoomInfo>();
        if (connectedRooms.Count > 1)
            connecting = true;
    }

}
