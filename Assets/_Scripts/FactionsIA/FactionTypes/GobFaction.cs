
using UnityEngine;

public class GobFaction : FactionType
{
    GameObject gobPrefab;
    

    public GobFaction(FactionBehaviour gob) : base(gob)
    {
        
    }

      public override void Enter()
    {
       unitsPrefab = gobPrefab;
    }
    public override void Exit()
    {
        
    }

   public override bool PotencialHQ(RoomInfo room)
    {
        int natureCount = 0;
        int waterCount = 0;

        foreach (var connectedroom in room.connectedRooms) // Assuming 'room.tiles' is a List<TileInfo>
        {
            foreach (TileInfo tile in connectedroom.tiles)
            {
                // Check if the tile is nature or water
                if (tile.isNature) natureCount++;
                if (tile.isWater) waterCount++;
            }
            // Early return if both conditions are met
            if (natureCount >= 3 && waterCount >= 2)
                return true;
        }

        return false;
    }

}
