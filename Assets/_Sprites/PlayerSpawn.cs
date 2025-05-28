using CrashKonijn.Goap.Runtime;
using UnityEngine;

public class PlayerSpawn : MonoBehaviour
{
    [SerializeField] private TileInspector tileInspector;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Vector3 spawnPosition;
    bool isSpawned = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spawnPosition = new Vector3(DungeonGenerator.Instance.dungeonHeight/2, DungeonGenerator.Instance.dungeonWidth/2, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isSpawned && DungeonGenerator.Instance.navReady)
        {
            GameObject player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            isSpawned = true;
            //tileInspector.player = player;
        }
    }
}
