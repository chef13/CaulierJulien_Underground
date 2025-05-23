using UnityEngine;
using System.Collections.Generic;
using System.Data;
using UnityEditor.Search;


public class TileInfo
{
    public Vector3Int position;
    public GameObject tile;
    public RoomInfo room;
    public CorridorInfo corridor;
    public bool isFloor;
    public bool isWater;
    public bool isNature;
    public bool isDeadEnd;
   
    public string faction;

     public TileInfo() { }
    public TileInfo(Vector3Int pos, GameObject tileObject, RoomInfo room, bool isFloor, bool isWater, bool isNature, bool isDeadEnd)
    {
        this.position = pos;
        this.tile = tileObject;
        this.room = room;
        this.isFloor = isFloor;
        this.isWater = isWater;
        this.isNature = isNature;
        this.isDeadEnd = isDeadEnd;

    }
}


    public class RoomInfo
{
    public BoundsInt roomBounds;
    public Vector2Int index;
    public List<TileInfo> tiles;
    public string faction;

    public HashSet<RoomInfo> connectedRooms = new HashSet<RoomInfo>();

    public RoomInfo(BoundsInt pos, List<TileInfo> tiles = null, string factionName = null, HashSet<RoomInfo> connect = null)
    {
            this.roomBounds = pos;
            this.tiles = tiles ?? new List<TileInfo>();
            this.faction = factionName ?? "None";
            this.connectedRooms = connect ?? new HashSet<RoomInfo>();
    }
}

public class CorridorInfo
{
    public List<TileInfo> tiles;
    public HashSet<RoomInfo> connectedRooms = new HashSet<RoomInfo>();
    public bool connecting;
    public bool deadend;


    public CorridorInfo()
    {
        this.tiles = tiles ?? new List<TileInfo>();
        this.connectedRooms = connectedRooms ?? new HashSet<RoomInfo>();
        if (connectedRooms.Count > 1)
            connecting = true;
    }

}
