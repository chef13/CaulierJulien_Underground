
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;
public class JoncFlaure : FlaureType
{


  public JoncFlaure(FlaureBehaviour jonc) : base(jonc)
  {

  }

  public override void Enter()
  {
  }
  public override void Exit()
  {

  }
    
      public override IEnumerator Grow()
    {
        flaure.currentGrowthTime += 2;
        List<TileInfo> waterTiles = new List<TileInfo>();
        for (int dx = -flaure.flaureData.range; dx <= flaure.flaureData.range; dx++)
        {
            for (int dy = -flaure.flaureData.range; dy <= flaure.flaureData.range; dy++)
            {
            Vector3Int checkPos = new Vector3Int(flaure.currentTile.position.x + dx, flaure.currentTile.position.y + dy, 0);
                if (DungeonGenerator.Instance.dungeonMap.TryGetValue(checkPos, out TileInfo tile))
                {
                  if (tile.objects.Count == 0 && tile.isWater)
                  {
                    waterTiles.Add(tile);
                  }
                }
            }
        }
        if (flaure.currentTile.isWater)
        {
            flaure.currentGrowthTime++;
        }
        if (waterTiles.Count != 0)
        flaure.currentGrowthTime += waterTiles.Count / flaure.flaureData.waterFactor;
        

        if (flaure.currentGrowthTime >= flaure.flaureData.growingTime)
        {
            flaure.currentGrowthTime = 0f;
            if (flaure.currentStage < flaure.flaureData.growthStages)
            {
                flaure.GrowingStage();
            }
            else
            {
                Expand();
            }

        }
        yield return new WaitForSeconds(5f);
        flaure.StartCoroutine(Grow());
    }

    public override void Expand()
    {
         List<TileInfo> emptyTiles = new List<TileInfo>();
    for (int dx = -flaure.flaureData.range; dx <= flaure.flaureData.range; dx++)
    {
      for (int dy = -flaure.flaureData.range; dy < flaure.flaureData.range; dy++)
      {
        Vector3Int checkPos = new Vector3Int(flaure.currentTile.position.x + dx, flaure.currentTile.position.y + dy, 0);
        if (DungeonGenerator.Instance.dungeonMap.TryGetValue(checkPos, out TileInfo tile))
        {
          if ((tile.isNature || tile.isWater) && (tile.objects.Count == 0))
          {
            emptyTiles.Add(tile);
          }
        }
      }
    }
    if (emptyTiles.Count == 0)
    {
      return;
    }
    TileInfo selectedTile = emptyTiles[Random.Range(0, emptyTiles.Count)];
    Vector3 spawnPostion = selectedTile.position + new Vector3(0.5f, 1f, 0);
    flaure.flaureSpawner.SpawnFlaure(selectedTile, flaure.flaureData, spawnPostion);
    }


   
}
