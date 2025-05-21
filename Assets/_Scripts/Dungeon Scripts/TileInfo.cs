using UnityEngine;
using System.Collections.Generic;


public class TileInfo
{
    public Vector3Int position;
    public GameObject tile;
    public RoomInfo room;
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
    public Vector3Int position;
    public List<TileInfo> tiles;
    public string faction;
    public HashSet<RoomInfo> connectedRooms = new HashSet<RoomInfo>();

    public RoomInfo(Vector3Int pos, List<TileInfo> tiles, string factionName = null)
    {
        this.position = pos;
        this.tiles = new List<TileInfo>();
        faction = factionName ?? "None";
    }
}

