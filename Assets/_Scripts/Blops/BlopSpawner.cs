using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using System.Linq;
using Unity.VisualScripting;

public class BlopSpawner : MonoBehaviour
{
    public Tilemap natureTilemap, waterTilemap;
    public static BlopSpawner Instance;
    public GameObject blopPrefab;
    public List<GameObject> blopList = new List<GameObject>();
    public List<GameObject> blopSpawnedList = new List<GameObject>();
    public Tilemap tilemap;
    public float blopSpawnRate;
    private bool isSpawningBlops = false;
    Vector2 randomPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }


        //StartCoroutine("BlopSpawnCoroutine");


    }

    // Update is called once per frame
    void Update()
    {
        if (DungeonGenerator.Instance.genReady && !isSpawningBlops)
        {
            StartCoroutine(BlopSpawnCoroutine());
            isSpawningBlops = true;
        }
        else if (!DungeonGenerator.Instance.genReady && isSpawningBlops)
        {
            StopCoroutine(BlopSpawnCoroutine());
            isSpawningBlops = false;
        }
    }

    public void SpawnBlop(Vector2 spawnPosition)
    {

        GameObject blop;
        if (blopList.Count > 0)
        {
            blop = blopList[Random.Range(0, blopList.Count)];
            blop.SetActive(true);
            blopSpawnedList.Add(blop);
            blopList.Remove(blop);
            blop.transform.position = spawnPosition;
        }
        else
        {
            blop = Instantiate(blopPrefab, spawnPosition, Quaternion.identity);
            blop.transform.SetParent(transform);
            blop.name = "Blop" + blopSpawnedList.Count;
            blopSpawnedList.Add(blop);
        }

        BlopBehaviour blopBehaviour = blop.GetComponent<BlopBehaviour>();

        Vector3Int cellPosition = tilemap.WorldToCell(spawnPosition);

        if (waterTilemap.GetTile(cellPosition) != null)
        {
            blopBehaviour.SwitchType(new BlueBlop(blopBehaviour));
        }
        else if (natureTilemap.GetTile(cellPosition) != null)
        {
            blopBehaviour.SwitchType(new GreenBlop(blopBehaviour));
        }
        else
        {
            blopBehaviour.SwitchType(new GreyBlop(blopBehaviour));
        }
    }

    IEnumerator BlopSpawnCoroutine()
    {
        ChangeRandomPos();

        SpawnBlop(new Vector2(randomPos.x + 0.5f, randomPos.y + 0.5f));
        yield return new WaitForSeconds(blopSpawnRate);
        StartCoroutine("BlopSpawnCoroutine");

    }

    private void ChangeRandomPos()
    {
         var floorTiles = DungeonGenerator.Instance.dungeonMap
        .Where(kvp => kvp.Value.isFloor)
        .Select(kvp => kvp.Value)
        .ToList();

        if (floorTiles.Count == 0)
        {
            Debug.LogWarning("No floor tiles found.");
            return;
        }

        TileInfo randomTile = floorTiles[Random.Range(0, floorTiles.Count)];
        randomPos = new Vector2(randomTile.position.x + 0.5f, randomTile.position.y + 0.5f);
    }
}
