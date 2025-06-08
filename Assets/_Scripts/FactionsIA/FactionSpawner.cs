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
                    DungeonGenerator.Instance.roomsMap.TryGetValue(new Vector2Int(1, 1), out room);
                }
                else if (i == 1)
                {
                    DungeonGenerator.Instance.roomsMap.TryGetValue(new Vector2Int(DungeonGenerator.Instance.roomGrid - 2, 1), out room);
                }
                else if (i == 2)
                {
                    DungeonGenerator.Instance.roomsMap.TryGetValue(new Vector2Int(DungeonGenerator.Instance.roomGrid - 2, DungeonGenerator.Instance.roomGrid - 2), out room);
                }
                else
                {
                    DungeonGenerator.Instance.roomsMap.TryGetValue(new Vector2Int(1, DungeonGenerator.Instance.roomGrid - 2), out room);
                }

                GameObject spawner = Instantiate(spawnerPrefab, room.roomBounds.center, Quaternion.identity);
                spawner.name = "Spawner " + i;

                spawners[i] = spawner;
            }
    }
    void Start()
    {
        int countOffactions = DungeonGenerator.Instance.startingfactionCount;
        while (countOffactions > 0)
        {
            RoomInfo factionRoomStart = null;
            switch (countOffactions - 1)
            {
                case 0:
                    factionRoomStart = DungeonGenerator.Instance.roomsMap[new Vector2Int(1, 1)];
                    countOffactions--;
                    break;
                case 1:
                    factionRoomStart = DungeonGenerator.Instance.roomsMap[new Vector2Int(DungeonGenerator.Instance.roomGrid - 2, 1)];
                    countOffactions--;
                    break;
                case 2:
                    factionRoomStart = DungeonGenerator.Instance.roomsMap[new Vector2Int(1, DungeonGenerator.Instance.roomGrid - 2)];
                    countOffactions--;
                    break;
                case 3:
                    factionRoomStart = DungeonGenerator.Instance.roomsMap[new Vector2Int(DungeonGenerator.Instance.roomGrid - 2, DungeonGenerator.Instance.roomGrid - 2)];
                    countOffactions--;
                    break;
                case 4:
                    factionRoomStart = DungeonGenerator.Instance.roomsMap[new Vector2Int(1, Mathf.FloorToInt(DungeonGenerator.Instance.roomGrid / 2f))];
                    countOffactions--;
                    break;
                case 5:
                    factionRoomStart = DungeonGenerator.Instance.roomsMap[new Vector2Int(Mathf.FloorToInt(DungeonGenerator.Instance.roomGrid / 2f), 1)];
                    countOffactions--;
                    break;
                case 6:
                    factionRoomStart = DungeonGenerator.Instance.roomsMap[new Vector2Int(DungeonGenerator.Instance.roomGrid - 2, Mathf.FloorToInt(DungeonGenerator.Instance.roomGrid / 2f))];
                    countOffactions--;
                    break;
                case 7:
                    factionRoomStart = DungeonGenerator.Instance.roomsMap[new Vector2Int(Mathf.FloorToInt(DungeonGenerator.Instance.roomGrid / 2f), DungeonGenerator.Instance.roomGrid - 2)];
                    countOffactions--;
                    break;
            }
            var faction = factionData[Random.Range(0, factionData.Length)];
            var pos = new Vector3(factionRoomStart.roomBounds.center.x, factionRoomStart.roomBounds.center.y, 0);
            var newfaction = SpawnFaction(faction, pos);
            newfaction.currentHQ.Add(factionRoomStart);
            factionRoomStart.faction = newfaction;
            foreach (var tile in factionRoomStart.tiles)
            {
                tile.faction = newfaction;
            }
            newfaction.numberOfHQ++;
        }

        FactionBehaviour wandererFaction = SpawnFaction(factionWanderer, this.transform.position);
        FlaureSpawner.instance.wandererFaction = wandererFaction.GetComponent<FactionBehaviour>();
        FlaureSpawner.instance.wandererFaction.name = "Wanderer Faction";
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    public FactionBehaviour SpawnFaction(FactionData factionData, Vector3 spawnTransform)
    {
        GameObject faction = Instantiate(factionPrefab, spawnTransform, Quaternion.identity);
        faction.SetActive(false);
        var factionIa = faction.GetComponent<FactionBehaviour>();
        factionIa.factionData = factionData;
        faction.name = factionData.factionName;
        factions.Add(faction);
        faction.SetActive(true);
        return factionIa;
    }
}
