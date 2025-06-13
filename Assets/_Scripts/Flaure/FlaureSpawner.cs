using UnityEngine;
using UnityEngine.Tilemaps;

using Random = UnityEngine.Random;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Rendering;


public class FlaureSpawner : MonoBehaviour
{
    public float coroutineDelay = 2f; // Delay for the growth coroutine
    public int flaureBatchSize = 10;
    public Coroutine growCoroutine, spawnCoroutine;
    public static FlaureSpawner instance;
    public GameObject champiPrefab;
    public FactionBehaviour wandererFaction;
    public FactionBehaviour dungeonFaction;
    public List<FlaureBehaviour> flaureBehaviours = new List<FlaureBehaviour>();
    public Queue<(TileInfo, FlaureData, Vector3)> spawnQueue = new Queue<(TileInfo, FlaureData, Vector3)>();
    public FlaureData champiData, fleureData, joncData, bushData;
    [SerializeField] private GameObject flaurePrefab;

    public int growthFactor = 0;

    private bool init;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        LaucheCoroutine();
        FirstSpawning();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LaucheCoroutine()
    {
        if (growCoroutine == null)
        {
            growCoroutine = StartCoroutine(FlaureGrowthCoroutine());
        }
        if (spawnCoroutine == null)
        {
            spawnCoroutine = StartCoroutine(SpawnFlaureQueueCoroutine());
        }
    }

    private void FirstSpawning()
    {
        for (int rv = 0; rv < DungeonGenerator.Instance.roomGrid; rv++)
        {
            for (int rh = 0; rh < DungeonGenerator.Instance.roomGrid; rh++)
            {
                if (DungeonGenerator.Instance.roomsMap.TryGetValue(new Vector2Int(rv, rh), out RoomInfo currentRoom))
                {
                    for (int i = 0; i < currentRoom.tiles.Count; i++)
                    {

                        TileInfo tileInfo = currentRoom.tiles[i];
                        List<TileInfo> deadEnds = new List<TileInfo>();
                        if (tileInfo.isNature && tileInfo.objects.Count == 0 && i % 4 == 0)
                        {
                            GameObject spawnedFlaur = null;
                            FlaureData dataToUse = null;
                            Vector3 spawnPos = tileInfo.position + new Vector3(0.5f, 0.5f, 0);

                            // Choose flaure type by logic, for example:
                            int typeChoice = Random.Range(0, 2); // 0 = Jonc, 1 = Bush

                            switch (typeChoice)
                            {
                                case 0: // Jonc
                                    dataToUse = joncData;
                                    spawnPos = tileInfo.position + new Vector3(0.5f, 0.5f, 0); // or whatever offset you want
                                    break;
                                case 1: // Bush
                                    dataToUse = bushData;
                                    spawnPos = tileInfo.position + new Vector3(0.5f, 0.5f, 0);
                                    break;
                                    // Add more cases if needed
                            }

                            if (dataToUse != null)
                                spawnedFlaur = SpawnFlaure(tileInfo, dataToUse, spawnPos);

                            if (spawnedFlaur != null)
                            {
                                var flaureBehaviour = spawnedFlaur.GetComponent<FlaureBehaviour>();
                                if (flaureBehaviour != null)
                                {
                                    flaureBehaviour.currentStage = flaureBehaviour.flaureData.growthStages;
                                    flaureBehaviour.spriteRenderer.sprite = flaureBehaviour.flaureData.sprites[flaureBehaviour.currentStage];
                                    if (flaureBehaviour.currentStage >= flaureBehaviour.flaureData.EdibleStage)
                                    {
                                        flaureBehaviour.isEdible = true;
                                    }
                                }
                                else
                                {
                                    Debug.LogError("FlaureBehaviour component missing on spawned flaure!");
                                }
                            }
                            else
                            {
                                Debug.LogWarning("SpawnFlaure returned null. Flaure not spawned.");
                            }
                        }
                        if (tileInfo.isDeadEnd && tileInfo.objects.Count == 0)
                        {
                            deadEnds.Add(tileInfo);
                        }

                        for (int d = 0; d < deadEnds.Count; d++)
                        {
                            if (d % 2 == 0)
                            {
                                tileInfo = deadEnds[d];
                                GameObject spawnedFlaur = SpawnFlaure(tileInfo, champiData, tileInfo.position + new Vector3(Random.Range(0.1f, 0.9f), Random.Range(0.1f, 0.9f), 0));
                                var flaureBehaviour = spawnedFlaur.GetComponent<FlaureBehaviour>();
                                flaureBehaviour.currentStage = flaureBehaviour.flaureData.growthStages;
                                flaureBehaviour.spriteRenderer.sprite = flaureBehaviour.flaureData.sprites[flaureBehaviour.currentStage];
                                if (flaureBehaviour.currentStage >= flaureBehaviour.flaureData.EdibleStage)
                                {
                                    flaureBehaviour.isEdible = true;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public GameObject SpawnFlaure(TileInfo tileInfo, FlaureData data, Vector3 position)
    {
        GameObject newFlaure = null;
        bool checkforpulling = false;
        for (int i = 0; i < flaureBehaviours.Count; i++)
        {
            if (flaureBehaviours[i].isActiveAndEnabled == false)
            {
                newFlaure = flaureBehaviours[i].gameObject;
                FlaureBehaviour flaureBehaviour = newFlaure.GetComponent<FlaureBehaviour>();
                flaureBehaviour.flaureData = data;
                flaureBehaviour.flaureSpawner = this;
                flaureBehaviour.currentTile = tileInfo;
                flaureBehaviour.currentStage = 0;
                flaureBehaviour.currentGrowthTime = 0f;
                newFlaure.transform.position = position;
                newFlaure.SetActive(true);
                newFlaure.name = data.flaureTypeEnum.ToString();
                tileInfo.objects.Add(newFlaure);
                checkforpulling = true;
                return newFlaure;
            }
        }
        if (!checkforpulling)
        {
            newFlaure = Instantiate(flaurePrefab, position, Quaternion.identity);
            FlaureBehaviour flaureBehaviour = newFlaure.GetComponent<FlaureBehaviour>();
            flaureBehaviour.flaureData = data;
            flaureBehaviour.currentTile = tileInfo;
            flaureBehaviour.flaureSpawner = this;
            flaureBehaviour.currentStage = 0;
            flaureBehaviour.currentGrowthTime = 0f;
            flaureBehaviours.Add(flaureBehaviour);
            flaureBehaviour.transform.parent = this.transform;
            tileInfo.objects.Add(newFlaure);
            newFlaure.SetActive(true);
            newFlaure.name = data.flaureTypeEnum.ToString();
            return newFlaure;
        }
        else
        {
            Debug.LogWarning("No FlaureBehaviour available to spawn new Flaure.");
            return null;
        }
    }

    private IEnumerator SpawnFlaureQueueCoroutine()
    {
        while (true)
        {
            if (spawnQueue.Count > 0)
            {
                var (tileInfo, data, position) = spawnQueue.Dequeue();
                SpawnFlaure(tileInfo, data, position);
            }
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator FlaureGrowthCoroutine()
    {
        yield return new WaitForSeconds(5f);
        int index = 0;
        //Debug.Log("CheckCreatureMaster coroutine started");
        while (true)
        {
            if (flaureBehaviours.Count > 0)
            {
                int flaureBatch = flaureBatchSize;
                while (flaureBatch > 0)
                {
                    int checkedCount = 0;
                    bool found = false;
                    while (checkedCount < flaureBehaviours.Count && !found)
                    {
                        var flaureToGrow = flaureBehaviours[index];
                        if (flaureToGrow != null && flaureToGrow.isActiveAndEnabled && flaureToGrow.coroutineDelay <= 0.1f)
                        {
                            flaureToGrow.currentFlaureType.Growing();
                            flaureToGrow.coroutineDelay = coroutineDelay;
                            found = true;
                            index = (index + 1) % flaureBehaviours.Count;
                            break;
                        }
                        index = (index + 1) % flaureBehaviours.Count;
                        checkedCount++;
                    }
                    flaureBatch--;
                }
                yield return new WaitForEndOfFrame();
            }

            yield return new WaitForEndOfFrame();
        }
    }
}
