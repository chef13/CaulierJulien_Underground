using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;
public class FactionSpawner : MonoBehaviour
{
    FactionSpawner instance;
    private bool init;

    [SerializeField] private FactionData[] factionData;
    [SerializeField] private FactionData factionWanderer;
    public GameObject[] spawners;
    public List<GameObject> factions = new List<GameObject>();
    [SerializeField] private GameObject factionPrefab;
    [SerializeField] private GameObject spawnerPrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Awake()
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


         for (int i = 0; i < spawners.Length; i++)

            {
                RoomInfo room;
                if (i == 0)
                {
                    DungeonGenerator.Instance.roomsMap.TryGetValue(new Vector2Int(0, 0), out room);
                }
                else if (i == 1)
                {
                    DungeonGenerator.Instance.roomsMap.TryGetValue(new Vector2Int(DungeonGenerator.Instance.roomGrid - 1, 0), out room);
                }
                else if (i == 2)
                {
                    DungeonGenerator.Instance.roomsMap.TryGetValue(new Vector2Int(DungeonGenerator.Instance.roomGrid - 1, DungeonGenerator.Instance.roomGrid - 1), out room);
                }
                else
                {
                    DungeonGenerator.Instance.roomsMap.TryGetValue(new Vector2Int(0, DungeonGenerator.Instance.roomGrid - 1), out room);
                }

                GameObject spawner = Instantiate(spawnerPrefab, room.roomBounds.center, Quaternion.identity);
                spawner.name = "Spawner " + i;
                spawners[i] = spawner;
            }
    }
    void Start()
    {
        for (int i = 0; i < spawners.Length; i++)
        {
            Transform spawnerPos = spawners[i].transform;
            var faction = factionData[Random.Range(0, factionData.Length)];
            SpawnFaction(faction, spawnerPos);
        }
        GameObject wandererFaction = SpawnFaction(factionWanderer, this.transform);
        FlaureSpawner.instance.wandererFaction = wandererFaction.GetComponent<FactionBehaviour>();
        FlaureSpawner.instance.wandererFaction.name = "Wanderer Faction";
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    public GameObject SpawnFaction(FactionData factionData, Transform spawnTransform)
    {
        GameObject faction = Instantiate(factionPrefab, spawnTransform.position, Quaternion.identity);
        faction.SetActive(false);
        var factionIa = faction.GetComponent<FactionBehaviour>();
        factionIa.factionData = factionData;
        faction.name = factionData.factionName;
        factions.Add(faction);
        faction.SetActive(true);
        return faction;
    }
}
