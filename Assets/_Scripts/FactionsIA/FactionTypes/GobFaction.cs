
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

        foreach (var tile in room.tiles) // Assuming 'room.tiles' is a List<TileInfo>
        {
            if (tile.isNature) natureCount++;
            if (tile.isWater) waterCount++;

            // Early return if both conditions are met
            if (natureCount >= 3 && waterCount >= 2)
                return true;
        }

        return false;
    }

}
