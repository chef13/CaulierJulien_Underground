
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;
public class BushFlaure : FlaureType
{


  public BushFlaure(FlaureBehaviour bush) : base(bush)
  {
  }

  public override void Enter()
  {
  }
  public override void Exit()
  {

  }
  public override void Update()
  {
    if (flaure.coroutineDelay >= 0)
        {
            flaure.coroutineDelay -= Time.deltaTime;
        }

  }
    public override void Growing()
  {
    flaure.currentGrowthTime += 2 * flaure.flaureSpawner.growthFactor;
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
          if (tile.objects.Count == 0 && tile.isNature)
          {
            emptyTiles.Add(tile);
          }
        }
      }
    }
    if (emptyTiles.Count == 0)
    {
      Debug.Log("BushFlaure: No empty tiles found for expansion.");
      return;
    }
    TileInfo selectedTile = emptyTiles[Random.Range(0, emptyTiles.Count)];
    Vector3 spawnPostion = selectedTile.position + new Vector3(0.5f, 0.5f, 0);
    flaure.flaureSpawner.spawnQueue.Enqueue((selectedTile, flaure.flaureData, spawnPostion));
    flaure.currentStage = flaure.currentStage - 2;
    flaure.spriteRenderer.sprite = flaure.flaureData.sprites[flaure.currentStage];



  }

  public override void Eaten()
  {
    flaure.currentStage--;
    if (flaure.currentStage < flaure.flaureData.EdibleStage)
    {
      flaure.isEdible = false;
    }
    
     flaure.currentStage = Mathf.Clamp(flaure.currentStage, 0, flaure.flaureData.sprites.Length - 1);
    flaure.spriteRenderer.sprite = flaure.flaureData.sprites[flaure.currentStage];
  }


   
}
