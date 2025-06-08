
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;
using Vector2 = UnityEngine.Vector2;
public class ChampiFlaure : FlaureType
{


  public ChampiFlaure(FlaureBehaviour champi) : base(champi)
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

    if (flaure.currentTile.isDeadEnd)
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

    if (flaure.currentTile.isDeadEnd)
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
    int randomSpawn = Random.Range(0, 20);
    if (randomSpawn < flaure.flaureData.spawnChance)
    {
      Vector3 spawnPosition = new Vector2(flaure.currentTile.position.x, flaure.currentTile.position.y);
     // Debug.Log("Spawning creature at " + spawnPosition + " with prefab " + flaure.flaureSpawner.champiPrefab + " and faction " + flaure.flaureSpawner.wandererFaction);
      CreatureSpawner.Instance.StartCoroutine(CreatureSpawner.Instance.SpawnCreatureInRoom(spawnPosition, flaure.flaureSpawner.champiPrefab, flaure.flaureSpawner.wandererFaction));
     // Debug.Log("succes " + spawnPosition);
      Eaten();
      return;
    }
    TileInfo selectedTile = null;
    List<TileInfo> emptyTiles = new List<TileInfo>();
    List<TileInfo> deadEndTiles = new List<TileInfo>();
    if (flaure.currentTile.objects.Count < 4)
    {
      selectedTile = flaure.currentTile;
    }

    else
    {
      for (int dx = -flaure.flaureData.range; dx <= flaure.flaureData.range; dx++)
      {
        for (int dy = -flaure.flaureData.range; dy < flaure.flaureData.range; dy++)
        {
          Vector3Int checkPos = new Vector3Int(flaure.currentTile.position.x + dx, flaure.currentTile.position.y + dy, 0);
          if (DungeonGenerator.Instance.dungeonMap.TryGetValue(checkPos, out TileInfo tile))
          {
            if (tile.isFloor && tile.objects.Count < 4 && !tile.isWater)
            {
              emptyTiles.Add(tile);
            }
            if (tile.isDeadEnd && tile.objects.Count < 4 && !tile.isWater)
            {
              deadEndTiles.Add(tile);
            }
          }
        }
      }
      if (deadEndTiles.Count != 0)
      {
        selectedTile = deadEndTiles[Random.Range(0, deadEndTiles.Count)];
      }
      else if (emptyTiles.Count != 0)
      { selectedTile = emptyTiles[Random.Range(0, emptyTiles.Count)]; }
      else
      {
        return;
      }
    }
    
    Vector3 spawnPostion = selectedTile.position + new Vector3(Random.Range(0.1f,0.9f),Random.Range(0.1f,0.9f), 0);
    flaure.flaureSpawner.spawnQueue.Enqueue((selectedTile, flaure.flaureData, spawnPostion));

          
        
  }
   
}
