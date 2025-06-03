
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;
public class FlowerFlaure : FlaureType
{


  public FlowerFlaure(FlaureBehaviour flower) : base(flower)
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
              flaure.currentGrowthTime++;
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
        if (!flaure.currentTile.isNature)
        {
            flaure.currentGrowthTime--;
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
            TileInfo selectedTile = null;
    List<TileInfo> emptyTiles = new List<TileInfo>();
    if (flaure.currentTile.objects.Count < 4)
    {
      emptyTiles.Add(flaure.currentTile);
    }
    for (int dx = -(flaure.currentTile.position.x + flaure.flaureData.range); dx <= flaure.flaureData.range; dx++)
      {
        for (int dy = -(flaure.currentTile.position.x + flaure.flaureData.range); dy < flaure.flaureData.range; dy++)
        {
          Vector3Int checkPos = new Vector3Int(flaure.currentTile.position.x + dx, flaure.currentTile.position.y + dy, 0);
          if (DungeonGenerator.Instance.dungeonMap.TryGetValue(checkPos, out TileInfo tile))
          {
            if (tile.isFloor && tile.objects.Count < 4)
            {
              emptyTiles.Add(tile);
            }
          }
        }
      }
    if (emptyTiles.Count != 0)
    { selectedTile = emptyTiles[Random.Range(0, emptyTiles.Count)];}
    else
    {
      return;
    }
    
    Vector3 spawnPostion = selectedTile.position + new Vector3(Random.Range(0.1f,0.9f),Random.Range(0.1f,0.9f), 0);
    flaure.flaureSpawner.SpawnFlaure(selectedTile, flaure.flaureData, spawnPostion);
    }


   
}
