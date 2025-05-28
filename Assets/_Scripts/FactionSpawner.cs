using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;
public class FactionSpawner : MonoBehaviour
{
    private bool init;

    [SerializeField] private FactionData[] factionData;
    public GameObject[] spawners;
    public List<GameObject> factions = new List<GameObject>();
    [SerializeField] private GameObject factionPrefab;
    [SerializeField] private GameObject spawnerPrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < spawners.Length; i++)

            {
                RoomInfo room;
                if (i == 0)
                {
                    DungeonGenerator.Instance.roomsMap.TryGetValue(new Vector2Int(0, 0), out room);
                }
                else if (i==1)
                {DungeonGenerator.Instance.roomsMap.TryGetValue(new Vector2Int(DungeonGenerator.Instance.roomGrid-1, 0), out room);}
                else if (i==2)
                {DungeonGenerator.Instance.roomsMap.TryGetValue(new Vector2Int(DungeonGenerator.Instance.roomGrid -1 , DungeonGenerator.Instance.roomGrid -1), out room);}
                else
                {DungeonGenerator.Instance.roomsMap.TryGetValue(new Vector2Int(0, DungeonGenerator.Instance.roomGrid -1), out room);}
                
                GameObject spawner = Instantiate(spawnerPrefab, room.roomBounds.center, Quaternion.identity);
                spawner.name = "Spawner " + i;
                spawners[i] = spawner;
            }
    }

    // Update is called once per frame
    void Update()
    {
        if (DungeonGenerator.Instance.navReady && !init)
        {
            init = true;
        }

        if (init && factions.Count < 4)
        {
            Transform spawnerPos = spawners[Random.Range(0, spawners.Length)].transform;
            GameObject faction = Instantiate(factionPrefab, spawnerPos.position, Quaternion.identity);
            faction.transform.SetParent(transform);
            factions.Add(faction);
        }
    }
    
    public void SpawnFaction(FactionData factionData, Transform transform)
    {
        GameObject faction = Instantiate(factionPrefab, transform.position, Quaternion.identity);
        faction.GetComponent<FactionBehaviour>().factionData = factionData;
        faction.name = factionData.factionName;
        factions.Add(faction);
    }
}
